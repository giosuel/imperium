#region

using System.Collections.Generic;
using System.Text;
using DunGen;
using Imperium.Core.LevelEditor;
using Imperium.Interface.Common;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using Formatting = Imperium.Util.Formatting;
using Tile = Imperium.Core.LevelEditor.Tile;

#endregion

namespace Imperium.Interface.ComponentManager.Widgets;

public class ComponentPreview : MonoBehaviour
{
    private Camera previewCamera;

    private Transform container;
    private Transform content;
    private RawImage cameraCanvas;

    private TMP_Text descriptionText;

    private static readonly Vector3 previewOrigin = new(-1000, -500, -1000);

    private GameObject currentPreviewObject;
    private readonly Dictionary<string, GameObject> previewCache = [];

    private float mouseOffsetX;
    private float mouseOffsetY;

    private bool rotateAnimation;
    private float rotateAnimationValue;

    private bool mouseInCanvas;

    internal void Init()
    {
        var cameraObj = new GameObject("Imp_TilePreview")
        {
            transform =
            {
                position = previewOrigin - Vector3.forward * 30
            }
        };
        previewCamera = cameraObj.AddComponent<Camera>();
        previewCamera.enabled = true;
        previewCamera.orthographic = true;
        previewCamera.nearClipPlane = 0.01f;
        previewCamera.farClipPlane = 200;
        previewCamera.cullingMask = Imperium.Player.gameplayCamera.cullingMask;
        previewCamera.targetTexture = new RenderTexture(269, 185, 24);

        var hdCameraData = cameraObj.gameObject.AddComponent<HDAdditionalCameraData>();
        hdCameraData.customRenderingSettings = true;
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, false);
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;

        Imperium.InputBindings.StaticMap["Scroll"].performed += OnMouseScroll;

        container = transform.Find("Preview");
        content = transform.Find("Preview/Content");
        descriptionText = transform.Find("Preview/Content/Description").GetComponent<TMP_Text>();
        cameraCanvas = transform.Find("Preview/Content/Canvas").GetComponent<RawImage>();
        cameraCanvas.texture = previewCamera.targetTexture;
        var canvasInteractable = cameraCanvas.gameObject.AddComponent<ImpInteractable>();
        canvasInteractable.onDrag += (_, _, drag) =>
        {
            mouseOffsetX += drag.x * 0.25f;
            mouseOffsetY = Mathf.Clamp(mouseOffsetY - drag.y * 0.25f, -60f, 60f);
        };
        canvasInteractable.onEnter += () => mouseInCanvas = true;
        canvasInteractable.onExit += () => mouseInCanvas = false;

        var cameraLightTop = new GameObject("Top").AddComponent<Light>();
        cameraLightTop.transform.SetParent(cameraObj.transform);
        cameraLightTop.transform.position = previewOrigin - Vector3.forward * 50;
        cameraLightTop.range = 700;
        cameraLightTop.intensity = 22000f;

        var cameraLightLeft = new GameObject("Left").AddComponent<Light>();
        cameraLightLeft.transform.SetParent(cameraObj.transform);
        cameraLightLeft.transform.position = previewOrigin - Vector3.forward * 30 + Vector3.left * 10;
        cameraLightLeft.range = 700;
        cameraLightLeft.intensity = 18000f;

        var cameraLightRight = new GameObject("Right").AddComponent<Light>();
        cameraLightRight.transform.SetParent(cameraObj.transform);
        cameraLightRight.transform.position = previewOrigin - Vector3.forward * 30 + Vector3.right * 10;
        cameraLightRight.range = 700;
        cameraLightRight.intensity = 18000f;

        container.gameObject.SetActive(false);
    }

    private void OnMouseScroll(InputAction.CallbackContext context)
    {
        if (!Imperium.Interface.IsOpen<ComponentManager>() || !mouseInCanvas) return;

        previewCamera.orthographicSize = Mathf.Clamp(
            previewCamera.orthographicSize - context.ReadValue<Vector2>().y * 0.01f,
            1,
            100
        );
    }

    private void OnShowPreview()
    {
        mouseOffsetX = 45;
        mouseOffsetY = 45;

        container.gameObject.SetActive(true);
        if (currentPreviewObject) currentPreviewObject.SetActive(false);
    }

    internal void PreviewTile(Tile tile)
    {
        OnShowPreview();

        var proxy = new TileProxy(tile.Prefab, true, Vector3.up);
        var bounds = proxy.Placement.LocalBounds;
        var radius = bounds.size.magnitude;
        previewCamera.orthographicSize = radius / 2;
        currentPreviewObject = GetPreviewObject(tile.Name, tile.Prefab, bounds.center);

        descriptionText.text = GetTileDescription(proxy);
        rotateAnimation = true;
    }

    internal void PreviewBlocker(Blocker blocker)
    {
        OnShowPreview();

        var offset = new Vector3(0, blocker.Socket.size.y / 2, 0);
        previewCamera.orthographicSize = blocker.Socket.size.y / 2 + 1;
        currentPreviewObject = GetPreviewObject(blocker.Name, blocker.Prefab, offset);

        rotateAnimation = false;
    }

    internal void PreviewConnector(Connector connector)
    {
        OnShowPreview();

        var offset = new Vector3(0, connector.Socket.size.y / 2, 0);
        previewCamera.orthographicSize = connector.Socket.size.y / 2 + 3;
        currentPreviewObject = GetPreviewObject(connector.Name, connector.Prefab, offset);

        rotateAnimation = true;
    }

    private string GetTileDescription(TileProxy tileProxy)
    {
        var sb = new StringBuilder();
        sb.Append(DescriptionLine("Name", tileProxy.PrefabTile.name));
        sb.Append(DescriptionLine("Dimensions", Formatting.FormatVector(tileProxy.Placement.LocalBounds.size)));
        sb.Append(DescriptionLine("Doorways", tileProxy.Doorways.Count.ToString()));

        return sb.ToString();
    }

    private string DescriptionLine(string title, string value)
    {
        return $"{RichText.Bold(title)}: {value}\n";
    }

    private GameObject GetPreviewObject(string objName, GameObject objPrefab, Vector3 offset)
    {
        if (!previewCache.TryGetValue(objName, out var previewObj))
        {
            previewObj = Instantiate(objPrefab, previewOrigin - offset, Quaternion.identity);
            ImpLevelEditor.Utils.SpawnNetworkChildren(previewObj);
            previewCache[objName] = previewObj;
        }

        previewObj.SetActive(true);

        return previewObj;
    }

    private void Update()
    {
        rotateAnimationValue += Time.deltaTime * 10;

        var direction = new Vector3(0, 0, -30.0f);
        var deltaX = rotateAnimation ? mouseOffsetX - rotateAnimationValue : mouseOffsetX;
        var rotation = Quaternion.Euler(mouseOffsetY, deltaX, 0);
        previewCamera.transform.position = previewOrigin + rotation * direction;
        previewCamera.transform.LookAt(previewOrigin);
    }

    internal void OnClose()
    {
        mouseInCanvas = false;
    }
}
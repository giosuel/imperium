using System.Collections.Generic;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.MapUI;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;

namespace Imperium.MonoBehaviours;

public class ImpMap : MonoBehaviour
{
    internal Camera Camera { get; private set; }
    internal MinimapOverlay Minimap { get; private set; }
    internal ImpBinding<HashSet<int>> FloorLevels { get; } = new([]);

    internal readonly ImpBinding<float> CameraClipStart = new(9);
    internal readonly ImpBinding<float> CameraClipEnd = new(50);

    internal static ImpMap Create() => new GameObject("ImpMap").AddComponent<ImpMap>();

    private void Awake()
    {
        var originalMapCam = GameObject.Find("MapCamera").GetComponent<Camera>();

        var cameraMapObject = new GameObject("ImpMap");
        cameraMapObject.transform.SetParent(originalMapCam.transform);
        cameraMapObject.transform.position = originalMapCam.transform.position + Vector3.up * 20f;
        cameraMapObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Camera top-down light
        var light = new GameObject("ImpMapLight").AddComponent<Light>();
        light.transform.SetParent(transform);
        light.transform.position = originalMapCam.transform.position + Vector3.up * 30f;
        light.range = 200f;
        light.intensity = 1000f;
        light.gameObject.layer = LayerMask.NameToLayer("HelmetVisor");

        Camera = cameraMapObject.AddComponent<Camera>();
        Camera.enabled = false;
        Camera.orthographic = true;

        Camera.cullingMask = ImpSettings.Map.CameraLayerMask.Value;
        ImpSettings.Map.CameraLayerMask.onUpdate += value => Camera.cullingMask = value;

        Camera.orthographicSize = ImpSettings.Map.CameraZoom.Value;
        ImpSettings.Map.CameraZoom.onUpdate += value => Camera.orthographicSize = value;

        Camera.farClipPlane = CameraClipEnd.Value;
        CameraClipEnd.onUpdate += value => Camera.farClipPlane = value;

        Camera.nearClipPlane = CameraClipStart.Value;
        CameraClipStart.onUpdate += value => Camera.nearClipPlane = value;

        var hdCameraData = cameraMapObject.AddComponent<HDAdditionalCameraData>();
        hdCameraData.customRenderingSettings = true;
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, false);
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;

        Imperium.IngamePlayerSettings.playerInput.actions.FindAction("SwitchItem").performed += OnMouseScroll;

        Minimap = Instantiate(ImpAssets.MinimapOverlayObject).AddComponent<MinimapOverlay>();
        Minimap.InitializeUI(Imperium.Theme, false);
        Minimap.onOpen += OnMinimapOpen;
        Minimap.onClose += OnMinimapClose;
    }

    private static void OnMouseScroll(InputAction.CallbackContext context)
    {
        if (!Imperium.Interface.Get<MapUI>().IsOpen && !Imperium.InputBindings.BaseMap["Alt"].IsPressed()) return;

        var multiplier = ImpSettings.Map.CameraZoom.Value / 100 * 8;
        ImpSettings.Map.CameraZoom.Set(
            Mathf.Clamp(
                ImpSettings.Map.CameraZoom.Value - context.ReadValue<float>() * multiplier,
                1,
                100
            )
        );
    }

    private void OnMinimapOpen()
    {
        Imperium.Map.Camera.enabled = true;
        Imperium.Map.Camera.rect = Minimap.CameraRect;
    }

    private void OnMinimapClose()
    {
        // Hide the map only when neither the minimap not the map are open
        if (!Imperium.Interface.Get<MapUI>().IsOpen) Camera.enabled = false;
    }
}
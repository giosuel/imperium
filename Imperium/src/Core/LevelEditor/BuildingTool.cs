#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Core.LevelEditor;

public class BuildingTool : MonoBehaviour
{
    private GameObject buildInfoPanel;
    private RectTransform canvasRect;
    private RectTransform panelRect;
    private TMP_Text panelText;

    private LineRenderer viewLine;
    private GameObject hitIndicator;

    private PlacedDungeon dungeon;

    internal void Init(PlacedDungeon placedDungeon)
    {
        dungeon = placedDungeon;

        buildInfoPanel = Instantiate(ImpAssets.BuildInfoPanel, transform);
        canvasRect = buildInfoPanel.GetComponent<RectTransform>();
        panelRect = buildInfoPanel.transform.Find("Container").GetComponent<RectTransform>();
        panelText = buildInfoPanel.transform.Find("Container/Box/Text").GetComponent<TMP_Text>();

        viewLine = ImpGeometry.CreateLine(
            useWorldSpace: true,
            thickness: 0.01f,
            startColor: new Color(0f, 1f, 0.73f),
            endColor: new Color(0f, 1f, 0.73f)
        );
        viewLine.gameObject.SetActive(false);

        hitIndicator = ImpGeometry.CreatePrimitive(PrimitiveType.Sphere, transform, color: new Color(1, 1, 1), 0.05f);
    }

    private readonly Dictionary<int, (Texture[], Color[])> materialsCache = [];
    private readonly Dictionary<int, (Texture, Color)> materialCache = [];

    private MeshRenderer[] selectedObjectRenderers = [];

    private void LateUpdate()
    {
        var camera = Imperium.Freecam.IsFreecamEnabled.Value
            ? Imperium.Freecam.FreecamCamera
            : Imperium.Player.hasBegunSpectating
                ? Imperium.StartOfRound.spectateCamera
                : Imperium.Player.gameplayCamera;
        var activeTexture = camera.activeTexture;
        var viewTransform = camera.transform;

        var playerRay = new Ray(
            viewTransform.position + viewTransform.forward * 0.5f,
            viewTransform.forward
        );
        if (!Physics.Raycast(playerRay, out var hitInfo, 20))
        {
            panelRect.gameObject.SetActive(false);
            ImpGeometry.SetLinePositions(
                viewLine,
                viewTransform.position + viewTransform.right,
                viewTransform.position + viewTransform.forward * 10f
            );
            hitIndicator.transform.position = viewTransform.position + viewTransform.forward * 10f;
            return;
        }

        ImpGeometry.SetLinePositions(
            viewLine,
            viewTransform.position + viewTransform.right,
            hitInfo.point
        );
        hitIndicator.transform.position = hitInfo.point;

        TileProp foundObject = null;

        if (selectedObjectRenderers != null)
        {
            foreach (var meshRenderer in selectedObjectRenderers)
            {
                if (materialCache.TryGetValue(meshRenderer.material.GetInstanceID(), out var materialValue))
                {
                    meshRenderer.material.mainTexture = materialValue.Item1;
                    meshRenderer.material.color = materialValue.Item2;
                }

                if (materialsCache.TryGetValue(meshRenderer.material.GetInstanceID(), out var materialsValue))
                {
                    for (var i = 0; i < meshRenderer.materials.Length; i++)
                    {
                        meshRenderer.materials[i].mainTexture = materialsValue.Item1[i];
                        meshRenderer.materials[i].color = materialsValue.Item2[i];
                    }
                }
            }
        }

        foreach (var tile in dungeon.Tiles)
        {
            foreach (var prop in tile.TileProps.Where(prop => prop.Colliders.Contains(hitInfo.collider)))
            {
                selectedObjectRenderers = prop.MeshRenderers;
                foundObject = prop;
                break;
            }

            if (foundObject != null) break;
        }

        if (foundObject != null)
        {
            if (selectedObjectRenderers != null)
            {
                foreach (var meshRenderer in selectedObjectRenderers)
                {
                    if (!materialCache.ContainsKey(meshRenderer.material.GetInstanceID()))
                    {
                        materialCache[meshRenderer.material.GetInstanceID()] = (
                            meshRenderer.material.mainTexture,
                            meshRenderer.material.color
                        );
                    }

                    if (!materialsCache.ContainsKey(meshRenderer.material.GetInstanceID()))
                    {
                        materialsCache[meshRenderer.material.GetInstanceID()] = (
                            meshRenderer.materials.Select(mat => mat.mainTexture).ToArray(),
                            meshRenderer.materials.Select(mat => mat.color).ToArray()
                        );
                    }

                    meshRenderer.material.mainTexture = null;
                    meshRenderer.material.color = new Color(0.13f, 0.95f, 1f);

                    foreach (var material in meshRenderer.materials)
                    {
                        material.mainTexture = null;
                        material.color = new Color(0.13f, 0.95f, 1f);
                    }
                }
            }

            var screenPosition = camera.WorldToScreenPoint(hitInfo.point);

            var scaleFactorX = activeTexture.width / canvasRect.sizeDelta.x;
            var scaleFactorY = activeTexture.height / canvasRect.sizeDelta.y;

            var positionX = screenPosition.x / scaleFactorX;
            var positionY = screenPosition.y / scaleFactorY;
            panelRect.anchoredPosition = new Vector2(positionX, positionY);
            panelRect.gameObject.SetActive(true);

            panelText.text = "FOUND!";
        }
        else
        {
            panelRect.gameObject.SetActive(false);
        }
    }
}
using System;
using System.Collections.Generic;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.MapUI;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Imperium.Visualizers.MonoBehaviours;

public class ObjectInsight : MonoBehaviour
{
    private Component targetObject;

    private GameObject infoPanelObject;
    private Transform panelContainer;
    private Transform infoPanel;
    private RectTransform infoPanelRect;
    private RectTransform infoPanelCanvasRect;

    private TMP_Text panelObjectName;
    private GameObject panelEntryTemplate;
    private Image deathOverlay;

    private readonly Dictionary<string, ObjectInsightEntry> targetInsightEntries = [];

    internal InsightDefinition<Component> InsightDefinition { get; private set; }

    internal void Init(
        Component target,
        InsightDefinition<Component> definition
    )
    {
        targetObject = target;
        InsightDefinition = definition;

        infoPanelObject = Instantiate(ImpAssets.EntityInfoPanel, transform);
        panelContainer = infoPanelObject.transform.Find("Container");
        infoPanel = panelContainer.Find("Panel");
        infoPanelRect = panelContainer.GetComponent<RectTransform>();
        infoPanelCanvasRect = infoPanelObject.GetComponent<RectTransform>();

        panelObjectName = infoPanel.Find("Name").GetComponent<TMP_Text>();
        deathOverlay = infoPanel.Find("Death").GetComponent<Image>();
        panelEntryTemplate = infoPanel.Find("Template").gameObject;
        panelEntryTemplate.SetActive(false);

        InsightDefinition.Insights.onUpdate += OnInsightsUpdate;
        OnInsightsUpdate(InsightDefinition.Insights.Value);
    }

    /// <summary>
    /// Replaces the current definition with a new one. Used, if a more specific insight was registered.
    /// </summary>
    /// <param name="definition">New insight definition</param>
    internal void UpdateInsightDefinition(InsightDefinition<Component> definition)
    {
        InsightDefinition.Insights.onUpdate -= OnInsightsUpdate;

        InsightDefinition = definition;
        InsightDefinition.Insights.onUpdate += OnInsightsUpdate;
    }

    private void OnInsightsUpdate(Dictionary<string, Func<Component, string>> insights)
    {
        foreach (var (insightName, insightGenerator) in insights)
        {
            if (!targetInsightEntries.TryGetValue(insightName, out var insightEntry))
            {
                targetInsightEntries[insightName] = CreateInsightEntry(insightName, insightGenerator);
                continue;
            }

            insightEntry.Init(insightName, insightGenerator, targetObject);
        }
    }

    private ObjectInsightEntry CreateInsightEntry(string insightName, Func<Component, string> insightGenerator)
    {
        // Remove possible existing entry
        if (targetInsightEntries.TryGetValue(insightName, out var existingInsightEntry))
        {
            Destroy(existingInsightEntry.gameObject);
        }

        var insightEntryObject = Instantiate(panelEntryTemplate, infoPanel);
        insightEntryObject.SetActive(true);
        var insightEntry = insightEntryObject.gameObject.AddComponent<ObjectInsightEntry>();
        insightEntry.Init(insightName, insightGenerator, targetObject);

        return insightEntry;
    }

    private void LateUpdate()
    {
        if (!targetObject)
        {
            Destroy(gameObject);
            return;
        }

        if (!InsightDefinition.VisibilityBinding.Value)
        {
            infoPanelObject.SetActive(false);
            return;
        }

        // Death overlay / disable on death
        if (InsightDefinition.IsDeadGenerator != null && InsightDefinition.IsDeadGenerator(targetObject))
        {
            if (ImpSettings.Visualizations.SSHideInactive.Value) return;
            deathOverlay.gameObject.SetActive(true);
        }
        else
        {
            deathOverlay.gameObject.SetActive(false);
        }

        var camera = Imperium.Freecam.IsFreecamEnabled.Value
            ? Imperium.Freecam.FreecamCamera
            : Imperium.Player.hasBegunSpectating
                ? Imperium.StartOfRound.spectateCamera
                : Imperium.Player.gameplayCamera;

        // Panel placement
        var worldPosition = InsightDefinition.PositionOverride?.Invoke(targetObject) ?? targetObject.transform.position;
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        if (Imperium.Map.Minimap.CameraRect.Contains(screenPosition))
        {
            infoPanelObject.SetActive(false);
            return;
        }

        var playerHasLOS = !Physics.Linecast(
            camera.transform.position, worldPosition,
            StartOfRound.Instance.collidersAndRoomMaskAndDefault
        );

        if (!playerHasLOS && !ImpSettings.Visualizations.SSAlwaysOnTop.Value || screenPosition.z < 0)
        {
            infoPanelObject.SetActive(false);
            return;
        }

        var activeTexture = camera.activeTexture;
        var scaleFactorX = activeTexture.width / infoPanelCanvasRect.sizeDelta.x;
        var scaleFactorY = activeTexture.height / infoPanelCanvasRect.sizeDelta.y;

        var positionX = screenPosition.x / scaleFactorX;
        var positionY = screenPosition.y / scaleFactorY;
        infoPanelRect.anchoredPosition = new Vector2(positionX, positionY);

        // Panel scaling by distance to player
        var panelScaleFactor = ImpSettings.Visualizations.SSOverlayScale.Value;
        if (ImpSettings.Visualizations.SSAutoScale.Value)
        {
            panelScaleFactor *= Math.Clamp(
                5 / Vector3.Distance(camera.transform.position, worldPosition),
                0.01f, 1f
            );
        }

        infoPanelRect.localScale = panelScaleFactor * Vector3.one;

        panelObjectName.text = InsightDefinition.NameGenerator != null
            ? InsightDefinition.NameGenerator(targetObject)
            : targetObject.GetInstanceID().ToString();

        infoPanelObject.SetActive(true);
    }
}
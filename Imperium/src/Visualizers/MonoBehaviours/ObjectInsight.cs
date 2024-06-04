using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types;
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

    private GameObject insightPanelObject;
    private Transform panelContainer;
    private Transform insightPanel;
    private RectTransform insightPanelRect;
    private RectTransform insightPanelCanvasRect;

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

        insightPanelObject = Instantiate(ImpAssets.ObjectInsightPanel, transform);
        panelContainer = insightPanelObject.transform.Find("Container");
        insightPanel = panelContainer.Find("Panel");
        insightPanelRect = panelContainer.GetComponent<RectTransform>();
        insightPanelCanvasRect = insightPanelObject.GetComponent<RectTransform>();

        panelObjectName = insightPanel.Find("Name").GetComponent<TMP_Text>();
        deathOverlay = insightPanel.Find("Death").GetComponent<Image>();
        panelEntryTemplate = insightPanel.Find("Template").gameObject;
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

        // Destroy insights that have been removed
        foreach (var insightName in targetInsightEntries.Keys.ToHashSet().Except(insights.Keys.ToHashSet()))
        {
            Destroy(targetInsightEntries[insightName].gameObject);
            targetInsightEntries.Remove(insightName);
        }
    }

    private ObjectInsightEntry CreateInsightEntry(string insightName, Func<Component, string> insightGenerator)
    {
        // Remove possible existing entry
        if (targetInsightEntries.TryGetValue(insightName, out var existingInsightEntry))
        {
            Destroy(existingInsightEntry.gameObject);
        }

        var insightEntryObject = Instantiate(panelEntryTemplate, insightPanel);
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
            insightPanelObject.SetActive(false);
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
            insightPanelObject.SetActive(false);
            return;
        }

        var playerHasLOS = !Physics.Linecast(
            camera.transform.position, worldPosition,
            StartOfRound.Instance.collidersAndRoomMaskAndDefault
        );

        if (!playerHasLOS && !ImpSettings.Visualizations.SSAlwaysOnTop.Value || screenPosition.z < 0)
        {
            insightPanelObject.SetActive(false);
            return;
        }

        var activeTexture = camera.activeTexture;
        var scaleFactorX = activeTexture.width / insightPanelCanvasRect.sizeDelta.x;
        var scaleFactorY = activeTexture.height / insightPanelCanvasRect.sizeDelta.y;

        var positionX = screenPosition.x / scaleFactorX;
        var positionY = screenPosition.y / scaleFactorY;
        insightPanelRect.anchoredPosition = new Vector2(positionX, positionY);

        // Panel scaling by distance to player
        var panelScaleFactor = ImpSettings.Visualizations.SSOverlayScale.Value;
        if (ImpSettings.Visualizations.SSAutoScale.Value)
        {
            panelScaleFactor *= Math.Clamp(
                5 / Vector3.Distance(camera.transform.position, worldPosition),
                0.01f, 1f
            );
        }

        transform.localScale = Vector3.one;
        insightPanelRect.localScale = panelScaleFactor * Vector3.one;

        panelObjectName.text = InsightDefinition.NameGenerator != null
            ? InsightDefinition.NameGenerator(targetObject)
            : targetObject.GetInstanceID().ToString();

        insightPanelObject.SetActive(true);
    }
}
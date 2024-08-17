#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Visualizers.Objects;

public class ObjectInsight : MonoBehaviour
{
    private Component targetObject;

    private GameObject insightPanelObject;
    private Transform panelContainer;
    private Transform insightPanel;
    private RectTransform insightPanelRect;
    private RectTransform insightPanelCanvasRect;

    private TMP_Text panelObjectName;
    private TMP_Text panelObjectPersonalName;
    private GameObject panelEntryTemplate;
    private Image deathOverlay;

    private readonly Dictionary<string, ObjectInsightEntry> targetInsightEntries = [];

    private readonly ImpTimer overlayUpdateTimer = ImpTimer.ForInterval(1);

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
        panelObjectPersonalName = insightPanel.Find("PersonalName").GetComponent<TMP_Text>();
        deathOverlay = insightPanel.Find("Death").GetComponent<Image>();
        panelEntryTemplate = insightPanel.Find("Template").gameObject;
        panelEntryTemplate.SetActive(false);

        InsightDefinition.Insights.onUpdate += OnInsightsUpdate;
        OnInsightsUpdate(InsightDefinition.Insights.Value);

        UpdateInsightOverlay();
    }

    /// <summary>
    ///     Replaces the current definition with a new one. Used, if a more specific insight was registered.
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

    /// <summary>
    ///     Executes the <see cref="InsightDefinition{T}.NameGenerator" /> and <see cref="InsightDefinition.IsDeadGenerator" />
    ///     functions.
    ///     Since these functions are provided by the client, they are only executed every so often
    ///     (<see cref="overlayUpdateTimer" />) for performance reasons.
    /// </summary>
    private void UpdateInsightOverlay()
    {
        // Insight panel type name
        panelObjectName.text = InsightDefinition.NameGenerator != null
            ? InsightDefinition.NameGenerator(targetObject)
            : targetObject.GetInstanceID().ToString();

        // Insight panel name
        if (InsightDefinition.PersonalNameGenerator == null)
        {
            panelObjectPersonalName.gameObject.SetActive(false);
        }
        else
        {
            panelObjectPersonalName.text = InsightDefinition.PersonalNameGenerator(targetObject);
            panelObjectPersonalName.gameObject.SetActive(true);
        }

        // Death overlay / disable on death
        if (InsightDefinition.IsDeadGenerator != null && InsightDefinition.IsDeadGenerator(targetObject))
        {
            if (Imperium.Settings.Visualization.SSHideInactive.Value) return;
            deathOverlay.gameObject.SetActive(true);
        }
        else
        {
            deathOverlay.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!targetObject || !insightPanelCanvasRect)
        {
            Destroy(gameObject);
            return;
        }

        var camera = Imperium.Freecam.IsFreecamEnabled.Value
            ? Imperium.Freecam.FreecamCamera
            : Imperium.Player.hasBegunSpectating
                ? Imperium.StartOfRound.spectateCamera
                : Imperium.Player.gameplayCamera;
        var activeCameraTexture = camera?.activeTexture;

        if (!InsightDefinition.VisibilityBinding.Value || !activeCameraTexture)
        {
            insightPanelObject.SetActive(false);
            return;
        }

        var worldPosition = InsightDefinition.PositionOverride?.Invoke(targetObject) ?? targetObject.transform.position;
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        // Disable rendering if panel is outside of camera view
        if (Imperium.Map.Minimap.CameraRect.Contains(screenPosition))
        {
            insightPanelObject.SetActive(false);
            return;
        }

        // Disable rendering if player doesn't have LOS and LOS is required
        var playerHasLOS = !Physics.Linecast(
            camera.transform.position, worldPosition,
            StartOfRound.Instance.collidersAndRoomMaskAndDefault
        );

        if (!playerHasLOS && !Imperium.Settings.Visualization.SSAlwaysOnTop.Value || screenPosition.z < 0)
        {
            insightPanelObject.SetActive(false);
            return;
        }

        // Update insight overlay values if timer is up
        if (overlayUpdateTimer.Tick()) UpdateInsightOverlay();

        // Insight overlay panel placement
        var scaleFactorX = activeCameraTexture.width / insightPanelCanvasRect.sizeDelta.x;
        var scaleFactorY = activeCameraTexture.height / insightPanelCanvasRect.sizeDelta.y;

        var positionX = screenPosition.x / scaleFactorX;
        var positionY = screenPosition.y / scaleFactorY;
        insightPanelRect.anchoredPosition = new Vector2(positionX, positionY);

        // Panel scaling by distance to player
        var panelScaleFactor = Imperium.Settings.Visualization.SSOverlayScale.Value;
        if (Imperium.Settings.Visualization.SSAutoScale.Value)
        {
            panelScaleFactor *= Math.Clamp(
                5 / Vector3.Distance(camera.transform.position, worldPosition),
                0.01f, 1f
            );
        }

        transform.localScale = Vector3.one;
        insightPanelRect.localScale = panelScaleFactor * Vector3.one;
        insightPanelObject.SetActive(true);
    }
}
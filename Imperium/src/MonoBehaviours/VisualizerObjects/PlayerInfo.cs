#region

using System;
using System.Globalization;
using GameNetcodeStuff;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects;

public class PlayerInfo : MonoBehaviour
{
    private PlayerControllerB playerController;

    private GameObject infoPanel;
    private RectTransform infoPanelRect;
    private RectTransform infoPanelCanvasRect;

    private TMP_Text nameText;
    private TMP_Text healthText;
    private TMP_Text threatText;
    private TMP_Text visibilityText;
    private TMP_Text staminaText;
    private TMP_Text weightText;
    private TMP_Text locationText;
    private Image deathOverlay;

    private PlayerInfoConfig playerInfoConfig;

    internal void Init(PlayerInfoConfig config, PlayerControllerB player)
    {
        playerInfoConfig = config;
        playerController = player;

        InitInfoPanel();
    }

    private void InitInfoPanel()
    {
        infoPanel = Instantiate(ImpAssets.PlayerInfoPanel, transform);
        infoPanelRect = infoPanel.transform.Find("Panel").GetComponent<RectTransform>();
        infoPanelCanvasRect = infoPanel.GetComponent<RectTransform>();

        deathOverlay = infoPanel.transform.Find("Panel/Death").GetComponent<Image>();

        nameText = infoPanel.transform.Find("Panel/Name").GetComponent<TMP_Text>();
        healthText = infoPanel.transform.Find("Panel/Health/Value").GetComponent<TMP_Text>();
        threatText = infoPanel.transform.Find("Panel/Threat/Value").GetComponent<TMP_Text>();
        visibilityText = infoPanel.transform.Find("Panel/Visibility/Value").GetComponent<TMP_Text>();
        staminaText = infoPanel.transform.Find("Panel/Stamina/Value").GetComponent<TMP_Text>();
        weightText = infoPanel.transform.Find("Panel/Weight/Value").GetComponent<TMP_Text>();
        locationText = infoPanel.transform.Find("Panel/Location/Value").GetComponent<TMP_Text>();

        infoPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!playerController) return;

        DrawInfoPanel(playerInfoConfig.Info.Value);
    }

    private void DrawInfoPanel(bool isShown)
    {
        if (!isShown)
        {
            infoPanel.SetActive(false);
            return;
        }

        // Death overlay / disable on death
        if (playerController.isPlayerDead)
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
        var worldPosition = playerController.gameplayCamera.transform.position + Vector3.up;
        var screenPosition = camera.WorldToScreenPoint(worldPosition);

        var playerHasLOS = !Physics.Linecast(
            camera.transform.position, worldPosition,
            StartOfRound.Instance.collidersAndRoomMaskAndDefault
        );

        if ((!playerHasLOS && !ImpSettings.Visualizations.SSAlwaysOnTop.Value) || screenPosition.z < 0)
        {
            infoPanel.SetActive(false);
            return;
        }

        var activeTexture = camera.activeTexture;
        var scaleFactor = activeTexture.width / infoPanelCanvasRect.sizeDelta.x;

        var positionX = screenPosition.x / scaleFactor;
        var positionY = screenPosition.y / scaleFactor;
        infoPanelRect.anchoredPosition = new Vector2(positionX, positionY);

        // Panel scaling
        var panelScaleFactor = ImpSettings.Visualizations.SSOverlayScale.Value;
        if (ImpSettings.Visualizations.SSAutoScale.Value)
        {
            panelScaleFactor *= Math.Clamp(
                5 / Vector3.Distance(camera.transform.position, worldPosition),
                0.01f, 1.5f
            );
        }
        infoPanelRect.localScale = panelScaleFactor * Vector3.one;

        var playerThreat = (IVisibleThreat)playerController;

        healthText.text = playerController.health.ToString();
        nameText.text = playerController.playerUsername;
        threatText.text = playerThreat.GetThreatLevel(Imperium.Player.gameplayCamera.transform.position).ToString();
        visibilityText.text = Math.Round(playerThreat.GetVisibility(), 2).ToString(CultureInfo.InvariantCulture);
        staminaText.text = Math.Round(playerController.sprintMeter, 2).ToString(CultureInfo.InvariantCulture);
        weightText.text = $"{Mathf.RoundToInt((playerController.carryWeight - 1) * 105)}lb";
        locationText.text = PlayerManager.GetLocationText(Imperium.Player, locationOnly: true);

        infoPanel.SetActive(true);
    }
}

internal class PlayerInfoConfig
{
    internal readonly string playerName;

    internal readonly ImpBinding<bool> Info;

    internal PlayerInfoConfig(string playerName)
    {
        this.playerName = playerName;
        Info = new ImpBinding<bool>(false);
    }
}
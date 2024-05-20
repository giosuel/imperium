#region

using System;
using System.Globalization;
using GameNetcodeStuff;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects;

public class PlayerInfo : MonoBehaviour
{
    public PlayerControllerB playerController;

    private GameObject canvas;
    private TMP_Text nameText;
    private TMP_Text healthText;
    private TMP_Text threatText;
    private TMP_Text visibilityText;
    private TMP_Text staminaText;
    private TMP_Text weightText;

    private void Awake()
    {
        canvas = transform.Find("Canvas").gameObject;
        nameText = transform.Find("Canvas/Name").GetComponent<TMP_Text>();
        healthText = transform.Find("Canvas/Health/Value").GetComponent<TMP_Text>();
        threatText = transform.Find("Canvas/Threat/Value").GetComponent<TMP_Text>();
        visibilityText = transform.Find("Canvas/Visibility/Value").GetComponent<TMP_Text>();
        staminaText = transform.Find("Canvas/Stamina/Value").GetComponent<TMP_Text>();
        weightText = transform.Find("Canvas/Weight/Value").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        canvas.SetActive(playerController);
        if (!playerController) return;

        healthText.text = playerController.health.ToString();

        nameText.text = playerController.playerUsername;

        threatText.text = ((IVisibleThreat)playerController)
            .GetThreatLevel(Imperium.Player.gameplayCamera.transform.position).ToString();
        visibilityText.text = Math.Round(((IVisibleThreat)playerController)
            .GetVisibility(), 2).ToString(CultureInfo.InvariantCulture);
        staminaText.text = Math.Round(playerController.sprintMeter, 2).ToString(CultureInfo.InvariantCulture);
        weightText.text =
            $"{Mathf.RoundToInt((playerController.carryWeight - 1) * 105).ToString(CultureInfo.InvariantCulture)}lb";

        canvas.transform.LookAt(Imperium.Freecam.IsFreecamEnabled.Value
            ? Imperium.Freecam.transform.position
            : Imperium.Player.gameplayCamera.transform.position);
    }
}
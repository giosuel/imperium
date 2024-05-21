#region

using System;
using System.Collections.Generic;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects;

public class EntityInfo : MonoBehaviour
{
    private EnemyAI entityController;

    private GameObject infoPanel;
    private RectTransform infoPanelRect;
    private RectTransform infoPanelCanvasRect;

    private TMP_Text nameText;
    private TMP_Text healthText;
    private TMP_Text stateText;
    private TMP_Text movementSpeedText;
    private TMP_Text stunTimeText;
    private TMP_Text targetText;
    private TMP_Text locationText;
    private Image deathOverlay;

    private EntityInfoConfig entityConfig;

    private Visualization visualization;

    private LineRenderer targetLookLine;
    private LineRenderer targetPlayerLine;

    private readonly LineRenderer[] pathLines = new LineRenderer[20];

    private readonly Dictionary<string, GameObject> VisualizerObjects = [];
    private readonly Dictionary<string, float> VisualizerTimers = [];

    private LineRenderer lastHeardNoise;
    private Vector3 lastHeardNoisePosition;
    private float lastHeardNoiseTimer;

    internal void Init(EntityInfoConfig config, Visualization visualizer, EnemyAI entity)
    {
        entityConfig = config;
        visualization = visualizer;
        entityController = entity;

        targetLookLine = ImpUtils.Geometry.CreateLine(entity.transform, 0.03f, true);

        targetPlayerLine = ImpUtils.Geometry.CreateLine(entity.transform, 0.03f, true);
        for (var i = 0; i < pathLines.Length; i++)
        {
            pathLines[i] = ImpUtils.Geometry.CreateLine(transform, 0.1f, true);
        }

        lastHeardNoise = ImpUtils.Geometry.CreateLine(entity.transform, 0.03f, true);

        InitInfoPanel();
    }

    private void InitInfoPanel()
    {
        infoPanel = Instantiate(ImpAssets.EntityInfoPanel, transform);
        infoPanelRect = infoPanel.transform.Find("Panel").GetComponent<RectTransform>();
        infoPanelCanvasRect = infoPanel.GetComponent<RectTransform>();

        deathOverlay = infoPanel.transform.Find("Panel/Death").GetComponent<Image>();

        nameText = infoPanel.transform.Find("Panel/Name").GetComponent<TMP_Text>();
        healthText = infoPanel.transform.Find("Panel/Health/Value").GetComponent<TMP_Text>();
        stateText = infoPanel.transform.Find("Panel/State/Value").GetComponent<TMP_Text>();
        movementSpeedText = infoPanel.transform.Find("Panel/MovementSpeed/Value").GetComponent<TMP_Text>();
        stunTimeText = infoPanel.transform.Find("Panel/StunTime/Value").GetComponent<TMP_Text>();
        targetText = infoPanel.transform.Find("Panel/Target/Value").GetComponent<TMP_Text>();
        locationText = infoPanel.transform.Find("Panel/Location/Value").GetComponent<TMP_Text>();
    }

    internal void NoiseVisualizerUpdate(Vector3 origin)
    {
        ImpUtils.Geometry.SetLinePositions(lastHeardNoise, entityController.transform.position, origin);
        lastHeardNoise.gameObject.SetActive(true);

        lastHeardNoisePosition = origin;
        lastHeardNoiseTimer = Time.realtimeSinceStartup;
    }

    internal void ConeVisualizerUpdate(
        Transform eye,
        float angle, float size,
        Material material,
        Func<EntityInfoConfig, ImpBinding<bool>> configGetter
    )
    {
        var identifier = Visualization.GenerateConeHash(entityController, eye, angle, size);

        if (!VisualizerObjects.TryGetValue(identifier, out var visualizer))
        {
            visualizer = new GameObject($"ImpVis_LOS_{identifier}");
            visualizer.AddComponent<MeshRenderer>().material = material;
            visualizer.AddComponent<MeshFilter>().mesh = visualization.GetOrGenerateCone(angle);

            VisualizerObjects[identifier] = visualizer;
        }

        if (ImpSettings.Visualizations.SmoothAnimations.Value)
        {
            visualizer.transform.localPosition = Vector3.zero;
            visualizer.transform.localRotation = Quaternion.identity;
            visualizer.transform.SetParent(eye);
        }
        else
        {
            visualizer.transform.position = eye.position;
            visualizer.transform.rotation = eye.rotation;
            visualizer.transform.SetParent(null);
        }

        visualizer.transform.localScale = Vector3.one * size;

        // Enable / Disable based on config
        visualizer.gameObject.SetActive(configGetter(entityConfig).Value);

        // Update the visualizer timer
        VisualizerTimers[identifier] = Time.realtimeSinceStartup;
    }

    internal void SphereVisualizerUpdate(
        Transform eye,
        float size, Material material,
        Func<EntityInfoConfig, ImpBinding<bool>> configGetter
    )
    {
        var identifier = Visualization.GenerateSphereHash(entityController, eye, size);

        if (!VisualizerObjects.TryGetValue(identifier, out var visualizer))
        {
            visualizer = ImpUtils.Geometry.CreatePrimitive(
                PrimitiveType.Sphere,
                // Parent the visualizer to the eye if smooth animations are enabled
                parent: ImpSettings.Visualizations.SmoothAnimations.Value ? eye : null,
                material: material,
                size: size,
                name: $"ImpVis_Custom_{identifier}"
            );

            VisualizerObjects[identifier] = visualizer;
        }

        if (ImpSettings.Visualizations.SmoothAnimations.Value)
        {
            visualizer.transform.localPosition = Vector3.zero;
            visualizer.transform.localRotation = Quaternion.identity;
            visualizer.transform.SetParent(eye);
        }
        else
        {
            visualizer.transform.SetParent(null);
            visualizer.transform.position = eye.position;
            visualizer.transform.rotation = eye.rotation;
        }

        visualizer.transform.localScale = Vector3.one * size;

        visualizer.transform.localPosition = Vector3.zero;
        visualizer.transform.localRotation = Quaternion.identity;
        visualizer.transform.localScale = Vector3.one * size;

        // Enable / Disable based on the provided config
        visualizer.gameObject.SetActive(configGetter(entityConfig).Value);

        // Update the visualizer timer
        VisualizerTimers[identifier] = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (!entityController) return;

        // Remove all visualizers whose timer expired
        foreach (var (identifier, timer) in VisualizerTimers)
        {
            if (Time.realtimeSinceStartup - timer > 0.76f)
            {
                VisualizerObjects[identifier].gameObject.SetActive(false);
            }
        }

        // Remove the noise line after 5 seconds
        if (Time.realtimeSinceStartup - lastHeardNoiseTimer > 5)
        {
            lastHeardNoise.gameObject.SetActive(false);
        }
        else
        {
            ImpUtils.Geometry.SetLinePositions(
                lastHeardNoise,
                entityController.transform.position,
                lastHeardNoisePosition
            );
        }

        DrawInfoPanel(entityConfig.Info.Value);
        DrawLookingAtLine(entityConfig.LookingAt.Value);
        DrawTargetPlayerLine(entityConfig.Targeting.Value);
        DrawPathLines(entityConfig.Pathfinding.Value);
    }

    private void DrawInfoPanel(bool isShown)
    {
        if (!isShown)
        {
            infoPanel.SetActive(false);
            return;
        }

        // Death overlay / disable on death
        if (entityController.isEnemyDead)
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
        var worldPosition = entityController.transform.position + Vector3.up * 3f;
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

        // Panel texts
        nameText.text = Imperium.ObjectManager.GetDisplayName(entityController.enemyType.enemyName);
        healthText.text = entityController.enemyHP.ToString();

        var state = entityController.currentBehaviourStateIndex.ToString();
        stateText.text = state;

        var movementSpeed = ImpUtils.Math.FormatFloatToThreeDigits(entityController.agent.speed);
        movementSpeedText.text = movementSpeed;

        var stunTime = $"{ImpUtils.Math.FormatFloatToThreeDigits(Math.Max(0, entityController.stunNormalizedTimer))}s";
        stunTimeText.text = stunTime;

        var target = entityController.targetPlayer ? entityController.targetPlayer.playerUsername : "-";
        targetText.text = target;

        locationText.text = entityController.isOutside
            ? "Outdoors"
            : entityController.isInsidePlayerShip
                ? "In Ship"
                : "Indoors";

        infoPanel.SetActive(true);
    }

    private void DrawPathLines(bool isShown)
    {
        var corners = entityController.agent.path.corners;
        var previousCorner = entityController.transform.position;
        for (var i = 0; i < pathLines.Length; i++)
        {
            if (i < corners.Length)
            {
                // Enable / Disable based on config
                pathLines[i].gameObject.SetActive(isShown);
                if (!isShown) continue;

                ImpUtils.Geometry.SetLinePositions(
                    pathLines[i],
                    previousCorner,
                    corners[i]
                );
                ImpUtils.Geometry.SetLineColor(pathLines[i], Color.white);
                previousCorner = corners[i];
            }
            else
            {
                pathLines[i].gameObject.SetActive(false);
            }
        }
    }

    private void DrawLookingAtLine(bool isShown)
    {
        if (!isShown)
        {
            targetLookLine.gameObject.SetActive(false);
            return;
        }

        Vector3? lookAtPosition = null;

        switch (entityController)
        {
            case HoarderBugAI hoarderBug when hoarderBug.lookTarget && hoarderBug.lookingAtPositionOfInterest:
                lookAtPosition = hoarderBug.lookTarget.position;
                break;
            case PufferAI puffer:
                lookAtPosition = Reflection.Get<PufferAI, Vector3>(puffer, "lookAtNoise");
                break;
        }

        if (lookAtPosition.HasValue && lookAtPosition != Vector3.zero)
        {
            targetLookLine.gameObject.SetActive(true);

            ImpUtils.Geometry.SetLinePositions(
                targetLookLine,
                entityController.transform.position,
                lookAtPosition.Value
            );
            ImpUtils.Geometry.SetLineColor(targetLookLine, new Color(0.47f, 0.66f, 0.35f));
        }
        else
        {
            targetLookLine.gameObject.SetActive(false);
        }
    }

    private void DrawTargetPlayerLine(bool isShown)
    {
        if (!isShown)
        {
            targetPlayerLine.gameObject.SetActive(false);
            return;
        }

        if (entityController.movingTowardsTargetPlayer && entityController.targetPlayer)
        {
            targetPlayerLine.gameObject.SetActive(true);

            ImpUtils.Geometry.SetLinePositions(
                targetPlayerLine,
                entityController.transform.position,
                entityController.targetPlayer.transform.position
            );
            ImpUtils.Geometry.SetLineColor(targetPlayerLine, Color.red);
        }
        else
        {
            targetPlayerLine.gameObject.SetActive(false);
        }
    }
}

internal class EntityInfoConfig
{
    internal readonly string entityName;

    internal readonly ImpConfig<bool> Info;
    internal readonly ImpConfig<bool> Pathfinding;
    internal readonly ImpConfig<bool> Targeting;
    internal readonly ImpConfig<bool> LookingAt;
    internal readonly ImpConfig<bool> LineOfSight;
    internal readonly ImpConfig<bool> Hearing;
    internal readonly ImpConfig<bool> Custom;

    internal EntityInfoConfig(string entityName)
    {
        this.entityName = entityName;

        Info = new ImpConfig<bool>("Visualizers.Info", entityName, false);
        Pathfinding = new ImpConfig<bool>("Visualizers.Pathfinding", entityName, false);
        Targeting = new ImpConfig<bool>("Visualizers.Targeting", entityName, false);
        LookingAt = new ImpConfig<bool>("Visualizers.LookingAt", entityName, false);
        LineOfSight = new ImpConfig<bool>("Visualizers.LineOfSight", entityName, false);
        Hearing = new ImpConfig<bool>("Visualizers.Hearing", entityName, false);
        Custom = new ImpConfig<bool>("Visualizers.Custom", entityName, false);
    }
}
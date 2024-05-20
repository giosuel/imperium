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

    private GameObject canvas;

    private TMP_Text nameText;
    private TMP_Text healthText;
    private TMP_Text stateText;
    private TMP_Text movementSpeedText;
    private TMP_Text stunTimeText;
    private TMP_Text targetText;
    private Image deathOverlay;

    private EntityInfoConfig entityConfig;

    private Visualization visualization;

    private LineRenderer targetingLine;
    private LineRenderer targetPlayer;

    private readonly LineRenderer[] pathLines = new LineRenderer[20];

    private readonly Dictionary<string, GameObject> LineOfSightVisualizers = [];
    private readonly Dictionary<string, float> LineOfSightVisualizerTimers = [];

    private void Awake()
    {
        canvas = transform.Find("Canvas").gameObject;
        nameText = transform.Find("Canvas/Name").GetComponent<TMP_Text>();
        healthText = transform.Find("Canvas/Health/Value").GetComponent<TMP_Text>();
        stateText = transform.Find("Canvas/State/Value").GetComponent<TMP_Text>();
        movementSpeedText = transform.Find("Canvas/MovementSpeed/Value").GetComponent<TMP_Text>();
        stunTimeText = transform.Find("Canvas/StunTime/Value").GetComponent<TMP_Text>();
        targetText = transform.Find("Canvas/Target/Value").GetComponent<TMP_Text>();

        deathOverlay = transform.Find("Canvas/Death").GetComponent<Image>();
    }

    internal void Init(EntityInfoConfig config, Visualization visualizer, EnemyAI entity)
    {
        entityConfig = config;
        visualization = visualizer;
        entityController = entity;

        targetingLine = ImpUtils.Geometry.CreateLine(entity.transform, 0.02f, true);

        targetPlayer = ImpUtils.Geometry.CreateLine(entity.transform, 0.02f, true);
        for (var i = 0; i < pathLines.Length; i++)
        {
            pathLines[i] = ImpUtils.Geometry.CreateLine(transform, 0.05f, true);
        }
    }

    internal void LineOfSightUpdate(Transform eye, float angle, float size)
    {
        var identifier = Visualization.GenerateLOSHash(entityController, eye, angle, size);

        if (!LineOfSightVisualizers.TryGetValue(identifier, out var visualizer))
        {
            visualizer = new GameObject($"ImpVis_LOS_{identifier}");
            // visualizer.transform.SetParent(null);
            visualizer.AddComponent<MeshRenderer>().material = ImpAssets.WireframeYellowMaterial;
            visualizer.AddComponent<MeshFilter>().mesh = visualization.GetOrGenerateCone(angle);
            LineOfSightVisualizers[identifier] = visualizer;
        }

        visualizer.transform.SetParent(eye);
        visualizer.transform.localPosition = Vector3.zero;
        visualizer.transform.localRotation = Quaternion.identity;
        visualizer.transform.localScale = Vector3.one * size;

        // Enable / Disable based on config
        visualizer.gameObject.SetActive(entityConfig.LineOfSight.Value);

        LineOfSightVisualizerTimers[identifier] = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        canvas.SetActive(entityController);
        if (!entityController) return;

        foreach (var (identifier, timer) in LineOfSightVisualizerTimers)
        {
            if (Time.realtimeSinceStartup - timer > 0.76f)
            {
                LineOfSightVisualizers[identifier].gameObject.SetActive(false);
            }
        }

        DrawInfoPanel(entityConfig.Info.Value);
        DrawTargetingLine(entityConfig.Targeting.Value);
        DrawTargetPlayerLine(entityConfig.Targeting.Value);
        DrawPathLines(entityConfig.Pathfinding.Value);
    }

    private void DrawInfoPanel(bool isShown)
    {
        if (!isShown)
        {
            canvas.SetActive(false);
            return;
        }

        canvas.SetActive(true);

        deathOverlay.gameObject.SetActive(entityController.isEnemyDead);

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

        canvas.transform.LookAt(Imperium.Freecam.IsFreecamEnabled.Value
            ? Imperium.Freecam.transform.position
            : Imperium.Player.gameplayCamera.transform.position);
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

    private void DrawTargetingLine(bool isShown)
    {
        if (!isShown)
        {
            targetingLine.gameObject.SetActive(false);
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
            targetingLine.gameObject.SetActive(true);

            ImpUtils.Geometry.SetLinePositions(
                targetingLine,
                entityController.transform.position,
                lookAtPosition.Value
            );
            ImpUtils.Geometry.SetLineColor(targetingLine, new Color(0.47f, 0.66f, 0.35f));
        }
        else
        {
            targetingLine.gameObject.SetActive(false);
        }
    }

    private void DrawTargetPlayerLine(bool isShown)
    {
        if (!isShown)
        {
            targetPlayer.gameObject.SetActive(false);
            return;
        }

        if (entityController.movingTowardsTargetPlayer && entityController.targetPlayer)
        {
            targetPlayer.gameObject.SetActive(true);

            ImpUtils.Geometry.SetLinePositions(
                targetPlayer,
                entityController.transform.position,
                entityController.targetPlayer.transform.position
            );
            ImpUtils.Geometry.SetLineColor(targetPlayer, Color.red);
        }
        else
        {
            targetPlayer.gameObject.SetActive(false);
        }
    }
}

internal class EntityInfoConfig
{
    internal readonly ImpConfig<bool> Info;
    internal readonly ImpConfig<bool> Pathfinding;
    internal readonly ImpConfig<bool> Targeting;
    internal readonly ImpConfig<bool> LineOfSight;
    internal readonly ImpConfig<bool> Hearing;
    internal readonly string entityName;

    internal EntityInfoConfig(string entityName)
    {
        this.entityName = entityName;

        Info = new ImpConfig<bool>("Visualizers.Info", entityName, false);
        Pathfinding = new ImpConfig<bool>("Visualizers.Pathfinding", entityName, false);
        Targeting = new ImpConfig<bool>("Visualizers.Targeting", entityName, false);
        LineOfSight = new ImpConfig<bool>("Visualizers.LineOfSight", entityName, false);
        Hearing = new ImpConfig<bool>("Visualizers.Hearing", entityName, false);
    }
}
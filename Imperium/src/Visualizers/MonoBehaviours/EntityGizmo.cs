#region

using System;
using System.Collections.Generic;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Visualizers.MonoBehaviours;

public class EntityGizmo : MonoBehaviour
{
    private EnemyAI entityController;

    private EntityGizmoConfig entityConfig;
    private Visualization visualization;

    private LineRenderer lastHeardNoise;
    private LineRenderer targetLookLine;
    private LineRenderer targetPlayerLine;

    private readonly LineRenderer[] pathLines = new LineRenderer[20];

    private readonly Dictionary<string, GameObject> VisualizerObjects = [];
    private readonly Dictionary<string, float> VisualizerTimers = [];

    private Vector3 lastHeardNoisePosition;
    private float lastHeardNoiseTimer;

    internal void Init(EntityGizmoConfig config, Visualization visualizer, EnemyAI entity)
    {
        entityConfig = config;
        visualization = visualizer;
        entityController = entity;

        targetLookLine = ImpGeometry.CreateLine(entity.transform, 0.03f, true);

        targetPlayerLine = ImpGeometry.CreateLine(entity.transform, 0.03f, true);
        for (var i = 0; i < pathLines.Length; i++)
        {
            pathLines[i] = ImpGeometry.CreateLine(transform, 0.1f, true);
        }

        lastHeardNoise = ImpGeometry.CreateLine(entity.transform, 0.03f, true);
    }

    internal void NoiseVisualizerUpdate(Vector3 origin)
    {
        ImpGeometry.SetLinePositions(lastHeardNoise, entityController.transform.position, origin);
        lastHeardNoise.gameObject.SetActive(entityConfig.Hearing.Value);

        lastHeardNoisePosition = origin;
        lastHeardNoiseTimer = Time.realtimeSinceStartup;
    }

    internal void ConeVisualizerUpdate(
        Transform eye,
        float angle, float size,
        Material material,
        Func<EntityGizmoConfig, ImpBinding<bool>> configGetter,
        Func<Vector3> relativepositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
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

        visualizer.transform.localScale = Vector3.one * size;

        if (ImpSettings.Visualizations.SmoothAnimations.Value)
        {
            visualizer.transform.localPosition = relativepositionOverride?.Invoke() ?? Vector3.zero;
            visualizer.transform.localRotation = Quaternion.identity;
            visualizer.transform.SetParent(eye, true);
        }
        else
        {
            visualizer.transform.position = absolutePositionOverride?.Invoke(eye) ?? eye.position;
            visualizer.transform.rotation = eye.rotation;
            visualizer.transform.SetParent(null, true);
        }

        // Enable / Disable based on config
        visualizer.gameObject.SetActive(configGetter(entityConfig).Value);

        // Update the visualizer timer
        VisualizerTimers[identifier] = Time.realtimeSinceStartup;
    }

    internal void SphereVisualizerUpdate(
        Transform eye,
        float size, Material material,
        Func<EntityGizmoConfig, ImpBinding<bool>> configGetter,
        Func<Vector3> relativepositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        var identifier = Visualization.GenerateSphereHash(entityController, eye, size);

        if (!VisualizerObjects.TryGetValue(identifier, out var visualizer))
        {
            visualizer = ImpGeometry.CreatePrimitive(
                PrimitiveType.Sphere,
                // Parent the visualizer to the eye if smooth animations are enabled
                parent: ImpSettings.Visualizations.SmoothAnimations.Value ? eye : null,
                material: material,
                size: size,
                name: $"ImpVis_Custom_{identifier}"
            );

            VisualizerObjects[identifier] = visualizer;
        }

        visualizer.transform.localScale = Vector3.one * size;

        if (ImpSettings.Visualizations.SmoothAnimations.Value)
        {
            visualizer.transform.localPosition = relativepositionOverride?.Invoke() ?? Vector3.zero;
            visualizer.transform.localRotation = Quaternion.identity;
            visualizer.transform.SetParent(eye, true);
        }
        else
        {
            visualizer.transform.position = absolutePositionOverride?.Invoke(eye) ?? eye.position;
            visualizer.transform.rotation = eye.rotation;
            visualizer.transform.SetParent(null, true);
        }

        // Enable / Disable based on the provided config
        visualizer.gameObject.SetActive(configGetter(entityConfig).Value);

        // Update the visualizer timer
        VisualizerTimers[identifier] = Time.realtimeSinceStartup;
    }

    private void OnDestroy()
    {
        foreach (var (_, obj) in VisualizerObjects) Destroy(obj);
        foreach (var obj in pathLines) Destroy(obj.gameObject);
        Destroy(targetLookLine.gameObject);
        Destroy(targetPlayerLine.gameObject);
        Destroy(lastHeardNoise.gameObject);
    }

    private void Update()
    {
        if (!entityController)
        {
            foreach (var (_, obj) in VisualizerObjects) Destroy(obj);
            Destroy(gameObject);
            return;
        }

        // Remove all visualizers whose timer expired
        foreach (var (identifier, timer) in VisualizerTimers)
        {
            if (Time.realtimeSinceStartup - timer > 0.76f)
            {
                VisualizerObjects[identifier].gameObject.SetActive(false);
            }
        }

        DrawPathLines(entityConfig.Pathfinding.Value);
        DrawNoiseLine(entityConfig.Hearing.Value);
        DrawLookingAtLine(entityConfig.LookingAt.Value);
        DrawTargetPlayerLine(entityConfig.Targeting.Value);
    }

    private void DrawNoiseLine(bool isShown)
    {
        if (!isShown)
        {
            lastHeardNoise.gameObject.SetActive(false);
            return;
        }

        if (Time.realtimeSinceStartup - lastHeardNoiseTimer > 5)
        {
            lastHeardNoise.gameObject.SetActive(false);
        }
        else
        {
            ImpGeometry.SetLinePositions(
                lastHeardNoise,
                entityController.transform.position,
                lastHeardNoisePosition
            );
        }
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

                ImpGeometry.SetLinePositions(
                    pathLines[i],
                    previousCorner,
                    corners[i]
                );
                ImpGeometry.SetLineColor(pathLines[i], Color.white);
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

            ImpGeometry.SetLinePositions(
                targetLookLine,
                entityController.transform.position,
                lookAtPosition.Value
            );
            ImpGeometry.SetLineColor(targetLookLine, new Color(0.47f, 0.66f, 0.35f));
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

            ImpGeometry.SetLinePositions(
                targetPlayerLine,
                entityController.transform.position,
                entityController.targetPlayer.transform.position
            );
            ImpGeometry.SetLineColor(targetPlayerLine, Color.red);
        }
        else
        {
            targetPlayerLine.gameObject.SetActive(false);
        }
    }
}

internal class EntityGizmoConfig
{
    internal readonly string entityName;

    internal readonly ImpConfig<bool> Info;
    internal readonly ImpConfig<bool> Pathfinding;
    internal readonly ImpConfig<bool> Targeting;
    internal readonly ImpConfig<bool> LookingAt;
    internal readonly ImpConfig<bool> LineOfSight;
    internal readonly ImpConfig<bool> Hearing;
    internal readonly ImpConfig<bool> Custom;

    internal EntityGizmoConfig(string entityName)
    {
        this.entityName = entityName;

        Info = new ImpConfig<bool>("Visualization.EntityGizmos.Info", entityName, false);
        Pathfinding = new ImpConfig<bool>("Visualization.EntityGizmos.Pathfinding", entityName, false);
        Targeting = new ImpConfig<bool>("Visualization.EntityGizmos.Targeting", entityName, false);
        LookingAt = new ImpConfig<bool>("Visualization.EntityGizmos.LookingAt", entityName, false);
        LineOfSight = new ImpConfig<bool>("Visualization.EntityGizmos.LineOfSight", entityName, false);
        Hearing = new ImpConfig<bool>("Visualization.EntityGizmos.Hearing", entityName, false);
        Custom = new ImpConfig<bool>("Visualization.EntityGizmos.Custom", entityName, false);
    }
}
#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.API.Types;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using JetBrains.Annotations;
using Unity.Netcode;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Visualizers.Objects;

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
    private readonly Dictionary<string, ulong> VisualizerObjectNetIds = [];
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
        float angle, float length,
        Material material,
        Func<EntityGizmoConfig, ImpBinding<bool>> configGetter,
        GizmoDuration gizmoDuration,
        int id = -1,
        Func<Vector3> relativePositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        // Default the ID to the radius
        if (id == -1) id = (int)length;

        var identifier = Visualization.GenerateConeHash(entityController, eye, angle, id);

        if (!VisualizerObjects.TryGetValue(identifier, out var visualizer))
        {
            visualizer = new GameObject($"ImpVis_LOS_{identifier}");
            visualizer.AddComponent<MeshRenderer>().material = material;
            visualizer.AddComponent<MeshFilter>().mesh = visualization.GetOrGenerateCone(angle);

            VisualizerObjects[identifier] = visualizer;
            VisualizerObjectNetIds[identifier] = entityController.GetComponent<NetworkObject>()?.NetworkObjectId
                                                 ?? (ulong)entityController.GetInstanceID();
        }

        visualizer.transform.localScale = Vector3.one * length;

        if (Imperium.Settings.Visualization.SmoothAnimations.Value)
        {
            visualizer.transform.localPosition = relativePositionOverride?.Invoke() ?? Vector3.zero;
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

        if (gizmoDuration == GizmoDuration.AIInterval)
        {
            VisualizerTimers[identifier] = Time.realtimeSinceStartup;
        }
    }

    internal void SphereVisualizerUpdate(
        [CanBeNull] Transform eye,
        float radius, Material material,
        Func<EntityGizmoConfig, ImpBinding<bool>> configGetter,
        GizmoDuration gizmoDuration,
        int id = -1,
        Func<Vector3> relativePositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        // Default the ID to the radius
        if (id == -1) id = (int)radius;

        var identifier = Visualization.GenerateSphereHash(entityController, eye, id);
        var visualizer = GetOrCreateSphereVisualizerObject(identifier, radius, material);

        if (Imperium.Settings.Visualization.SmoothAnimations.Value)
        {
            visualizer.transform.localPosition = relativePositionOverride?.Invoke() ?? Vector3.zero;
            visualizer.transform.localRotation = Quaternion.identity;
            visualizer.transform.SetParent(eye, true);
        }
        else
        {
            visualizer.transform.position = absolutePositionOverride?.Invoke(eye) ?? eye?.position ?? Vector3.zero;
            visualizer.transform.rotation = eye ? eye.rotation : Quaternion.identity;
            visualizer.transform.SetParent(null, true);
        }

        // Enable / Disable based on the provided config
        visualizer.SetActive(configGetter(entityConfig).Value);

        if (gizmoDuration == GizmoDuration.AIInterval)
        {
            VisualizerTimers[identifier] = Time.realtimeSinceStartup;
        }
    }

    internal void StaticSphereVisualizerUpdate(
        GameObject ownerObj,
        Vector3 position,
        float radius,
        Material material,
        Func<EntityGizmoConfig, ImpBinding<bool>> configGetter,
        GizmoDuration gizmoDuration,
        int id = -1
    )
    {
        // Default the ID to the radius
        if (id == -1) id = (int)radius;

        var identifier = Visualization.GenerateSphereHash(entityController, ownerObj, id);
        var visualizer = GetOrCreateSphereVisualizerObject(identifier, radius, material);

        visualizer.transform.position = position;
        visualizer.transform.rotation = Quaternion.identity;

        // Enable / Disable based on the provided config
        visualizer.SetActive(configGetter(entityConfig).Value);

        if (gizmoDuration == GizmoDuration.AIInterval)
        {
            VisualizerTimers[identifier] = Time.realtimeSinceStartup;
        }
    }

    private GameObject GetOrCreateSphereVisualizerObject(string identifier, float radius, Material material)
    {
        if (!VisualizerObjects.TryGetValue(identifier, out var visualizer))
        {
            visualizer = ImpGeometry.CreatePrimitive(
                PrimitiveType.Sphere,
                parent: null,
                material: material,
                size: radius * 2,
                name: $"ImpVis_Custom_{identifier}"
            );

            VisualizerObjects[identifier] = visualizer;
            VisualizerObjectNetIds[identifier] = entityController.GetComponent<NetworkObject>()?.NetworkObjectId
                                                 ?? (ulong)entityController.GetInstanceID();
        }

        return visualizer;
    }

    private void OnDestroy()
    {
        foreach (var (_, obj) in VisualizerObjects) Destroy(obj);
        foreach (var obj in pathLines.Where(obj => obj)) Destroy(obj.gameObject);
        if (targetLookLine) Destroy(targetLookLine.gameObject);
        if (targetPlayerLine) Destroy(targetPlayerLine.gameObject);
        if (lastHeardNoise) Destroy(lastHeardNoise.gameObject);
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
            var isDisabled = Imperium.ObjectManager.DisabledObjects.Value.Contains(VisualizerObjectNetIds[identifier]);
            if (Time.realtimeSinceStartup - timer > 0.76f && !isDisabled)
            {
                VisualizerObjects[identifier].gameObject.SetActive(false);
            }
        }

        DrawPathLines(entityConfig.Pathfinding.Value && entityController.enabled);
        DrawNoiseLine(entityConfig.Hearing.Value && entityController.enabled);
        DrawTargetPlayerLine(entityConfig.Targeting.Value && entityController.enabled);
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
            ImpGeometry.SetLineColor(targetPlayerLine, Color.red, Color.red);
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
    internal readonly ImpConfig<bool> LineOfSight;
    internal readonly ImpConfig<bool> Hearing;
    internal readonly ImpConfig<bool> Custom;

    internal EntityGizmoConfig(string entityName, ConfigFile config)
    {
        this.entityName = entityName;

        var escapedEntityName = entityName
            .Replace("\"", "")
            .Replace("\'", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("\n", "")
            .Replace("\t", "")
            .Replace("\\", "")
            .Trim();

        Info = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Info", escapedEntityName, false);
        Pathfinding = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Pathfinding", escapedEntityName, false);
        Targeting = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Targeting", escapedEntityName, false);
        LineOfSight = new ImpConfig<bool>(config, "Visualization.EntityGizmos.LineOfSight", escapedEntityName, false);
        Hearing = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Hearing", escapedEntityName, false);
        Custom = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Custom", escapedEntityName, false);
    }
}

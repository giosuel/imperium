#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.API.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

/// <summary>
///     Entity-specific gizmos like LoS indicators, target rays, noise rays, etc.
/// </summary>
internal class EntityGizmos : BaseVisualizer<IReadOnlyCollection<EnemyAI>, EntityGizmo>
{
    internal readonly Dictionary<EnemyType, EntityGizmoConfig> EntityInfoConfigs = [];

    private readonly ConfigFile config;

    internal EntityGizmos(IBinding<IReadOnlyCollection<EnemyAI>> objectsBinding, ConfigFile config) : base(objectsBinding)
    {
        this.config = config;

        foreach (var entity in Resources.FindObjectsOfTypeAll<EnemyType>())
        {
            EntityInfoConfigs[entity] = new EntityGizmoConfig(entity.enemyName, config);
        }
    }

    protected override void OnRefresh(IReadOnlyCollection<EnemyAI> objects)
    {
        ClearObjects();

        foreach (var entity in objects.Where(entity => entity))
        {
            if (!visualizerObjects.ContainsKey(entity.GetInstanceID()))
            {
                if (!EntityInfoConfigs.TryGetValue(entity.enemyType, out var entityConfig))
                {
                    entityConfig = new EntityGizmoConfig(entity.enemyType.enemyName, config);
                    EntityInfoConfigs[entity.enemyType] = entityConfig;
                }

                var entityGizmoObject = new GameObject($"Imp_EntityGizmo_{entity.GetInstanceID()}");
                var entityGizmo = entityGizmoObject.AddComponent<EntityGizmo>();
                entityGizmo.Init(entityConfig, Imperium.Visualization, entity);

                visualizerObjects[entity.GetInstanceID()] = entityGizmo;
            }
        }
    }

    internal void NoiseVisualizerUpdate(EnemyAI instance, Vector3 origin)
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.NoiseVisualizerUpdate(origin);
        }
    }

    internal void ConeVisualizerUpdate(
        EnemyAI instance, Transform eye, float angle, float length, Material material,
        GizmoType gizmoType = GizmoType.LineOfSight, GizmoDuration gizmoDuration = GizmoDuration.AIInterval,
        int id = -1,
        Func<Vector3> relativePositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.ConeVisualizerUpdate(
                eye ?? instance.transform,
                angle, length, material,
                visConfig => gizmoType == GizmoType.LineOfSight ? visConfig.LineOfSight : visConfig.Custom,
                gizmoDuration,
                id,
                relativePositionOverride,
                absolutePositionOverride
            );
        }
    }

    internal void SphereVisualizerUpdate(
        EnemyAI instance, Transform eye, float radius, Material material,
        GizmoType gizmoType = GizmoType.LineOfSight, GizmoDuration gizmoDuration = GizmoDuration.AIInterval,
        int id = -1,
        Func<Vector3> relativePositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.SphereVisualizerUpdate(
                eye ?? instance.transform,
                radius, material,
                visConfig => gizmoType == GizmoType.LineOfSight ? visConfig.LineOfSight : visConfig.Custom,
                gizmoDuration,
                id,
                relativePositionOverride,
                absolutePositionOverride
            );
        }
    }

    internal void StaticSphereVisualizerUpdate(
        EnemyAI instance, Vector3 position, Material material,
        float radius = 2f,
        GizmoType gizmoType = GizmoType.LineOfSight, GizmoDuration gizmoDuration = GizmoDuration.Indefinite,
        int id = -1
    )
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.StaticSphereVisualizerUpdate(
                instance.gameObject,
                position, radius, material,
                visConfig => gizmoType == GizmoType.LineOfSight ? visConfig.LineOfSight : visConfig.Custom,
                gizmoDuration,
                id
            );
        }
    }
}
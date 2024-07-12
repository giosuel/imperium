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

    internal EntityGizmos(IBinding<IReadOnlyCollection<EnemyAI>> objectsBinding, ConfigFile config) : base(objectsBinding)
    {
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
                var entityGizmoObject = new GameObject($"Imp_EntityGizmo_{entity.GetInstanceID()}");
                var entityGizmo = entityGizmoObject.AddComponent<EntityGizmo>();
                entityGizmo.Init(EntityInfoConfigs[entity.enemyType], Imperium.Visualization, entity);

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
        EnemyAI instance, Transform eye, float angle, float size, Material material, bool isCustom = false,
        Func<Vector3> relativepositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.ConeVisualizerUpdate(
                eye ?? instance.transform,
                angle, size, material,
                config => isCustom ? config.Custom : config.LineOfSight,
                relativepositionOverride,
                absolutePositionOverride
            );
        }
    }

    internal void SphereVisualizerUpdate(
        EnemyAI instance, Transform eye, float size, Material material, bool isCustom = false,
        Func<Vector3> relativepositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.SphereVisualizerUpdate(
                eye,
                size, material,
                config => isCustom ? config.Custom : config.LineOfSight,
                relativepositionOverride,
                absolutePositionOverride
            );
        }
    }
}
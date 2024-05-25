#region

using System.Collections.Generic;
using System.Linq;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class EntityInfos : BaseVisualizer<HashSet<EnemyAI>>
{
    private readonly Dictionary<int, EntityInfo> indicatorComponents = [];

    internal readonly Dictionary<EnemyType, EntityInfoConfig> EntityInfoConfigs = [];

    internal EntityInfos(
        ImpBinding<HashSet<EnemyAI>> objectsBinding
    ) : base("Entity Infos", objectsBinding)
    {
        foreach (var entity in Resources.FindObjectsOfTypeAll<EnemyType>())
        {
            EntityInfoConfigs[entity] = new EntityInfoConfig(entity.enemyName);
        }
    }

    protected override void Refresh(HashSet<EnemyAI> objects)
    {
        foreach (var entity in objects.Where(entity => entity))
        {
            if (!indicatorObjects.ContainsKey(entity.GetInstanceID()))
            {
                var entityInfoObject = new GameObject($"Imp_EntityInfo_{entity.GetInstanceID()}");
                entityInfoObject.transform.SetParent(entity.transform);
                var entityInfo = entityInfoObject.AddComponent<EntityInfo>();
                entityInfo.Init(EntityInfoConfigs[entity.enemyType], Imperium.Visualization, entity);

                indicatorObjects[entity.GetInstanceID()] = entityInfoObject;

                // This is a temporary fix
                indicatorComponents[entity.GetInstanceID()] = entityInfo;
            }
        }
    }

    internal void NoiseVisualizerUpdate(EnemyAI instance, Vector3 origin)
    {
        if (indicatorComponents.TryGetValue(instance.GetInstanceID(), out var entity))
        {
            entity.NoiseVisualizerUpdate(origin);
        }
    }

    internal void ConeVisualizerUpdate(
        EnemyAI instance, Transform eye, float angle, float size, Material material, bool isCustom = false
    )
    {
        if (indicatorComponents.TryGetValue(instance.GetInstanceID(), out var entity))
        {
            entity.ConeVisualizerUpdate(
                eye ?? instance.transform,
                angle, size, material,
                config => isCustom ? config.Custom : config.LineOfSight
            );
        }
    }

    internal void SphereVisualizerUpdate(
        EnemyAI instance, Transform eye, float size, Material material, bool isCustom = false
    )
    {
        if (indicatorComponents.TryGetValue(instance.GetInstanceID(), out var entity))
        {
            entity.SphereVisualizerUpdate(
                eye ?? instance.transform,
                size, material,
                config => isCustom ? config.Custom : config.LineOfSight
            );
        }
    }
}
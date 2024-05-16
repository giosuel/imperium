#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class EntityInfos(ImpBinding<HashSet<EnemyAI>> objectsBinding)
    : BaseVisualizer<HashSet<EnemyAI>>("Entity Infos", objectsBinding)
{
    protected override void Refresh(HashSet<EnemyAI> objects)
    {
        foreach (var entity in objects.Where(entity => entity))
        {
            if (!indicatorObjects.ContainsKey(entity.GetInstanceID()))
            {
                var entityInfoObject = Object.Instantiate(ImpAssets.EntityInfo);
                var collisionDectector = entity.GetComponentInChildren<EnemyAICollisionDetect>();
                var parent = collisionDectector ? collisionDectector.transform  : entity.transform;

                // Get the highest point of any box collider (min 1f)
                var collider = entity.GetComponentInChildren<BoxCollider>();
                var offsetY = 1f;
                // Offsets the entity infobox above the entity if the entity has a box collider
                if (collider)
                {
                    var bounds = collider.bounds;
                    offsetY += (bounds.max.y - bounds.min.y) / 2;
                }

                entityInfoObject.transform.position = parent.position + Vector3.up * offsetY;
                entityInfoObject.transform.localScale = Vector3.one * 0.4f;

                // ReSharper disable once Unity.InstantiateWithoutParent
                // This needs to be done in order to not inherit the scale of the parent
                entityInfoObject.transform.SetParent(parent, true);
                var entityInfo = entityInfoObject.AddComponent<EntityInfo>();
                entityInfo.Init(entity);

                entityInfoObject.SetActive(ImpSettings.Visualizations.EntityInfo.Value);
                indicatorObjects[entity.GetInstanceID()] = entityInfoObject;
            }
        }
    }
}
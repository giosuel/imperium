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

internal class VentTimers(ImpBinding<HashSet<EnemyVent>> objectsBinding, ImpBinding<bool> visibleBinding)
    : BaseVisualizer<HashSet<EnemyVent>>("Vent Timers", objectsBinding, visibleBinding)
{
    protected override void Refresh(HashSet<EnemyVent> objects)
    {
        foreach (var entityVent in objects.Where(obj => obj))
        {
            if (!indicatorObjects.ContainsKey(entityVent.GetInstanceID()))
            {
                var parent = entityVent.transform;
                var timerObject = Object.Instantiate(ImpAssets.SpawnTimerObject, parent, true);
                var rotation = parent.rotation;
                timerObject.transform.rotation = rotation;
                timerObject.transform.localRotation = Quaternion.Euler(0, 180, -90);
                timerObject.transform.position = parent.position + Vector3.up * 0.8f;
                var timer = timerObject.AddComponent<VentTimer>();
                timer.vent = entityVent;

                indicatorObjects[entityVent.GetInstanceID()] = timerObject;
            }
        }
    }
}
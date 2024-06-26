#region

using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class VentTimers(
    IBinding<IReadOnlyCollection<EnemyVent>> objectsBinding,
    IBinding<bool> visibilityBinding
) : BaseVisualizer<IReadOnlyCollection<EnemyVent>, VentTimer>(objectsBinding, visibilityBinding)
{
    protected override void OnRefresh(IReadOnlyCollection<EnemyVent> objects)
    {
        foreach (var entityVent in objects.Where(obj => obj))
        {
            if (!visualizerObjects.ContainsKey(entityVent.GetInstanceID()))
            {
                var ventTimerObject = Object.Instantiate(ImpAssets.SpawnTimerObject, entityVent.transform, true);
                ventTimerObject.name = $"Imp_VentTimer_{entityVent.GetInstanceID()}";
                ventTimerObject.transform.rotation = entityVent.transform.rotation;
                ventTimerObject.transform.localRotation = Quaternion.Euler(0, 180, -90);
                ventTimerObject.transform.position = entityVent.transform.position + Vector3.up * 0.8f;

                var ventTimer = ventTimerObject.AddComponent<VentTimer>();
                ventTimer.vent = entityVent;

                visualizerObjects[entityVent.GetInstanceID()] = ventTimer;
            }
        }
    }
}
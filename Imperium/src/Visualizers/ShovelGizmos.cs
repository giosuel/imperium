#region

using Imperium.API.Types;
using Imperium.Core;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class ShovelGizmos(ImpBinding<bool> visibilityBinding)
    : BaseVisualizer<Shovel, ShovelGizmo>(visibilityBinding: visibilityBinding)
{
    internal void Refresh(Shovel shovel, bool isActivelyHolding)
    {
        if (!visualizerObjects.TryGetValue(shovel.GetInstanceID(), out var shotgunGizmo))
        {
            var shotgunGizmoObject = new GameObject($"Imp_ShovelGizmo_{shovel.GetInstanceID()}");
            shotgunGizmoObject.transform.SetParent(shovel.transform);
            shotgunGizmo = shotgunGizmoObject.AddComponent<ShovelGizmo>();

            visualizerObjects[shovel.GetInstanceID()] = shotgunGizmo;
        }

        shotgunGizmo.Init(shovel, isActivelyHolding);
        shotgunGizmo.gameObject.SetActive(Imperium.Settings.Visualization.ShovelIndicators.Value);
    }
}
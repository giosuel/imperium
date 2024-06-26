#region

using Imperium.API.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class ShotgunGizmos(
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<ShotgunItem, ShotgunGizmo>(visibilityBinding: visibilityBinding)
{
    internal void Refresh(ShotgunItem shotgun, bool isActivelyHolding)
    {
        if (!visualizerObjects.TryGetValue(shotgun.GetInstanceID(), out var shotgunGizmo))
        {
            var shotgunGizmoObject = new GameObject($"Imp_ShotgunGizmo_{shotgun.GetInstanceID()}");
            shotgunGizmoObject.transform.SetParent(shotgun.transform);
            shotgunGizmo = shotgunGizmoObject.AddComponent<ShotgunGizmo>();

            visualizerObjects[shotgun.GetInstanceID()] = shotgunGizmo;
        }

        shotgunGizmo.Init(shotgun, isActivelyHolding);
        shotgunGizmo.gameObject.SetActive(Imperium.Settings.Visualization.ShotgunIndicators.Value);
    }
}
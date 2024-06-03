#region

using Imperium.Core;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class ShotgunGizmos(
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<ShotgunItem, ShotgunGizmo>(visibleBinding: visibleBinding)
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
        shotgunGizmo.gameObject.SetActive(ImpSettings.Visualizations.ShotgunIndicators.Value);
    }
}
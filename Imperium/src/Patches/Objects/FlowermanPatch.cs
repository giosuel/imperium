#region

using HarmonyLib;
using Imperium.API.Types;
using Imperium.Util;
using Imperium.Visualizers;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(FlowermanAI))]
public static class FlowermanPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DoAIInterval")]
    private static void DoAIIntervalPatch(FlowermanAI __instance)
    {
        Imperium.Visualization.EntityGizmos.StaticSphereVisualizerUpdate(
            __instance,
            __instance.favoriteSpot.position,
            material: ImpAssets.WireframeYellow,
            id: 1,
            gizmoType: GizmoType.Custom
        );
    }
}
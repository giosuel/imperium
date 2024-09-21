#region

using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(CaveDwellerAI))]
public static class CaveDwellerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DoAIInterval")]
    private static void DoAIIntervalPatch(CaveDwellerAI __instance)
    {
        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            __instance,
            __instance.eye,
            __instance.currentSearchWidth + 15,
            material: ImpAssets.WireframeRed
        );

        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            __instance,
            __instance.eye,
            __instance.currentSearchWidth + 15,
            material: ImpAssets.WireframeCyan,
            relativepositionOverride: () =>  __instance.caveHidingSpot,
            absolutePositionOverride: eye => __instance.caveHidingSpot
        );
    }
}
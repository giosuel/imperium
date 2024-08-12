#region

using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(RadMechAI))]
public static class RadMechAIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("CheckSightForThreat")]
    private static void CheckSightForThreatPostfixPatch(RadMechAI __instance)
    {
        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            __instance,
            __instance.eye,
            60f,
            material: ImpAssets.WireframePurple,
            relativepositionOverride: () => Vector3.forward * 58f - Vector3.up * 10f,
            absolutePositionOverride: eye => eye.forward * 58f - eye.up * 10f
        );
    }
}
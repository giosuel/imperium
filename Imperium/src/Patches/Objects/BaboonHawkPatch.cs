#region

using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(BaboonBirdAI))]
public static class BaboonHawkPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DoLOSCheck")]
    private static void DoLOSCheckPostfixPatch(BaboonBirdAI __instance)
    {
        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            __instance,
            __instance.eye,
            40f,
            material: ImpAssets.WireframePurple,
            relativepositionOverride: () => Vector3.forward * 38f + Vector3.up * 8f,
            absolutePositionOverride: eye => eye.position + eye.forward * 38f + eye.up * 8f
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            110,
            10,
            material: ImpAssets.WireframeCyan
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            30,
            20,
            material: ImpAssets.WireframeAmaranth
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            80,
            16,
            material: ImpAssets.WireframeYellow
        );
    }
}
#region

using HarmonyLib;
using Imperium.API;
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
            40f * 2,
            material: Materials.WireframePurple,
            relativepositionOverride: () => Vector3.forward * 38f + Vector3.up * 8f,
            absolutePositionOverride: eye => eye.position + eye.forward * 38f + eye.up * 8f
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            180,
            10,
            material: Materials.WireframeCyan
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            160,
            16,
            material: Materials.WireframeAmaranth
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            60,
            20,
            material: Materials.WireframeYellow
        );
    }
}
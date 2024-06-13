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
    private static void DoLOSCheckPostfixPatch(RadMechAI __instance)
    {
        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            __instance,
            __instance.eye,
            40f * 2,
            material: API.Materials.WireframePurple,
            relativepositionOverride: () => Vector3.forward * 38f + Vector3.up * 8f,
            absolutePositionOverride: eye => eye.position + eye.forward * 38f + eye.up * 8f
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            180,
            10,
            material: API.Materials.WireframeCyan
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            160,
            16,
            material: API.Materials.WireframeAmaranth
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            60,
            20,
            material: API.Materials.WireframeYellow
        );
    }
}
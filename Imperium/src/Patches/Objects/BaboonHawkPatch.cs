using HarmonyLib;
using Imperium.Util;
using UnityEngine;

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
            material: ImpAssets.WireframePurpleMaterial,
            relativepositionOverride: () => Vector3.forward * 38f + Vector3.up * 8f,
            absolutePositionOverride: eye => eye.position + eye.forward * 38f + eye.up * 8f
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            180,
            10,
            material: ImpAssets.WireframeCyanMaterial
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            160,
            16,
            material: ImpAssets.WireframeAmaranthMaterial
        );

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            60,
            20,
            material: ImpAssets.WireframeYellowMaterial
        );
    }
}
#region

using HarmonyLib;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(NutcrackerEnemyAI))]
public static class NutcrackerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForLocalPlayer")]
    private static void CheckLineOfSightForLocalPlayerPostfixPatch(
        NutcrackerEnemyAI __instance, float width, int range, int proximityAwareness
    )
    {
        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            width,
            range,
            material: ImpAssets.WireframeCyan
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                null,
                proximityAwareness * 2,
                material: ImpAssets.WireframePurple
            );
        }
    }
}
#region

using HarmonyLib;
using Imperium.API;
using UnityEngine;

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
            material: Materials.WireframeCyan
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                null,
                proximityAwareness * 2,
                material: Materials.WireframePurple
            );
        }
    }
}
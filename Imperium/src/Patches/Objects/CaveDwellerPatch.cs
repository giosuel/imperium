#region

using HarmonyLib;
using Imperium.API.Types;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(CaveDwellerAI))]
public static class CaveDwellerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DoAIInterval")]
    private static void DoAIIntervalPatch(CaveDwellerAI __instance)
    {
        if (__instance.isOutside)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                radius: __instance.currentSearchWidth + 15,
                material: ImpAssets.WireframeRed,
                id: 1
            );

            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                radius: __instance.currentSearchWidth + 30,
                material: ImpAssets.WireframeGreen,
                id: 2
            );
        }
        else
        {
            Imperium.Visualization.EntityGizmos.StaticSphereVisualizerUpdate(
                __instance,
                __instance.caveHidingSpot,
                radius: __instance.currentSearchWidth + 15,
                material: ImpAssets.WireframeRed,
                id: 3,
                gizmoType: GizmoType.Custom
            );

            Imperium.Visualization.EntityGizmos.StaticSphereVisualizerUpdate(
                __instance,
                __instance.caveHidingSpot,
                radius: __instance.currentSearchWidth + 30,
                material: ImpAssets.WireframeGreen,
                id: 4,
                gizmoType: GizmoType.Custom
            );
        }
    }
}
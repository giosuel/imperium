#region

using HarmonyLib;
using Imperium.API.Types;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(FlowermanAI))]
public static class FlowermanPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DoAIInterval")]
    private static void DoAIIntervalPatch(FlowermanAI __instance)
    {
        if (__instance.favoriteSpot)
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
}
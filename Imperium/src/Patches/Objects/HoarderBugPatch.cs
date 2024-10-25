#region

using HarmonyLib;
using Imperium.API.Types;
using Imperium.Util;
using Imperium.Visualizers;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(HoarderBugAI))]
public static class HoarderBugPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForItem")]
    private static void CheckLineOfSightForItemPrefixPatch(
        HoarderBugAI __instance, HoarderBugItemStatus searchForItemsOfStatus,
        float width, int range, float proximityAwareness
    )
    {
        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            width,
            proximityAwareness,
            material: ImpAssets.WireframeRed
        );

        Imperium.Visualization.EntityGizmos.StaticSphereVisualizerUpdate(
            __instance,
            __instance.nestPosition,
            material: ImpAssets.WireframeYellow,
            id: 1,
            gizmoType: GizmoType.Custom
        );
    }
}
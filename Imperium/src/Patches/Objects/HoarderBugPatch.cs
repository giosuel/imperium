#region

using HarmonyLib;
using Imperium.Util;

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
    }
}
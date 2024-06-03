#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(KnifeItem))]
internal static class KnifePatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DiscardItem")]
    internal static void DiscardItemPatch(KnifeItem __instance)
    {
        Imperium.Visualization.KnifeGizmos.Refresh(__instance, false);
    }
}
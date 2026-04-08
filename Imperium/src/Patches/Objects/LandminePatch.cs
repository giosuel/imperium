#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(Landmine))]
internal static class LandminePatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Detonate")]
    internal static void DetonatePatch(Landmine __instance)
    {
        Imperium.Visualization.LandmineGizmos.SnapshotPlayerHitbox(__instance.GetInstanceID());
    }
}
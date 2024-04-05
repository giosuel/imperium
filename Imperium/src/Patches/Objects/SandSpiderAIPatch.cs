#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(SandSpiderAI))]
internal static class SandSpiderAIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("SpawnWebTrapClientRpc")]
    private static void SpawnWebTrapClientRpcPatch()
    {
        Imperium.Visualization.RefreshOverlays();
        Imperium.ObjectManager.RefreshLevelObstacles();
    }
}
#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(QuickMenuManager))]
internal static class QuickMenuManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("LeaveGameConfirm")]
    private static void LeaveGameConfirmPatch()
    {
        PreInitPatches.ReturnedFromGame = true;
        Imperium.Unload();
    }
}
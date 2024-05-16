#region

using HarmonyLib;
using Imperium.Netcode;
using Unity.Netcode;

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
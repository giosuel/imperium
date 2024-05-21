#region

using HarmonyLib;
using Imperium.Core;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(OutOfBoundsTrigger))]
internal static class OutOfBoundsTriggerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("OnTriggerEnter")]
    private static bool OnTriggerEnterPatch()
    {
        return !ImpSettings.Player.DisableOOB.Value;
    }
}
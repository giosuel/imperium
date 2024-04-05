#region

using HarmonyLib;
using Imperium.Core;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("SetClockVisible")]
    private static bool SetClockVisiblePatch(HUDManager __instance)
    {
        if (ImpSettings.Time.PermanentClock.Value || !Imperium.Player.isInsideFactory)
        {
            __instance.Clock.targetAlpha = Imperium.GameManager.TimeIsPaused.Value ? 0.4f : 1f;
            return false;
        }

        return true;
    }
}
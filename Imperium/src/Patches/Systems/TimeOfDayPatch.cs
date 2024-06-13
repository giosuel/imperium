#region

using HarmonyLib;
using UnityEngine;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(TimeOfDay))]
public class TimeOfDayPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("MoveGlobalTime")]
    private static bool MoveGlobalTimePrefixPatch(TimeOfDay __instance)
    {
        if (Imperium.Settings.Time.RealtimeClock.Value)
        {
            Imperium.HUDManager.SetClock(__instance.normalizedTimeOfDay, __instance.numberOfHours);
        }

        if (Imperium.MoonManager.TimeIsPaused.Value) return false;

        var timeBefore = __instance.globalTime;
        __instance.globalTime = Mathf.Clamp(
            timeBefore + Time.deltaTime * Imperium.MoonManager.TimeSpeed.Value,
            0f, __instance.globalTimeAtEndOfDay
        );
        __instance.timeUntilDeadline -= __instance.globalTime - timeBefore;

        return false;
    }


    [HarmonyPrefix]
    [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
    private static void UpdateProfitQuotaCurrentTimePatch(TimeOfDay __instance)
    {
        if (Imperium.GameManager.DisableQuota.Value) __instance.timeUntilDeadline = __instance.totalTime * 3f;
    }
}
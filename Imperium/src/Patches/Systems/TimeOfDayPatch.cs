#region

using HarmonyLib;
using Imperium.Core;
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

        if (
            !Imperium.MoonManager.TimeIsPaused.Value &&
            !Mathf.Approximately(Imperium.MoonManager.TimeSpeed.Value, ImpConstants.DefaultTimeSpeed)
        )
        {
            __instance.globalTimeSpeedMultiplier = Imperium.MoonManager.TimeSpeed.Value;
        }

        return true;
    }


    [HarmonyPrefix]
    [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
    private static void UpdateProfitQuotaCurrentTimePatch(TimeOfDay __instance)
    {
        if (Imperium.GameManager.DisableQuota.Value) __instance.timeUntilDeadline = __instance.totalTime * 3f;
    }
}
#region

using HarmonyLib;
using Imperium.Core;
using Imperium.Interface.MapUI;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(HUDManager))]
internal static class HUDManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("SetClockVisible")]
    private static bool SetClockVisiblePatch(HUDManager __instance)
    {
        if (Imperium.Settings.Time.PermanentClock.Value || !Imperium.Player.isInsideFactory)
        {
            if (Imperium.MoonManager.TimeIsPaused.Value)
            {
                __instance.Clock.targetAlpha = 0.6f;
                __instance.clockIcon.sprite = ImpAssets.LockImage;
            }
            else
            {
                __instance.Clock.targetAlpha = 1f;
                Imperium.TimeOfDay.RefreshClockUI();
            }
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Stops the ping from going through when the map is opened (Left click is used to drag the map)
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch("PingScan_performed")]
    private static bool PingScan_performedPatch(HUDManager __instance)
    {
        return !Imperium.Interface.IsOpen<MapUI>();
    }
}
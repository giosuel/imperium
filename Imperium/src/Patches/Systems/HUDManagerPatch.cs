#region

using HarmonyLib;
using Imperium.MonoBehaviours.ImpUI.MapUI;

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
            __instance.Clock.targetAlpha = Imperium.MoonManager.TimeIsPaused.Value ? 0.4f : 1f;
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
        return !Imperium.Interface.Get<MapUI>().IsOpen;
    }
}
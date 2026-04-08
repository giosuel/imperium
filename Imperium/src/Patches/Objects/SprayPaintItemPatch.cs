#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(SprayPaintItem))]
internal static class SprayPaintItemPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("LateUpdate")]
    private static void LateUpdatePatch(SprayPaintItem __instance)
    {
        if (Imperium.Settings.Player.InfiniteBattery.Value)
        {
            __instance.sprayCanTank = 1f;
            // It is important that Weed Killer is able to run out of its burst meter,
            // so that it stops and resets addVehicleHPInterval, otherwise the interval
            // depletes slower and depletion speed is inversely proportional to FPS.
            // See https://github.com/giosuel/imperium/issues/76
            if (!__instance.isWeedKillerSprayBottle)
            {
                __instance.sprayCanShakeMeter = 1f;
            }
        }
    }
}
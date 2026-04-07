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
            __instance.sprayCanTank = 1;
            __instance.sprayCanShakeMeter = 1;
        }
    }
}
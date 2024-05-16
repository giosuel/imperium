#region

using HarmonyLib;
using Imperium.Core;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(SprayPaintItem))]
internal static class SprayPaintItemPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("LateUpdate")]
    private static void LateUpdatePatch(SprayPaintItem __instance)
    {
        if (ImpSettings.Player.InfiniteBattery.Value)
        {
            Reflection.Set(__instance, "sprayCanTank", 1);
            Reflection.Set(__instance, "sprayCanShakeMeter", 1);
        }
    }
}
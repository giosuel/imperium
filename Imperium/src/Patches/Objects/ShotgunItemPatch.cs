#region

using HarmonyLib;
using Imperium.Core;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(ShotgunItem))]
internal static class ShotgunItemPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("ShootGun")]
    private static void ShootGunPatch(ShotgunItem __instance)
    {
        if (Imperium.Settings.Shotgun.InfiniteAmmo.Value) __instance.shellsLoaded = 2;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    private static void UpdatePatch(ShotgunItem __instance)
    {
        if (__instance.isHeld)
        {
            __instance.useCooldown = Imperium.Settings.Shotgun.FullAuto.Value
                ? 0
                // Get default use cooldown from the shotgun spawn prefab
                : __instance.itemProperties.spawnPrefab.GetComponent<ShotgunItem>().useCooldown;
            if (Imperium.Settings.Shotgun.InfiniteAmmo.Value) __instance.shellsLoaded = 2;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("EquipItem")]
    private static void EquipItemPostfixPatch(ShotgunItem __instance)
    {
        Imperium.Visualization.ShotgunGizmos.Refresh(__instance, true);
    }

    [HarmonyPrefix]
    [HarmonyPatch("PocketItem")]
    private static void PocketItemPrefixPatch(ShotgunItem __instance)
    {
        // Reset shotgun cooldown to default value when pocketing
        __instance.useCooldown = ImpConstants.ShotgunDefaultCooldown;

        Imperium.Visualization.ShotgunGizmos.Refresh(__instance, false);
    }

    [HarmonyPrefix]
    [HarmonyPatch("DiscardItem")]
    private static void DiscardItemPrefixPatch(ShotgunItem __instance)
    {
        // Reset shotgun cooldown to default value when dropping
        __instance.useCooldown = ImpConstants.ShotgunDefaultCooldown;

        Imperium.Visualization.ShotgunGizmos.Refresh(__instance, false);
    }
}
#region

using HarmonyLib;
using Imperium.Core;
using Imperium.Core.Lifecycle;
using Imperium.Interface.MapUI;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(IngamePlayerSettings))]
internal static class IngamePlayerSettingsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("UpdateGameToMatchSettings")]
    private static void UpdateGameToMatchSettingsPostfixPatch(IngamePlayerSettings __instance)
    {
        // Update cameras after possible resolution change
        PlayerManager.UpdateCameras();
    }
}
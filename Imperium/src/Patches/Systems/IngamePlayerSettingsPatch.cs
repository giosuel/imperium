#region

using HarmonyLib;
using Imperium.Core.Lifecycle;

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
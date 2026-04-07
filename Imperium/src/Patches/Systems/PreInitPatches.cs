#region

using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace Imperium.Patches.Systems;

internal static class PreInitPatches
{
    [HarmonyPatch(typeof(PreInitSceneScript))]
    internal static class PreInitSceneScriptPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SkipToFinalSetting")]
        private static bool SkipToFinalSettingPatch(PreInitSceneScript __instance)
        {
            if (!Imperium.Settings.Preferences.QuickloadAutoLaunch.Value) return true;

            var launchMode = Imperium.Settings.Preferences.QuickloadLaunchMode.Value.ToString();
            Imperium.IO.LogInfo($"[SYS] Quickload is bypassing start-up and is loading into '{launchMode}'.");
            __instance.launchSettingsPanelsContainer.SetActive(false);

            Imperium.StartupManager.ExecuteAutoLaunch();

            return false;
        }
    }

    // Makes it so the game instantly loads a save when main menu loads
    [HarmonyPatch(typeof(MenuManager))]
    internal static class MenuManagerPatch
    {
        private static bool HasLoaded;

        [HarmonyPostfix]
        [HarmonyPatch("OnEnable")]
        internal static void OnEnablePatch(MenuManager __instance)
        {
            if (!Imperium.Settings.Preferences.QuickloadAutoLoad.Value || HasLoaded) return;

            if (__instance.menuButtons != null && __instance.menuButtons.name == "MainButtons") {
                __instance.lobbyNameInputField.text = "Imperium Test Environment";

                var saveNum = Imperium.Settings.Preferences.QuickloadSaveNumber.Value;
                Imperium.IO.LogInfo($"[SYS] Quickload is auto loading save #{saveNum}...");

                var fileName = $"LCSaveFile{saveNum}";

                if (Imperium.Settings.Preferences.QuickloadCleanSave.Value && ES3.FileExists(fileName))
                {
                    ES3.DeleteFile(fileName);
                }

                GameNetworkManager.Instance.currentSaveFileName = fileName;
                GameNetworkManager.Instance.saveFileNum = Imperium.Settings.Preferences.QuickloadSaveNumber.Value;

                __instance.ConfirmHostButton();
                HasLoaded = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void AwakePatch(MenuManager __instance)
        {
            if (GameNetworkManager.Instance != null && __instance.versionNumberText != null)
            {
                if (!__instance.versionNumberText.text.Contains(PluginInfo.PLUGIN_NAME))
                {
                    __instance.versionNumberText.text =
                        $"{__instance.versionNumberText.text} ({PluginInfo.PLUGIN_NAME} {PluginInfo.PLUGIN_VERSION})";
                    __instance.versionNumberText.margin = new Vector4(0, 0, -300, 0);
                    var launchedInLanText = __instance.launchedInLanModeText.GetComponent<RectTransform>();
                    launchedInLanText.anchoredPosition = launchedInLanText.anchoredPosition with
                    {
                        x = launchedInLanText.anchoredPosition.x + 185
                    };
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameNetworkManager))]
    internal static class GameNetworkManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("SteamMatchmaking_OnLobbyCreated")]
        private static void SteamMatchmaking_OnLobbyCreatedPatch(ref Steamworks.Data.Lobby lobby) => lobby.SetData("imperium", PluginInfo.PLUGIN_VERSION);
    }
}
#region

using HarmonyLib;
using Imperium.Core;
using Imperium.Netcode;
using Imperium.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace Imperium.Patches.Systems;

internal static class PreInitPatches
{
    // Stores if the main menu is loaded from quitting
    internal static bool ReturnedFromGame;

    // Makes it so the game instantly goes into main menu on start-up
    [HarmonyPatch(typeof(PreInitSceneScript))]
    internal static class PreInitSceneScriptPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("SkipToFinalSetting")]
        private static void SkipToFinalSettingPatch(IngamePlayerSettings __instance)
        {
            if (ImpSettings.Preferences.QuickloadSkipStart.Value &&
                (!ReturnedFromGame || ImpSettings.Preferences.QuickloadOnQuit.Value))
            {
                Imperium.Log.LogInfo("[SYS] Quickload is bypassing start-up sequence...");
                SceneManager.LoadScene("InitScene");
            }
        }
    }

    // Makes it so the game instantly loads a save when main menu loads
    [HarmonyPatch(typeof(MenuManager))]
    internal static class MenuManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void StartPatch(MenuManager __instance)
        {
            if (ImpSettings.Preferences.QuickloadSkipMenu.Value &&
                (!ReturnedFromGame || ImpSettings.Preferences.QuickloadOnQuit.Value))
            {
                var saveNum = ImpSettings.Preferences.QuickloadSaveNumber.Value;
                Imperium.Log.LogInfo($"[SYS] Quickload is loading level #{saveNum}...");

                var fileName = $"LCSaveFile{saveNum}";

                if (ImpSettings.Preferences.QuickloadCleanFile.Value && ES3.FileExists(fileName))
                    ES3.DeleteFile(fileName);

                GameNetworkManager.Instance.currentSaveFileName = fileName;
                GameNetworkManager.Instance.saveFileNum = ImpSettings.Preferences.QuickloadSaveNumber.Value;
                GameNetworkManager.Instance.lobbyHostSettings =
                    new HostSettings("Imperium Test Environment", false);

                GameNetworkManager.Instance.StartHost();
                ImpNetworkManager.IsHost.Set(true);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetLoadingScreen")]
        private static bool SetLoadingScreenPatch(MenuManager __instance)
        {
            return !ImpSettings.Preferences.QuickloadSkipMenu.Value || ReturnedFromGame;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Awake")]
        private static void AwakePatch(MenuManager __instance)
        {
            if (GameNetworkManager.Instance != null && __instance.versionNumberText != null)
            {
                if (!__instance.versionNumberText.text.Contains(Imperium.PLUGIN_NAME))
                {
                    __instance.versionNumberText.text =
                        $"{__instance.versionNumberText.text} ({Imperium.PLUGIN_NAME} {Imperium.PLUGIN_VERSION})";
                    __instance.versionNumberText.margin = new Vector4(0, 0, -300, 0);
                }
            }
        }
    }
}
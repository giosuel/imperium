#region

using HarmonyLib;
using Imperium.Core;
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
        private static void SkipToFinalSettingPatch(PreInitSceneScript __instance)
        {
            if (Imperium.Settings.Preferences.QuickloadSkipStart.Value &&
                (!ReturnedFromGame || Imperium.Settings.Preferences.QuickloadOnQuit.Value))
            {
                Imperium.IO.LogInfo("[SYS] Quickload is bypassing start-up sequence...");
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
            if (Imperium.Settings.Preferences.QuickloadSkipMenu.Value &&
                (!ReturnedFromGame || Imperium.Settings.Preferences.QuickloadOnQuit.Value))
            {
                var saveNum = Imperium.Settings.Preferences.QuickloadSaveNumber.Value;
                Imperium.IO.LogInfo($"[SYS] Quickload is loading level #{saveNum}...");

                var fileName = $"LCSaveFile{saveNum}";

                if (Imperium.Settings.Preferences.QuickloadCleanFile.Value && ES3.FileExists(fileName))
                    ES3.DeleteFile(fileName);

                GameNetworkManager.Instance.currentSaveFileName = fileName;
                GameNetworkManager.Instance.saveFileNum = Imperium.Settings.Preferences.QuickloadSaveNumber.Value;
                GameNetworkManager.Instance.lobbyHostSettings =
                    new HostSettings("Imperium Test Environment", false);

                GameNetworkManager.Instance.StartHost();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("SetLoadingScreen")]
        private static bool SetLoadingScreenPatch(MenuManager __instance)
        {
            return !Imperium.Settings.Preferences.QuickloadSkipMenu.Value || ReturnedFromGame;
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
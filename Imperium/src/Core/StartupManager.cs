#region

using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Core;

public class StartupManager
{
    private bool HasLaunched;

    internal StartupManager()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (Imperium.Settings.Preferences.QuickloadSkipSplash.Value) RunSplashRemover();
    }

    internal void ExecuteAutoLaunch()
    {
        if (HasLaunched) return;
        HasLaunched = true;

        switch (Imperium.Settings.Preferences.QuickloadLaunchMode.Value)
        {
            case LaunchMode.LAN:
                SceneManager.LoadScene("InitSceneLANMode");
                break;
            case LaunchMode.Online:
            default:
                SceneManager.LoadScene("InitScene");
                break;
        }
    }

    private static void SkipBootAnimation()
    {
        var game = Object.FindObjectOfType<InitializeGame>();
        if (game == null) return;

        game.runBootUpScreen = false;
        game.bootUpAnimation = null;
        game.bootUpAudio = null;
    }

    private static void SkipLanPopup()
    {
        var obj = Object.FindObjectOfType<MenuManager>();
        if (obj == null) return;

        Object.Destroy(obj.lanWarningContainer);
    }

    private static void SkipMenuAnimation()
    {
        GameNetworkManager.Instance.firstTimeInMenu = false;
    }

    private void RunSplashRemover()
    {
        Task.Factory.StartNew(() =>
        {
            while (!HasLaunched)
            {
                SplashScreen.Stop(SplashScreen.StopBehavior.StopImmediate);
                if (Time.realtimeSinceStartup < 15) continue;
                break;
            }
        });
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!Imperium.Settings.Preferences.QuickloadSkipSplash.Value) return;

        switch (scene.name)
        {
            case "InitScene":
            case "InitSceneLANMode":
                SkipBootAnimation();
                break;
            case "MainMenu":
                SkipMenuAnimation();
                SkipLanPopup();
                break;
        }
    }
}

internal enum LaunchMode
{
    Online,
    LAN
}
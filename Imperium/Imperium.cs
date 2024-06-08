#region

using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Core;
using Imperium.Integration;
using Imperium.MonoBehaviours;
using Imperium.MonoBehaviours.ImpUI.ImperiumUI;
using Imperium.MonoBehaviours.ImpUI.MapUI;
using Imperium.MonoBehaviours.ImpUI.MinimapSettings;
using Imperium.MonoBehaviours.ImpUI.MoonUI;
using Imperium.MonoBehaviours.ImpUI.NavigatorUI;
using Imperium.MonoBehaviours.ImpUI.ObjectsUI;
using Imperium.MonoBehaviours.ImpUI.OracleUI;
using Imperium.MonoBehaviours.ImpUI.RenderingUI;
using Imperium.MonoBehaviours.ImpUI.SaveUI;
using Imperium.MonoBehaviours.ImpUI.SettingsUI;
using Imperium.MonoBehaviours.ImpUI.SpawningUI;
using Imperium.MonoBehaviours.ImpUI.TeleportUI;
using Imperium.MonoBehaviours.ImpUI.VisualizationUI;
using Imperium.MonoBehaviours.ImpUI.WeatherUI;
using Imperium.MonoBehaviours.VisualizerObjects.NoiseOverlay;
using Imperium.Netcode;
using Imperium.Patches.Objects;
using Imperium.Patches.Systems;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium;

[BepInDependency("com.sinai.unityexplorer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.sinai.universelib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Imperium : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "giosuel.Imperium";
    public const string PLUGIN_NAME = "Imperium";
    public const string PLUGIN_VERSION = "0.1.9";

    internal static ManualLogSource Log;
    internal static ConfigFile ConfigFile;

    // Global relays for game singletons to keep track of dependencies
    internal static Terminal Terminal;
    internal static HUDManager HUDManager;
    internal static PlayerControllerB Player;
    internal static TimeOfDay TimeOfDay => TimeOfDay.Instance;
    internal static IngamePlayerSettings IngamePlayerSettings => IngamePlayerSettings.Instance;
    internal static StartOfRound StartOfRound => StartOfRound.Instance;
    internal static RoundManager RoundManager => RoundManager.Instance;
    internal static ShipBuildModeManager ShipBuildModeManager => ShipBuildModeManager.Instance;

    // Imperium game and lifecycle managers
    internal static GameManager GameManager;
    internal static ObjectManager ObjectManager;
    internal static PlayerManager PlayerManager;
    internal static Visualization Visualization;
    internal static Oracle Oracle;

    // Other Imperium objects
    internal static ImpMap Map;
    internal static ImpFreecam Freecam;
    internal static ImpNightVision NightVision;
    internal static ImpNoiseListener NoiseListener;
    internal static ImpInputBindings InputBindings;
    internal static ImpPositionIndicator ImpPositionIndicator;
    internal static ImpInterfaceManager Interface;

    internal static Harmony Harmony;

    // Global variable indicating if Imperium is loaded
    internal static bool IsImperiumReady;

    internal static bool IsImperiumLaunched;

    // Indicates if Imperium access was initially granted when the client joined the lobby
    internal static bool WasImperiumAccessGranted;

    // Global variable indicating if ship is currently landed on a moon
    internal static ImpBinaryBinding IsSceneLoaded;

    internal static ImpBinding<ImpTheme> Theme;

    private void Awake()
    {
        Log = Logger;
        ConfigFile = Config;

        if (!ImpAssets.Load()) return;

        Harmony = new Harmony(PLUGIN_GUID);
        PreLaunchPatch();
        RunNetcodePatcher();

        IsImperiumReady = true;
        Log.LogInfo("[OK] Imperium is ready!");
    }

    private static void RunNetcodePatcher()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                if (attributes.Length > 0)
                {
                    method.Invoke(null, null);
                }
            }
        }
    }

    internal static void DisableImperium()
    {
        if (!IsImperiumLaunched) return;

        Interface.Close();
        InputBindings.BaseMap.Disable();
        InputBindings.FreecamMap.Disable();
        InputBindings.SpawningMap.Disable();
        Interface.StopListening();
    }

    internal static void EnableImperium()
    {
        if (!IsImperiumLaunched) return;

        InputBindings.BaseMap.Enable();
        InputBindings.FreecamMap.Enable();
        InputBindings.SpawningMap.Enable();
        Interface.StartListening();
    }

    internal static void Launch()
    {
        if (!IsImperiumReady)
        {
            ImpOutput.Send("Imperium failed to launch \u2299︿\u2299");
            return;
        }

        IsImperiumLaunched = true;

        InputBindings = new ImpInputBindings();
        Terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
        HUDManager = FindObjectOfType<HUDManager>();

        IsSceneLoaded = new ImpBinaryBinding(false);
        Theme = new ImpBinding<ImpTheme>(ImpThemeManager.DefaultTheme);

        Map = ImpMap.Create();
        Freecam = ImpFreecam.Create();
        NightVision = ImpNightVision.Create();
        Interface = ImpInterfaceManager.Create(Theme);
        NoiseListener = ImpNoiseListener.Create();
        ImpPositionIndicator = ImpPositionIndicator.Create();

        PlayerManager = new PlayerManager(IsSceneLoaded, ImpNetworkManager.ConnectedPlayers, Freecam);
        GameManager = new GameManager(IsSceneLoaded, ImpNetworkManager.ConnectedPlayers);
        ObjectManager = new ObjectManager(IsSceneLoaded, ImpNetworkManager.ConnectedPlayers);
        Oracle = new Oracle();
        Visualization = new Visualization(Oracle.State, ObjectManager);

        MoonManager.Create(ObjectManager);

        GameManager.IndoorSpawningPaused.onTrigger += Oracle.Simulate;
        GameManager.OutdoorSpawningPaused.onTrigger += Oracle.Simulate;
        GameManager.DaytimeSpawningPaused.onTrigger += Oracle.Simulate;
        GameManager.IndoorDeviation.onTrigger += Oracle.Simulate;
        GameManager.DaytimeDeviation.onTrigger += Oracle.Simulate;
        GameManager.MaxIndoorPower.onTrigger += Oracle.Simulate;
        GameManager.MaxOutdoorPower.onTrigger += Oracle.Simulate;
        GameManager.MaxDaytimePower.onTrigger += Oracle.Simulate;
        GameManager.MinIndoorSpawns.onTrigger += Oracle.Simulate;
        GameManager.MinOutdoorSpawns.onTrigger += Oracle.Simulate;

        Interface.OpenInterface.onUpdate += openInterface =>
        {
            if (openInterface) ImpPositionIndicator.HideIndicator();
        };

        ObjectManager.CurrentPlayers.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelEntities.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelItems.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelLandmines.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelTurrets.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelSpiderWebs.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelBreakerBoxes.onTrigger += Visualization.RefreshOverlays;

        InputBindings.BaseMap["ToggleHUD"].performed += ToggleHUD;

        ImpSettings.LoadAll();
        PlayerManager.UpdateCameras();

        // Network syncing
        ImpNetTime.Instance.BindNetworkVariables();

        // Patch the rest of the functionality at the end to make sure all the dependencies of the static patch
        // functions are loaded
        Harmony.PatchAll();
        UnityExplorerIntegration.PatchFunctions(Harmony);

        SpawnUI();

        if (!NetworkManager.Singleton.IsHost) ImpNetTime.Instance.RequestTimeServerRpc();
    }

    private static void ToggleHUD(InputAction.CallbackContext callbackContext)
    {
        if (Player.quickMenuManager.isMenuOpen ||
            Player.inTerminalMenu ||
            Player.isTypingChat ||
            ShipBuildModeManager.InBuildMode) return;

        HUDManager.HideHUD(!Reflection.Get<HUDManager, bool>(HUDManager, "hudHidden"));
    }

    internal static void Unload()
    {
        Harmony.UnpatchSelf();

        DisableImperium();

        WasImperiumAccessGranted = false;
        IsImperiumLaunched = false;

        ImpSettings.Reinstantiate();

        PreLaunchPatch();
    }

    internal static void Reload()
    {
        Unload();
        Launch();
    }

    private static void SpawnUI()
    {
        Interface.Register<SettingsUI, ImperiumUI>(ImpAssets.SettingsUIObject);
        Interface.Register<ConfirmationUI, ImperiumUI>(ImpAssets.ConfirmationUIObject);
        Interface.Register<SaveUI, ImperiumUI>(ImpAssets.SaveUIObject);
        Interface.Register<ObjectsUI, ImperiumUI>(ImpAssets.ObjectsUIObject);
        Interface.Register<MoonUI, ImperiumUI>(ImpAssets.MoonUIObject);
        Interface.Register<RenderingUI, ImperiumUI>(ImpAssets.RenderingUIObject);
        Interface.Register<VisualizationUI>(ImpAssets.VisualizerUIObject);
        Interface.Register<MinimapSettings>(ImpAssets.MinimapSettingsObject);
        Interface.Register<ImperiumUI>(ImpAssets.ImperiumUIObject, "<Keyboard>/F1");
        Interface.Register<SpawningUI>(ImpAssets.SpawningUIObject, "<Keyboard>/F2", closeOnMovement: false);
        Interface.Register<TeleportUI>(ImpAssets.TeleportUIObject, "<Keyboard>/F3", closeOnMovement: false);
        Interface.Register<WeatherUI>(ImpAssets.WeatherUIObject, "<Keyboard>/F4");
        Interface.Register<OracleUI>(ImpAssets.OracleUIObject, "<Keyboard>/F5");
        Interface.Register<NavigatorUI>(ImpAssets.NavigatorUIObject, "<Keyboard>/F6");
        Interface.Register<MapUI>(ImpAssets.MapUIObject, "<Keyboard>/F8");

        Interface.StartListening();

        ImpThemeManager.BindTheme(ImpSettings.Preferences.Theme, Theme);

        Log.LogInfo("[OK] Imperium UIs have been registered! \\o/");
    }

    private static void PreLaunchPatch()
    {
        Harmony.PatchAll(typeof(PlayerControllerPatch.PreloadPatches));
        Harmony.PatchAll(typeof(StartOfRoundPatch.PreloadPatches));
        Harmony.PatchAll(typeof(GameNetworkManagerPatch.PreloadPatches));
        Harmony.PatchAll(typeof(TerminalPatch.PreloadPatches));

        Harmony.PatchAll(typeof(PreInitPatches.PreInitSceneScriptPatch));
        Harmony.PatchAll(typeof(PreInitPatches.MenuManagerPatch));
    }
}
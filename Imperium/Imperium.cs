#region

using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Core;
using Imperium.Core.Lifecycle;
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
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium;

[BepInDependency("com.sinai.unityexplorer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.sinai.universelib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("LethalNetworkAPI")]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Imperium : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "giosuel.Imperium";
    public const string PLUGIN_NAME = "Imperium";
    public const string PLUGIN_VERSION = "0.1.9";

    private static ConfigFile configFile;
    private static Harmony Harmony;

    /*
     * Relays to vanilla singletons
     */
    internal static Terminal Terminal;
    internal static HUDManager HUDManager;
    internal static PlayerControllerB Player;
    internal static TimeOfDay TimeOfDay => TimeOfDay.Instance;
    internal static IngamePlayerSettings IngamePlayerSettings => IngamePlayerSettings.Instance;
    internal static StartOfRound StartOfRound => StartOfRound.Instance;
    internal static RoundManager RoundManager => RoundManager.Instance;
    internal static ShipBuildModeManager ShipBuildModeManager => ShipBuildModeManager.Instance;

    /*
     * Preload systems. Loaded when Imperium is loaded by BepInEx.
     */
    internal static ImpSettings Settings;
    internal static ImpOutput IO;
    internal static ImpNetworking Networking;

    /*
     * Lifecycle systems. Loaded when Imperium is launched.
     */
    internal static GameManager GameManager;
    internal static ObjectManager ObjectManager;
    internal static PlayerManager PlayerManager;
    internal static MoonManager MoonManager;
    internal static ShipManager ShipManager;
    internal static Visualization Visualization;
    internal static Oracle Oracle;

    /*
     * GameObjects and world-space managers. Loaded when Imperium is launched.
     */
    internal static ImpMap Map;
    internal static ImpFreecam Freecam;
    internal static ImpNightVision NightVision;
    internal static ImpNoiseListener NoiseListener;
    internal static ImpInputBindings InputBindings;
    internal static ImpPositionIndicator ImpPositionIndicator;
    internal static ImpInterfaceManager Interface;

    /// <summary>
    /// Set to true, then Imperium is initally loaded by BepInEx.
    /// </summary>
    internal static bool IsImperiumReady;

    /// <summary>
    /// Set to true, then Imperium is launched and ready be used and serve API calls.
    /// </summary>
    internal static bool IsImperiumLaunched;

    /// <summary>
    /// Set to true, when Imperium access is first granted. Always set to true the host.
    /// </summary>
    internal static bool WasImperiumAccessGranted;

    /// <summary>
    /// Binding that updates whenever the scene ship lands and takes off.
    /// </summary>
    internal static ImpBinaryBinding IsSceneLoaded;

    internal static ImpBinding<ImpTheme> Theme;

    private void Awake()
    {
        configFile = Config;

        Settings = new ImpSettings(Config);
        IO = new ImpOutput(Logger);
        Networking = new ImpNetworking(Settings.Preferences.AllowClients);

        if (!ImpAssets.Load()) return;

        Harmony = new Harmony(PLUGIN_GUID);
        PreLaunchPatch();

        IO.LogInfo("[OK] Imperium is ready!");

        IsImperiumReady = true;
    }

    // private static void RunNetcodePatcher()
    // {
    //     var types = Assembly.GetExecutingAssembly().GetTypes();
    //     foreach (var type in types)
    //     {
    //         var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
    //         foreach (var method in methods)
    //         {
    //             var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
    //             if (attributes.Length > 0)
    //             {
    //                 method.Invoke(null, null);
    //             }
    //         }
    //     }
    // }

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
            IO.Send("Imperium failed to launch \u2299︿\u2299");
            return;
        }

        InputBindings = new ImpInputBindings();
        Terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
        HUDManager = FindObjectOfType<HUDManager>();

        IsSceneLoaded = new ImpBinaryBinding(false);
        Theme = new ImpBinding<ImpTheme>(ImpThemeManager.DefaultTheme);

        Map = ImpMap.Create();
        Freecam = ImpFreecam.Create();
        Interface = ImpInterfaceManager.Create(Theme);
        NightVision = ImpNightVision.Create();
        NoiseListener = ImpNoiseListener.Create();
        ImpPositionIndicator = ImpPositionIndicator.Create();

        Oracle = new Oracle();

        GameManager = new GameManager(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        MoonManager = new MoonManager(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        ShipManager = new ShipManager(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        ObjectManager = new ObjectManager(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        PlayerManager = new PlayerManager(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        Visualization = new Visualization(Oracle.State, ObjectManager, configFile);

        MoonContainer.Create(ObjectManager);

        MoonManager.IndoorSpawningPaused.onTrigger += Oracle.Simulate;
        MoonManager.OutdoorSpawningPaused.onTrigger += Oracle.Simulate;
        MoonManager.DaytimeSpawningPaused.onTrigger += Oracle.Simulate;
        MoonManager.IndoorDeviation.onTrigger += Oracle.Simulate;
        MoonManager.DaytimeDeviation.onTrigger += Oracle.Simulate;
        MoonManager.MaxIndoorPower.onTrigger += Oracle.Simulate;
        MoonManager.MaxOutdoorPower.onTrigger += Oracle.Simulate;
        MoonManager.MaxDaytimePower.onTrigger += Oracle.Simulate;
        MoonManager.MinIndoorSpawns.onTrigger += Oracle.Simulate;
        MoonManager.MinOutdoorSpawns.onTrigger += Oracle.Simulate;

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

        Settings.LoadAll();
        PlayerManager.UpdateCameras();

        // Patch the rest of the functionality at the end to make sure all the dependencies of the static patch
        // functions are loaded
        Harmony.PatchAll();
        UnityExplorerIntegration.PatchFunctions(Harmony);

        IsImperiumLaunched = true;

        SpawnUI();
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

        // Imperium.Settings.Reinstantiate();

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

        ImpThemeManager.BindTheme(Imperium.Settings.Preferences.Theme, Theme);

        IO.LogInfo("[OK] Imperium UIs have been registered! \\o/");
    }

    private static void PreLaunchPatch()
    {
        Harmony.PatchAll(typeof(PlayerControllerPatch.PreloadPatches));
        Harmony.PatchAll(typeof(StartOfRoundPatch.PreloadPatches));
        Harmony.PatchAll(typeof(TerminalPatch.PreloadPatches));

        Harmony.PatchAll(typeof(PreInitPatches.PreInitSceneScriptPatch));
        Harmony.PatchAll(typeof(PreInitPatches.MenuManagerPatch));
    }
}
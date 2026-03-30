#region

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.API.Types.Networking;
using Imperium.Core;
using Imperium.Core.EventLogging;
using Imperium.Core.Input;
using Imperium.Core.LevelEditor;
using Imperium.Core.Lifecycle;
using Imperium.Core.Scripts;
using Imperium.Integration;
using Imperium.Interface.ImperiumUI;
using Imperium.Interface.MapUI;
using Imperium.Interface.OracleUI;
using Imperium.Interface.SpawningUI;
using Imperium.Netcode;
using Imperium.Patches.Objects;
using Imperium.Patches.Systems;
using Imperium.Util;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects.NoiseOverlay;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium;

[BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.sinai.universelib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.fumiko.CullFactory", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.sinai.unityexplorer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("imabatby.lethallevelloader", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.rune580.LethalCompanyInputUtils")]
[BepInDependency("LethalNetworkAPI")]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Imperium : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "giosuel.Imperium";
    public const string PLUGIN_NAME = "Imperium";
    public const string PLUGIN_VERSION = "1.1.1";

    private static Harmony Harmony;
    private static ManualLogSource Log;
    private static ConfigFile configFile;

    /*
     * Relays to vanilla singletons. This makes tracking Imperium singleton access easier.
     */
    internal static Terminal Terminal { get; private set; }
    internal static HUDManager HUDManager { get; private set; }
    internal static PlayerControllerB Player { get; set; }
    internal static TimeOfDay TimeOfDay => TimeOfDay.Instance;
    internal static IngamePlayerSettings IngamePlayerSettings => IngamePlayerSettings.Instance;
    internal static StartOfRound StartOfRound => StartOfRound.Instance;
    internal static RoundManager RoundManager => RoundManager.Instance;
    internal static ShipBuildModeManager ShipBuildModeManager => ShipBuildModeManager.Instance;

    /*
     * Preload systems. Instantiated when Imperium is initialized (Stage 1).
     */
    internal static ImpSettings Settings { get; private set; }
    internal static ImpOutput IO { get; private set; }
    internal static ImpNetworking Networking { get; set; }
    internal static StartupManager StartupManager { get; private set; }

    /*
     * Lifecycle systems. Instantiated when Imperium is launched (Stage 2).
     */
    internal static GameManager GameManager { get; private set; }
    internal static ObjectManager ObjectManager { get; private set; }
    internal static PlayerManager PlayerManager { get; private set; }
    internal static MoonManager MoonManager { get; private set; }
    internal static ShipManager ShipManager { get; private set; }
    internal static CruiserManager CruiserManager { get; private set; }
    internal static Visualization Visualization { get; private set; }
    internal static Oracle Oracle { get; private set; }
    internal static ImpEventLog EventLog { get; private set; }

    /*
     * Imperium game objects and world-space managers. Instantiated when Imperium is launched (Stage 2).
     */
    internal static ImpMap Map { get; private set; }
    internal static ImpFreecam Freecam { get; private set; }
    internal static ImpNightVision NightVision { get; private set; }
    internal static ImpNoiseListener NoiseListener { get; private set; }
    internal static ImpTapeMeasure ImpTapeMeasure { get; private set; }
    internal static ImpLevelEditor ImpLevelEditor { get; private set; }
    internal static ImpInputBindings InputBindings { get; private set; }
    internal static ImpPositionIndicator ImpPositionIndicator { get; private set; }
    internal static ImpInterfaceManager Interface { get; private set; }
    internal static WaypointManager WaypointManager { get; private set; }

    /// <summary>
    ///     Set to true, then Imperium is initialized (Stage 1).
    /// </summary>
    internal static bool IsImperiumInitialized { get; private set; }

    /// <summary>
    ///     Set to true, then Imperium is launched (Stage 2) and ready to serve API calls.
    /// </summary>
    internal static bool IsImperiumLaunched { get; private set; }

    /// <summary>
    ///     Set to true, when Imperium is launched and imperium access is currently granted.
    /// </summary>
    internal static bool IsImperiumEnabled { get; private set; }

    /// <summary>
    ///     Binding that updates whenever the scene ship lands and takes off.
    /// </summary>
    internal static ImpBinaryBinding IsSceneLoaded { get; private set; }

    /// <summary>
    /// Imperium initialization (Stage 1)
    ///
    /// This happens as soon as BepInEx loads the Imperium plugin.
    /// </summary>
    private void Awake()
    {
        configFile = Config;
        Log = Logger;

        /*
         * Temporary settings instance for startup functionality.
         * This object will be re-instantiated once Imperium launches, meaning all listeners will be removed!
         */
        Settings = new ImpSettings(Config);

        IO = new ImpOutput(Log);
        StartupManager = new StartupManager();

        InputBindings = new ImpInputBindings();
        InputBindings.BaseMap.Disable();
        InputBindings.StaticMap.Disable();
        InputBindings.FreecamMap.Disable();
        InputBindings.InterfaceMap.Disable();

        if (!ImpAssets.Load()) return;

        Harmony = new Harmony(PLUGIN_GUID);
        PreLaunchPatches();

        IO.LogInfo("[INIT] Imperium has been successfully initialized \\o/");

        IsImperiumInitialized = true;
    }

    internal static void DisableImperium()
    {
        IsImperiumEnabled = false;

        Interface.Destroy();
        PlayerManager.IsFlying.SetFalse();
        Freecam.IsFreecamEnabled.SetFalse();

        InputBindings.BaseMap.Disable();
        InputBindings.StaticMap.Disable();
        InputBindings.FreecamMap.Disable();
        InputBindings.InterfaceMap.Disable();
    }

    internal static void EnableImperium()
    {
        if (!IsImperiumLaunched) return;

        InputBindings.BaseMap.Enable();
        InputBindings.StaticMap.Enable();
        InputBindings.FreecamMap.Enable();
        InputBindings.InterfaceMap.Enable();

        IsImperiumEnabled = true;

        Settings.LoadAll();
        RegisterInterfaces();
        PlayerManager.UpdateCameras();
    }

    /// <summary>
    /// Imperium launch (Stage 2)
    ///
    /// This is executed after Imperium access has been granted by the host.
    /// </summary>
    internal static void Launch()
    {
        if (!IsImperiumInitialized) return;

        // Re-instantiate settings to get rid of existing bindings
        Settings = new ImpSettings(configFile);
        IO.BindNotificationSettings(Settings);
        Networking.BindAllowClients(Settings.Preferences.AllowClients);

        Terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
        HUDManager = FindObjectOfType<HUDManager>();

        IsSceneLoaded = new ImpBinaryBinding(false);

        Interface = ImpInterfaceManager.Create(Settings.Preferences.Theme);

        EventLog = new ImpEventLog();

        Oracle = ImpLifecycleObject.Create<Oracle>(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        GameManager = ImpLifecycleObject.Create<GameManager>(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        MoonManager = ImpLifecycleObject.Create<MoonManager>(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        ShipManager = ImpLifecycleObject.Create<ShipManager>(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        CruiserManager = ImpLifecycleObject.Create<CruiserManager>(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        ObjectManager = ImpLifecycleObject.Create<ObjectManager>(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        PlayerManager = ImpLifecycleObject.Create<PlayerManager>(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        WaypointManager = ImpLifecycleObject.Create<WaypointManager>(IsSceneLoaded, ImpNetworking.ConnectedPlayers);
        Visualization = new Visualization(Oracle.State, ObjectManager, configFile);

        Map = ImpMap.Create();
        Freecam = ImpFreecam.Create();
        NightVision = ImpNightVision.Create();
        ImpTapeMeasure = ImpTapeMeasure.Create();
        NoiseListener = ImpNoiseListener.Create();
        ImpPositionIndicator = ImpPositionIndicator.Create();

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

        // Patch the rest of the functionality at the end to make sure all the dependencies of the static patch
        // functions are loaded
        Harmony.PatchAll();
        UnityExplorerIntegration.PatchFunctions(Harmony);

        IsImperiumLaunched = true;
        
        // Enable Imperium frontend if Imperium is enabled in the config
        if (Settings.Preferences.EnableImperium.Value)
        {
            EnableImperium();

            // #if DEBUG
            // This needs to be here as it depends on the UI
            // ImpLevelEditor = ImpLevelEditor.Create();
            // #endif

            // Send scene update to ensure consistency in the UIs
            IsSceneLoaded.SetFalse();
        }
        else
        {
            DisableImperium();
        }
    }

    internal static void Unload()
    {
        if (!IsImperiumLaunched) return;

        Harmony.UnpatchSelf();

        DisableImperium();

        Networking.Unsubscribe();

        IsImperiumLaunched = false;

        PreLaunchPatches();
    }

    internal static void Reload()
    {
        Unload();
        Launch();

        IO.Send("[SYS] Successfully reloaded Imperium.");
    }

    private static void RegisterInterfaces()
    {
        Interface.OpenInterface.onUpdate += openInterface =>
        {
            if (openInterface) ImpPositionIndicator.Deactivate();
        };

        Interface.RegisterInterface<ImperiumUI>(
            ImpAssets.ImperiumUIObject,
            "ImperiumUI",
            "Imperium UI",
            "Imperium's main interface.",
            InputBindings.InterfaceMap.ImperiumUI
        );
        Interface.RegisterInterface<SpawningUI>(
            ImpAssets.SpawningUIObject,
            "SpawningUI",
            "Spawning",
            "Allows you to spawn objects\nsuch as Scrap or Entities.",
            InputBindings.InterfaceMap.SpawningUI
        );
        Interface.RegisterInterface<MapUI>(
            ImpAssets.MapUIObject,
            "MapUI",
            "Map",
            "Imperium's built-in map.",
            InputBindings.InterfaceMap.MapUI
        );
        Interface.RegisterInterface<OracleUI>(
            ImpAssets.OracleUIObject,
            "OracleUI",
            "Oracle",
            "Entity spawning predictions.",
            InputBindings.InterfaceMap.OracleUI,
            IsSceneLoaded
        );
        Interface.RegisterInterface<MinimapSettings>(ImpAssets.MinimapSettingsObject);
        // Interface.RegisterInterface<ComponentManager>(ImpAssets.ComponentManagerObject);

        Interface.RefreshTheme();

        IO.LogInfo("[SYS] Imperium interfaces have been registered! \\o/");
    }

    private static void PreLaunchPatches()
    {
        Harmony.PatchAll(typeof(PlayerControllerPatch.PreloadPatches));
        Harmony.PatchAll(typeof(TerminalPatch.PreloadPatches));

        Harmony.PatchAll(typeof(PreInitPatches.PreInitSceneScriptPatch));
        Harmony.PatchAll(typeof(PreInitPatches.MenuManagerPatch));
        Harmony.PatchAll(typeof(PreInitPatches.GameNetworkManagerPatch));
    }
}
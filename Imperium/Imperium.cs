#region

using BepInEx;
using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
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

[BepInDependency("com.sinai.unityexplorer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.sinai.universelib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("evaisa.lethallib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("mrov.WeatherRegistry", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.rune580.LethalCompanyInputUtils")]
[BepInDependency("LethalNetworkAPI")]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Imperium : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "giosuel.Imperium";
    public const string PLUGIN_NAME = "Imperium";
    public const string PLUGIN_VERSION = "0.2.5";

    private static ConfigFile configFile;
    private static Harmony Harmony;

    /*
     * Relays to vanilla singletons.
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
     * Preload systems. Instantiated when Imperium is loaded by BepInEx.
     */
    internal static ImpSettings Settings { get; private set; }
    internal static ImpOutput IO { get; private set; }
    internal static ImpNetworking Networking { get; set; }

    /*
     * Lifecycle systems. Instantiated when Imperium is launched.
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
     * GameObjects and world-space managers. Instantiated when Imperium is launched.
     */
    internal static ImpMap Map { get; private set; }
    internal static ImpFreecam Freecam { get; private set; }
    internal static ImpNightVision NightVision { get; private set; }
    internal static ImpNoiseListener NoiseListener { get; private set; }
    internal static ImpTapeMeasure ImpTapeMeasure { get; private set; }
    internal static ImpLevelEditor ImpLevelEditor { get; }
    internal static ImpInputBindings InputBindings { get; private set; }
    internal static ImpPositionIndicator ImpPositionIndicator { get; private set; }
    internal static ImpInterfaceManager Interface { get; private set; }

    /// <summary>
    ///     Set to true, then Imperium is initally loaded by BepInEx.
    /// </summary>
    internal static bool IsImperiumLoaded { get; private set; }

    /// <summary>
    ///     Set to true, then Imperium is launched and ready be used and serve API calls.
    /// </summary>
    internal static bool IsImperiumLaunched { get; private set; }

    /// <summary>
    ///     Set to true, when Imperium access is first granted. Always set to true on the host.
    /// </summary>
    internal static bool WasImperiumAccessGranted { get; private set; }

    /// <summary>
    ///     Set to true, when Imperium is loaded and imperium access is currently granted.
    /// </summary>
    internal static bool IsImperiumEnabled { get; private set; }

    /// <summary>
    ///     Binding that updates whenever the scene ship lands and takes off.
    /// </summary>
    internal static ImpBinaryBinding IsSceneLoaded { get; private set; }

    private void Awake()
    {
        configFile = Config;

        Settings = new ImpSettings(Config);
        IO = new ImpOutput(Logger);

        InputBindings = new ImpInputBindings();
        InputBindings.BaseMap.Disable();
        InputBindings.StaticMap.Disable();
        InputBindings.FreecamMap.Disable();
        InputBindings.InterfaceMap.Disable();

        if (!ImpAssets.Load()) return;

        Harmony = new Harmony(PLUGIN_GUID);
        PreLaunchPatch();

        IO.LogInfo("[OK] Imperium is ready!");

        IsImperiumLoaded = true;
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

        Interface = ImpInterfaceManager.Create(Settings.Preferences.Theme);
        StartUI();

        Settings.LoadAll();

        IsImperiumEnabled = true;
    }

    internal static void Launch()
    {
        if (!IsImperiumLoaded)
        {
            IO.Send("Imperium failed to launch \u2299︿\u2299");
            return;
        }

        // Re-instantiate settings to get rid of existing bindings
        Settings = new ImpSettings(configFile);
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

        // Enable Imperium frontend if Imperium is enabled in the config
        if (Settings.Preferences.EnableImperium.Value)
        {
            InputBindings.BaseMap.ToggleHUD.performed += ToggleHUD;

            InputBindings.BaseMap.Enable();
            InputBindings.StaticMap.Enable();
            InputBindings.FreecamMap.Enable();
            InputBindings.InterfaceMap.Enable();

            IsImperiumEnabled = true;

            Settings.LoadAll();

            StartUI();
// #if DEBUG
//             // This needs to be here as it depends on the UI
//             ImpLevelEditor = ImpLevelEditor.Create();
// #endif

            // Send scene update to ensure consistency in the UIs
            IsSceneLoaded.SetFalse();

            PlayerManager.UpdateCameras();
        }
        else
        {
            InputBindings.BaseMap.Disable();
            InputBindings.StaticMap.Disable();
            InputBindings.FreecamMap.Disable();
            InputBindings.InterfaceMap.Disable();
        }

        WasImperiumAccessGranted = true;
        IsImperiumLaunched = true;
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
        if (!IsImperiumLaunched) return;

        PreInitPatches.ReturnedFromGame = true;

        Harmony.UnpatchSelf();

        DisableImperium();

        Networking.Unsubscribe();

        WasImperiumAccessGranted = false;
        IsImperiumLaunched = false;

        PreLaunchPatch();
    }

    internal static void Reload()
    {
        Unload();
        Launch();
    }

    private static void StartUI()
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

        IO.LogInfo("[OK] Imperium UIs have been registered! \\o/");
    }

    private static void PreLaunchPatch()
    {
        Harmony.PatchAll(typeof(PlayerControllerPatch.PreloadPatches));
        Harmony.PatchAll(typeof(TerminalPatch.PreloadPatches));

        Harmony.PatchAll(typeof(PreInitPatches.PreInitSceneScriptPatch));
        Harmony.PatchAll(typeof(PreInitPatches.MenuManagerPatch));
    }
}
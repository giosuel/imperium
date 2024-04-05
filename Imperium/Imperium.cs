#region

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Core;
using Imperium.Integration;
using Imperium.MonoBehaviours;
using Imperium.MonoBehaviours.ImpUI.ImperiumUI;
using Imperium.MonoBehaviours.ImpUI.MoonUI;
using Imperium.MonoBehaviours.ImpUI.NavigatorUI;
using Imperium.MonoBehaviours.ImpUI.ObjectsUI;
using Imperium.MonoBehaviours.ImpUI.OracleUI;
using Imperium.MonoBehaviours.ImpUI.RenderingUI;
using Imperium.MonoBehaviours.ImpUI.SaveUI;
using Imperium.MonoBehaviours.ImpUI.SettingsUI;
using Imperium.MonoBehaviours.ImpUI.SpawningUI;
using Imperium.MonoBehaviours.ImpUI.TeleportUI;
using Imperium.MonoBehaviours.ImpUI.WeatherUI;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Netcode;
using Imperium.Patches.Objects;
using Imperium.Patches.Systems;
using Imperium.Util;
using Imperium.Util.Binding;
using RuntimeNetcodeRPCValidator;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium;

[BepInDependency(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("com.sinai.unityexplorer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.sinai.universelib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Imperium : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "giosuel.Imperium";
    public const string PLUGIN_NAME = "Imperium";
    public const string PLUGIN_VERSION = "0.1.2";

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

    // Imperium game and lifecycle managers
    internal static GameManager GameManager;
    internal static ObjectManager ObjectManager;
    internal static PlayerManager PlayerManager;
    internal static Visualization Visualization;
    internal static Oracle.Oracle Oracle;

    // Other Imperium objects
    internal static ImpFreecam Freecam;
    internal static ImpInputBindings InputBindings;
    internal static PositionIndicator PositionIndicator;

    internal static ImpInterfaceManager Interface;

    private static Harmony harmony;
    private static NetcodeValidator netcodeValidator;

    // Global variable indicating if Imperium is loaded
    internal static bool IsImperiumReady;

    // Global variable indicating if ship is currently landed on a moon
    internal static ImpBinaryBinding IsSceneLoaded;

    private void Awake()
    {
        Log = Logger;
        ConfigFile = Config;
        if (!ImpAssets.Load()) return;

        netcodeValidator = new NetcodeValidator(PLUGIN_GUID);
        netcodeValidator.PatchAll();
        netcodeValidator.BindToPreExistingObjectByBehaviour<ImpNetSpawning, Terminal>();
        netcodeValidator.BindToPreExistingObjectByBehaviour<ImpNetQuota, Terminal>();
        netcodeValidator.BindToPreExistingObjectByBehaviour<ImpNetTime, Terminal>();
        netcodeValidator.BindToPreExistingObjectByBehaviour<ImpNetWeather, Terminal>();
        netcodeValidator.BindToPreExistingObjectByBehaviour<ImpNetPlayer, Terminal>();
        netcodeValidator.BindToPreExistingObjectByBehaviour<ImpNetCommunication, Terminal>();

        harmony = new Harmony(PLUGIN_GUID);

        PreLaunchPatch();

        IsImperiumReady = true;

        ImpOutput.Log("[OK] Imperium is ready!");
    }

    internal static void Launch()
    {
        if (!IsImperiumReady)
        {
            ImpOutput.Send("[ERR] Imperium failed to launch \u2299︿\u2299");
            return;
        }

        InputBindings = new ImpInputBindings();
        Terminal = GameObject.Find("TerminalScript").GetComponent<Terminal>();
        HUDManager = FindObjectOfType<HUDManager>();

        IsSceneLoaded = new ImpBinaryBinding(false);

        ImpNetworkManager.IsHost.Set(NetworkManager.Singleton.IsHost);

        PlayerManager.UpdateCameras();
        Freecam = ImpFreecam.Create();
        Interface = ImpInterfaceManager.Create();
        PositionIndicator = Instantiate(ImpAssets.IndicatorObject).AddComponent<PositionIndicator>();

        PlayerManager = new PlayerManager(IsSceneLoaded, ImpNetworkManager.ConnectedPlayers, Freecam);
        GameManager = new GameManager(IsSceneLoaded, ImpNetworkManager.ConnectedPlayers);
        ObjectManager = new ObjectManager(IsSceneLoaded, ImpNetworkManager.ConnectedPlayers);
        Oracle = new Oracle.Oracle();
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
            if (openInterface) PositionIndicator.HideIndicator();
        };

        ObjectManager.CurrentPlayers.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelEntities.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelItems.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelLandmines.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelTurrets.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelSpiderWebs.onTrigger += Visualization.RefreshOverlays;
        ObjectManager.CurrentLevelBreakerBoxes.onTrigger += Visualization.RefreshOverlays;

        ImpSettings.LoadAll();

        SpawnUI();

        // Patch the rest of the functionality at the end to make sure all the dependencies of the static patch
        // functions are loaded
        harmony.PatchAll();
        UnityExplorerIntegration.PatchFunctions(harmony);

        if (!ImpNetworkManager.IsHost.Value) ImpNetTime.Instance.RequestTimeServerRpc();
    }

    internal static void Unload()
    {
        harmony.UnpatchSelf();
        PreLaunchPatch();

        InputBindings.BaseMap.Disable();
        InputBindings.FreecamMap.Disable();
        Interface.StopListening();

        ImpSettings.Reinstantiate();
    }

    internal static void ReloadUI()
    {
        Interface.Close();
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
        Interface.Register<ImperiumUI>(ImpAssets.ImperiumUIObject, "<Keyboard>/F1");
        Interface.Register<SpawningUI>(ImpAssets.SpawningUIObject, "<Keyboard>/F2");
        Interface.Register<TeleportUI>(ImpAssets.TeleportUIObject, "<Keyboard>/F3");
        Interface.Register<WeatherUI>(ImpAssets.WeatherUIObject, "<Keyboard>/F4");
        Interface.Register<OracleUI>(ImpAssets.OracleUIObject, "<Keyboard>/F5");
        Interface.Register<NavigatorUI>(ImpAssets.NavigatorUIObject, "<Keyboard>/F6");

        Interface.StartListening();

        ImpOutput.Log("[OK] Imperium UIs have been registered! \\o/");
    }

    private static void PreLaunchPatch()
    {
        harmony.PatchAll(typeof(PlayerControllerPatch.PreloadPatches));
        harmony.PatchAll(typeof(TerminalPatch.PreloadPatches));
        harmony.PatchAll(typeof(PreInitPatches.PreInitSceneScriptPatch));
        harmony.PatchAll(typeof(PreInitPatches.MenuManagerPatch));
    }
}
#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

#endregion

namespace Imperium.Util;

public abstract class ImpAssets
{
    internal static GameObject ImperiumUIObject;
    internal static GameObject TeleportUIObject;
    internal static GameObject WeatherUIObject;
    internal static GameObject SpawningUIObject;
    internal static GameObject MoonUIObject;
    internal static GameObject SaveUIObject;
    internal static GameObject ObjectsUIObject;
    internal static GameObject SettingsUIObject;
    internal static GameObject ConfirmationUIObject;
    internal static GameObject RenderingUIObject;
    internal static GameObject OracleUIObject;
    internal static GameObject NavigatorUIObject;
    internal static GameObject VisualizerUIObject;
    internal static GameObject IndicatorObject;
    internal static GameObject MapUIObject;
    internal static GameObject MinimapSettingsObject;
    internal static GameObject LayerSelector;
    internal static GameObject MinimapOverlayObject;
    internal static GameObject NoiseOverlay;
    internal static GameObject NetworkHandler;
    internal static AudioClip GrassClick;
    internal static AudioClip ButtonClick;

    internal static GameObject SpawnTimerObject;
    internal static GameObject SpikeTrapTimerObject;
    internal static GameObject SpawnIndicator;
    internal static GameObject PlayerInfo;
    internal static GameObject EntityInfo;

    // Imperium visualizer materials
    public static Material XrayMaterial;
    public static Material FresnelBlueMaterial;
    public static Material FresnelYellowMaterial;
    public static Material FresnelGreenMaterial;
    public static Material FresnelRedMaterial;
    public static Material WireframePurpleMaterial;
    public static Material WireframeCyanMaterial;
    public static Material WireframeAmaranthMaterial;
    public static Material WireframeYellowMaterial;
    public static Material WireframeGreenMaterial;
    public static Material WireframeRedMaterial;

    internal static bool Load()
    {
        var assetFile = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "imperium_assets"
        );
        var assets = AssetBundle.LoadFromFile(assetFile);
        if (assets == null)
        {
            Imperium.Log.LogInfo($"[PRELOAD] Failed to load assets from {assetFile}, aborting!");
            return false;
        }

        logBuffer = [];
        List<bool> loadResults =
        [
            LoadFile(assets, "Assets/Prefabs/imperium_ui.prefab", out ImperiumUIObject),
            LoadFile(assets, "Assets/Prefabs/teleport_ui.prefab", out TeleportUIObject),
            LoadFile(assets, "Assets/Prefabs/weather_ui.prefab", out WeatherUIObject),
            LoadFile(assets, "Assets/Prefabs/spawning_ui.prefab", out SpawningUIObject),
            LoadFile(assets, "Assets/Prefabs/moon_ui.prefab", out MoonUIObject),
            LoadFile(assets, "Assets/Prefabs/save_ui.prefab", out SaveUIObject),
            LoadFile(assets, "Assets/Prefabs/objects_ui.prefab", out ObjectsUIObject),
            LoadFile(assets, "Assets/Prefabs/settings_ui.prefab", out SettingsUIObject),
            LoadFile(assets, "Assets/Prefabs/rendering_ui.prefab", out RenderingUIObject),
            LoadFile(assets, "Assets/Prefabs/oracle_ui.prefab", out OracleUIObject),
            LoadFile(assets, "Assets/Prefabs/navigator_ui.prefab", out NavigatorUIObject),
            LoadFile(assets, "Assets/Prefabs/visualizer_ui.prefab", out VisualizerUIObject),
            LoadFile(assets, "Assets/Prefabs/confirmation_ui.prefab", out ConfirmationUIObject),
            LoadFile(assets, "Assets/Prefabs/indicator.prefab", out IndicatorObject),
            LoadFile(assets, "Assets/Prefabs/map_ui.prefab", out MapUIObject),
            LoadFile(assets, "Assets/Prefabs/minimap.prefab", out MinimapOverlayObject),
            LoadFile(assets, "Assets/Prefabs/minimap_settings.prefab", out MinimapSettingsObject),
            LoadFile(assets, "Assets/Prefabs/layer_selector.prefab", out LayerSelector),
            LoadFile(assets, "Assets/Prefabs/spawn_timer.prefab", out SpawnTimerObject),
            LoadFile(assets, "Assets/Prefabs/spiketrap_timer.prefab", out SpikeTrapTimerObject),
            LoadFile(assets, "Assets/Prefabs/player_info.prefab", out PlayerInfo),
            LoadFile(assets, "Assets/Prefabs/entity_info.prefab", out EntityInfo),
            LoadFile(assets, "Assets/Prefabs/spawn_indicator.prefab", out SpawnIndicator),
            LoadFile(assets, "Assets/Prefabs/noise_overlay.prefab", out NoiseOverlay),
            LoadFile(assets, "Assets/Prefabs/network_handler.prefab", out NetworkHandler),
            LoadFile(assets, "Assets/Materials/xray.mat", out XrayMaterial),
            LoadFile(assets, "Assets/Materials/fresnel_blue.mat", out FresnelBlueMaterial),
            LoadFile(assets, "Assets/Materials/fresnel_red.mat", out FresnelRedMaterial),
            LoadFile(assets, "Assets/Materials/fresnel_green.mat", out FresnelGreenMaterial),
            LoadFile(assets, "Assets/Materials/fresnel_yellow.mat", out FresnelYellowMaterial),
            LoadFile(assets, "Assets/Materials/wireframe_purple.mat", out WireframePurpleMaterial),
            LoadFile(assets, "Assets/Materials/wireframe_cyan.mat", out WireframeCyanMaterial),
            LoadFile(assets, "Assets/Materials/wireframe_amaranth.mat", out WireframeAmaranthMaterial),
            LoadFile(assets, "Assets/Materials/wireframe_yellow.mat", out WireframeYellowMaterial),
            LoadFile(assets, "Assets/Materials/wireframe_green.mat", out WireframeGreenMaterial),
            LoadFile(assets, "Assets/Materials/wireframe_red.mat", out WireframeRedMaterial),
            LoadFile(assets, "Assets/Audio/GrassClick.wav", out GrassClick),
            LoadFile(assets, "Assets/Audio/ButtonClick.ogg", out ButtonClick)
        ];


        if (loadResults.Any(result => result == false))
        {
            Imperium.Log.LogInfo($"[PRELOAD] Failed to load one or more assets from {assetFile}, aborting!");
            return false;
        }

        ImpOutput.LogBlock(logBuffer, "Imperium Asset Loader");

        return true;
    }

    private static List<string> logBuffer = [];

    private static bool LoadFile<T>(AssetBundle assets, string path, out T loadedObject) where T : Object
    {
        loadedObject = assets.LoadAsset<T>(path);
        if (!loadedObject)
        {
            Imperium.Log.LogError($"Failed to load '{path}' from ./imperium_assets");
            return false;
        }

        logBuffer.Add($"> Successfully loaded {path.Split("/").Last()} from asset bundle.");

        return true;
    }
}
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
    /*
     * UI Prefabs
     */
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
    internal static GameObject MapUIObject;
    internal static GameObject MinimapSettingsObject;
    internal static GameObject LayerSelectorObject;
    internal static GameObject MinimapOverlayObject;
    
    /*
     * Other Prefabs
     */
    internal static GameObject IndicatorObject;
    internal static GameObject NoiseOverlay;
    internal static GameObject NetworkHandler;
    internal static GameObject SpawnTimerObject;
    internal static GameObject SpikeTrapTimerObject;
    internal static GameObject SpawnIndicator;
    internal static GameObject ObjectInsightPanel;

    /*
     * Audio Clips
     */
    internal static AudioClip GrassClick;
    internal static AudioClip ButtonClick;
    
    /*
     * Materials
     */
    public static Material XrayMaterial;
    public static Material FresnelWhiteMaterial;
    public static Material FresnelBlueMaterial;
    public static Material FresnelYellowMaterial;
    public static Material FresnelGreenMaterial;
    public static Material FresnelRedMaterial;
    public static Material WireframeNavMeshMaterial;
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
            LoadAsset(assets, "Assets/Prefabs/imperium_ui.prefab", out ImperiumUIObject),
            LoadAsset(assets, "Assets/Prefabs/teleport_ui.prefab", out TeleportUIObject),
            LoadAsset(assets, "Assets/Prefabs/weather_ui.prefab", out WeatherUIObject),
            LoadAsset(assets, "Assets/Prefabs/spawning_ui.prefab", out SpawningUIObject),
            LoadAsset(assets, "Assets/Prefabs/moon_ui.prefab", out MoonUIObject),
            LoadAsset(assets, "Assets/Prefabs/save_ui.prefab", out SaveUIObject),
            LoadAsset(assets, "Assets/Prefabs/objects_ui.prefab", out ObjectsUIObject),
            LoadAsset(assets, "Assets/Prefabs/settings_ui.prefab", out SettingsUIObject),
            LoadAsset(assets, "Assets/Prefabs/rendering_ui.prefab", out RenderingUIObject),
            LoadAsset(assets, "Assets/Prefabs/oracle_ui.prefab", out OracleUIObject),
            LoadAsset(assets, "Assets/Prefabs/navigator_ui.prefab", out NavigatorUIObject),
            LoadAsset(assets, "Assets/Prefabs/visualizer_ui.prefab", out VisualizerUIObject),
            LoadAsset(assets, "Assets/Prefabs/confirmation_ui.prefab", out ConfirmationUIObject),
            LoadAsset(assets, "Assets/Prefabs/indicator.prefab", out IndicatorObject),
            LoadAsset(assets, "Assets/Prefabs/map_ui.prefab", out MapUIObject),
            LoadAsset(assets, "Assets/Prefabs/minimap.prefab", out MinimapOverlayObject),
            LoadAsset(assets, "Assets/Prefabs/minimap_settings.prefab", out MinimapSettingsObject),
            LoadAsset(assets, "Assets/Prefabs/layer_selector.prefab", out LayerSelectorObject),
            LoadAsset(assets, "Assets/Prefabs/spawn_timer.prefab", out SpawnTimerObject),
            LoadAsset(assets, "Assets/Prefabs/spiketrap_timer.prefab", out SpikeTrapTimerObject),
            LoadAsset(assets, "Assets/Prefabs/insight_panel.prefab", out ObjectInsightPanel),
            LoadAsset(assets, "Assets/Prefabs/spawn_indicator.prefab", out SpawnIndicator),
            LoadAsset(assets, "Assets/Prefabs/noise_overlay.prefab", out NoiseOverlay),
            LoadAsset(assets, "Assets/Prefabs/network_handler.prefab", out NetworkHandler),
            LoadAsset(assets, "Assets/Materials/xray.mat", out XrayMaterial),
            LoadAsset(assets, "Assets/Materials/fresnel_white.mat", out FresnelWhiteMaterial),
            LoadAsset(assets, "Assets/Materials/fresnel_blue.mat", out FresnelBlueMaterial),
            LoadAsset(assets, "Assets/Materials/fresnel_red.mat", out FresnelRedMaterial),
            LoadAsset(assets, "Assets/Materials/fresnel_green.mat", out FresnelGreenMaterial),
            LoadAsset(assets, "Assets/Materials/fresnel_yellow.mat", out FresnelYellowMaterial),
            LoadAsset(assets, "Assets/Materials/wireframe_navmesh.mat", out WireframeNavMeshMaterial),
            LoadAsset(assets, "Assets/Materials/wireframe_purple.mat", out WireframePurpleMaterial),
            LoadAsset(assets, "Assets/Materials/wireframe_cyan.mat", out WireframeCyanMaterial),
            LoadAsset(assets, "Assets/Materials/wireframe_amaranth.mat", out WireframeAmaranthMaterial),
            LoadAsset(assets, "Assets/Materials/wireframe_yellow.mat", out WireframeYellowMaterial),
            LoadAsset(assets, "Assets/Materials/wireframe_green.mat", out WireframeGreenMaterial),
            LoadAsset(assets, "Assets/Materials/wireframe_red.mat", out WireframeRedMaterial),
            LoadAsset(assets, "Assets/Audio/GrassClick.wav", out GrassClick),
            LoadAsset(assets, "Assets/Audio/ButtonClick.ogg", out ButtonClick)
        ];


        if (loadResults.Any(result => result == false))
        {
            Imperium.Log.LogInfo($"[PRELOAD] Failed to load one or more assets from {assetFile}, aborting!");
            return false;
        }

        ImpOutput.LogBlock(logBuffer, "Imperium Resource Loader");

        return true;
    }

    private static List<string> logBuffer = [];

    private static bool LoadAsset<T>(AssetBundle assets, string path, out T loadedObject) where T : Object
    {
        loadedObject = assets.LoadAsset<T>(path);
        if (!loadedObject)
        {
            Imperium.Log.LogError($"[PRELOAD] Failed to load '{path}' from ./imperium_assets");
            return false;
        }

        logBuffer.Add($"> Successfully loaded {path.Split("/").Last()} from asset bundle.");

        return true;
    }
}
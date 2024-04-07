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
    internal static GameObject IndicatorObject;
    internal static GameObject LayerSelector;
    internal static AudioClip GrassClick;
    internal static AudioClip ButtonClick;

    internal static GameObject SpawnTimerObject;
    internal static GameObject SpikeTrapTimerObject;
    internal static GameObject SpawnIndicator;
    internal static GameObject PlayerInfo;
    internal static GameObject EntityInfo;
    internal static Material XrayMaterial;
    internal static Material FresnelBlueMaterial;
    internal static Material FresnelYellowMaterial;
    internal static Material FresnelGreenMaterial;
    internal static Material FresnelRedMaterial;
    internal static Material WireframePurpleMaterial;
    internal static Material WireframeCyanMaterial;
    internal static Material WireframeAmaranthMaterial;
    internal static Material WireframeYellowMaterial;
    internal static Material WireframeGreenMaterial;
    internal static Material WireframeRedMaterial;

    internal static bool Load()
    {
        var assets = AssetBundle.LoadFromFile(
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "imperium_assets"));
        if (assets == null)
        {
            Imperium.Output.Log("[PRELOAD] Failed to load Imperium assets, aborting!");
            return false;
        }

        logBuffer = [];

        List<bool> loadResults =
        [
            LoadFile(assets, "Assets/imperium_ui.prefab", out ImperiumUIObject),
            LoadFile(assets, "Assets/teleport_ui.prefab", out TeleportUIObject),
            LoadFile(assets, "Assets/weather_ui.prefab", out WeatherUIObject),
            LoadFile(assets, "Assets/spawning_ui.prefab", out SpawningUIObject),
            LoadFile(assets, "Assets/moon_ui.prefab", out MoonUIObject),
            LoadFile(assets, "Assets/save_ui.prefab", out SaveUIObject),
            LoadFile(assets, "Assets/objects_ui.prefab", out ObjectsUIObject),
            LoadFile(assets, "Assets/settings_ui.prefab", out SettingsUIObject),
            LoadFile(assets, "Assets/rendering_ui.prefab", out RenderingUIObject),
            LoadFile(assets, "Assets/oracle_ui.prefab", out OracleUIObject),
            LoadFile(assets, "Assets/navigator_ui.prefab", out NavigatorUIObject),
            LoadFile(assets, "Assets/confirmation_ui.prefab", out ConfirmationUIObject),
            LoadFile(assets, "Assets/indicator.prefab", out IndicatorObject),
            LoadFile(assets, "Assets/layer_selector.prefab", out LayerSelector),
            LoadFile(assets, "Assets/spawn_timer.prefab", out SpawnTimerObject),
            LoadFile(assets, "Assets/spiketrap_timer.prefab", out SpikeTrapTimerObject),
            LoadFile(assets, "Assets/player_info.prefab", out PlayerInfo),
            LoadFile(assets, "Assets/entity_info.prefab", out EntityInfo),
            LoadFile(assets, "Assets/spawn_indicator.prefab", out SpawnIndicator),
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
            LoadFile(assets, "Assets/Audio/ButtonClick.ogg", out ButtonClick),
        ];

        Imperium.Output.LogBlock(logBuffer, "Imperium Asset Loader");

        if (loadResults.Any(result => result == false))
        {
            Imperium.Output.Log("[PRELOAD]  Failed to load one or more assets from ./imperium_assets, aborting!");
            return false;
        }

        return true;
    }

    private static List<string> logBuffer = [];

    private static bool LoadFile<T>(AssetBundle assets, string path, out T loadedObject) where T : Object
    {
        loadedObject = assets.LoadAsset<T>(path);
        if (!loadedObject)
        {
            Imperium.Output.Error($"Failed to load '{path}' from ./imperium_assets");
            return false;
        }

        logBuffer.Add($"> Successfully loaded {path.Split("/").Last()} from asset bundle.");

        return true;
    }
}
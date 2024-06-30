#region

using Imperium.Interface.Common;
using Imperium.Util;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Rendering;

internal class RenderingWindow : ImperiumWindow
{
    protected override void InitWindow()
    {
        var content = transform.Find("Content");

        ImpSlider.Bind(
            path: "Resolution",
            container: content,
            valueBinding: Imperium.Settings.Rendering.ResolutionMultiplier,
            indicatorFormatter: value => $"{Formatting.FormatFloatToThreeDigits(value)}",
            debounceTime: 0.2f,
            theme: theme
        );

        ImpToggle.Bind("Lighting/SunlightToggle", content, Imperium.Settings.Rendering.Sunlight, theme);
        ImpToggle.Bind("Lighting/SpaceSunToggle", content, Imperium.Settings.Rendering.SpaceSun, theme);
        ImpToggle.Bind("Lighting/IndirectLightToggle", content, Imperium.Settings.Rendering.IndirectLighting, theme);
        ImpToggle.Bind("Volumetrics/VolumetricFogToggle", content, Imperium.Settings.Rendering.VolumetricFog, theme);
        ImpToggle.Bind("Volumetrics/GroundFogToggle", content, Imperium.Settings.Rendering.GroundFog, theme);
        ImpToggle.Bind("Volumetrics/StormyVolumeToggle", content, Imperium.Settings.Rendering.StormyVolume, theme);
        ImpToggle.Bind("Volumetrics/SkyboxVolumeToggle", content, Imperium.Settings.Rendering.SkyboxVolume, theme);
        ImpToggle.Bind("Volumetrics/GlobalVolumeToggle", content, Imperium.Settings.Rendering.GlobalVolume, theme);
        ImpToggle.Bind("FrameSettings/DecalLayers", content, Imperium.Settings.Rendering.DecalLayers, theme);
        ImpToggle.Bind("FrameSettings/SSGI", content, Imperium.Settings.Rendering.SSGI, theme);
        ImpToggle.Bind("FrameSettings/RayTracing", content, Imperium.Settings.Rendering.RayTracing, theme);
        ImpToggle.Bind("FrameSettings/VolumetricClouds", content, Imperium.Settings.Rendering.VolumetricClouds, theme);
        ImpToggle.Bind("FrameSettings/SSS", content, Imperium.Settings.Rendering.SSS, theme);
        ImpToggle.Bind(
            "FrameSettings/VolumeReprojection",
            content,
            Imperium.Settings.Rendering.VolumeReprojection,
            theme
        );
        ImpToggle.Bind(
            "FrameSettings/TransparentPrepass",
            content,
            Imperium.Settings.Rendering.TransparentPrepass,
            theme
        );
        ImpToggle.Bind(
            "FrameSettings/TransparentPostpass",
            content,
            Imperium.Settings.Rendering.TransparentPostpass,
            theme
        );
        ImpToggle.Bind("PostProcessing/CELToggle", content, Imperium.Settings.Rendering.CelShading, theme);
        ImpToggle.Bind("PlayerOverlays/StarsToggle", content, Imperium.Settings.Rendering.StarsOverlay, theme);
        ImpToggle.Bind("PlayerOverlays/HUDVisorToggle", content, Imperium.Settings.Rendering.HUDVisor, theme);
        ImpToggle.Bind("PlayerOverlays/HUDToggle", content, Imperium.Settings.Rendering.PlayerHUD, theme);
        ImpToggle.Bind("PlayerFilters/FearFilterToggle", content, Imperium.Settings.Rendering.FearFilter, theme);
        ImpToggle.Bind(
            "PlayerFilters/FlashbangFilterToggle",
            content,
            Imperium.Settings.Rendering.FlashbangFilter,
            theme
        );
        ImpToggle.Bind(
            "PlayerFilters/UnderwaterFilterToggle",
            content,
            Imperium.Settings.Rendering.UnderwaterFilter,
            theme
        );
        ImpToggle.Bind(
            "PlayerFilters/DrunknessFilterToggle",
            content,
            Imperium.Settings.Rendering.DrunknessFilter,
            theme
        );
        ImpToggle.Bind("PlayerFilters/ScanSphereToggle", content, Imperium.Settings.Rendering.ScanSphere, theme);

        // Refresh space sun whenever the ship takes off
        Imperium.IsSceneLoaded.onTrigger += Imperium.Settings.Rendering.SpaceSun.Refresh;
    }
}
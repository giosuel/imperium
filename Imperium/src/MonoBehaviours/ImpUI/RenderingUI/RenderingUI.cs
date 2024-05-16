#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.RenderingUI;

internal class RenderingUI : SingleplexUI
{
    protected override void InitUI()
    {
        ImpSlider.Bind(
            path: "Resolution",
            container: content,
            valueBinding: ImpSettings.Rendering.ResolutionMultiplier,
            indicatorFormatter: value => $"{ImpUtils.Math.FormatFloatToThreeDigits(value)}",
            debounceTime: 0.2f,
            theme: theme
        );

        ImpToggle.Bind("Lighting/SunlightToggle", content, ImpSettings.Rendering.Sunlight, theme);
        ImpToggle.Bind("Lighting/SpaceSunToggle", content, ImpSettings.Rendering.SpaceSun, theme);
        ImpToggle.Bind("Lighting/IndirectLightToggle", content, ImpSettings.Rendering.IndirectLighting, theme);
        ImpToggle.Bind("Volumetrics/VolumetricFogToggle", content, ImpSettings.Rendering.VolumetricFog, theme);
        ImpToggle.Bind("Volumetrics/GroundFogToggle", content, ImpSettings.Rendering.GroundFog, theme);
        ImpToggle.Bind("Volumetrics/StormyVolumeToggle", content, ImpSettings.Rendering.StormyVolume, theme);
        ImpToggle.Bind("Volumetrics/SkyboxVolumeToggle", content, ImpSettings.Rendering.SkyboxVolume, theme);
        ImpToggle.Bind("Volumetrics/GlobalVolumeToggle", content, ImpSettings.Rendering.GlobalVolume, theme);
        ImpToggle.Bind("Volumetrics/SteamleakToggle", content, ImpSettings.Rendering.Steamleaks, theme);
        ImpToggle.Bind("FrameSettings/DecalLayers", content, ImpSettings.Rendering.DecalLayers, theme);
        ImpToggle.Bind("FrameSettings/SSGI", content, ImpSettings.Rendering.SSGI, theme);
        ImpToggle.Bind("FrameSettings/RayTracing", content, ImpSettings.Rendering.RayTracing, theme);
        ImpToggle.Bind("FrameSettings/VolumetricClouds", content, ImpSettings.Rendering.VolumetricClouds, theme);
        ImpToggle.Bind("FrameSettings/SSS", content, ImpSettings.Rendering.SSS, theme);
        ImpToggle.Bind("FrameSettings/VolumeReprojection", content, ImpSettings.Rendering.VolumeReprojection, theme);
        ImpToggle.Bind("FrameSettings/TransparentPrepass", content, ImpSettings.Rendering.TransparentPrepass, theme);
        ImpToggle.Bind("FrameSettings/TransparentPostpass", content, ImpSettings.Rendering.TransparentPostpass, theme);
        ImpToggle.Bind("PostProcessing/CELToggle", content, ImpSettings.Rendering.CelShading, theme);
        ImpToggle.Bind("PlayerOverlays/StarsToggle", content, ImpSettings.Rendering.StarsOverlay, theme);
        ImpToggle.Bind("PlayerOverlays/HUDVisorToggle", content, ImpSettings.Rendering.HUDVisor, theme);
        ImpToggle.Bind("PlayerOverlays/HUDToggle", content, ImpSettings.Rendering.PlayerHUD, theme);
        ImpToggle.Bind("PlayerFilters/FearFilterToggle", content, ImpSettings.Rendering.FearFilter, theme);
        ImpToggle.Bind("PlayerFilters/FlashbangFilterToggle", content, ImpSettings.Rendering.FlashbangFilter, theme);
        ImpToggle.Bind("PlayerFilters/UnderwaterFilterToggle", content, ImpSettings.Rendering.UnderwaterFilter, theme);
        ImpToggle.Bind("PlayerFilters/DrunknessFilterToggle", content, ImpSettings.Rendering.DrunknessFilter, theme);
        ImpToggle.Bind("PlayerFilters/ScanSphereToggle", content, ImpSettings.Rendering.ScanSphere, theme);
    }
}
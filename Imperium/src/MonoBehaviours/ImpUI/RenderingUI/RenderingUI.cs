#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Util;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.RenderingUI;

internal class RenderingUI : StandaloneUI
{
    public override void Awake() => InitializeUI();

    protected override void InitUI()
    {
        ImpSlider.Bind(
            path: "Resolution",
            container: content,
            valueBinding: ImpSettings.Rendering.ResolutionMultiplier,
            indicatorFormatter: value => $"{ImpUtils.Math.FormatFloatToThreeDigits(value)}",
            debounceTime: 0.2f
        );

        ImpToggle.Bind("Lighting/SunlightToggle", content, ImpSettings.Rendering.Sunlight);
        ImpToggle.Bind("Lighting/SpaceSunToggle", content, ImpSettings.Rendering.SpaceSun);
        ImpToggle.Bind("Lighting/IndirectLightToggle", content, ImpSettings.Rendering.IndirectLighting);
        ImpToggle.Bind("Volumetrics/VolumetricFogToggle", content, ImpSettings.Rendering.VolumetricFog);
        ImpToggle.Bind("Volumetrics/GroundFogToggle", content, ImpSettings.Rendering.GroundFog);
        ImpToggle.Bind("Volumetrics/StormyVolumeToggle", content, ImpSettings.Rendering.StormyVolume);
        ImpToggle.Bind("Volumetrics/SkyboxVolumeToggle", content, ImpSettings.Rendering.SkyboxVolume);
        ImpToggle.Bind("Volumetrics/GlobalVolumeToggle", content, ImpSettings.Rendering.GlobalVolume);
        ImpToggle.Bind("Volumetrics/SteamleakToggle", content, ImpSettings.Rendering.Steamleaks);
        ImpToggle.Bind("FrameSettings/DecalLayers", content, ImpSettings.Rendering.DecalLayers);
        ImpToggle.Bind("FrameSettings/SSGI", content, ImpSettings.Rendering.SSGI);
        ImpToggle.Bind("FrameSettings/RayTracing", content, ImpSettings.Rendering.RayTracing);
        ImpToggle.Bind("FrameSettings/VolumetricClouds", content, ImpSettings.Rendering.VolumetricClouds);
        ImpToggle.Bind("FrameSettings/SubsurfaceScattering", content, ImpSettings.Rendering.SSGI);
        ImpToggle.Bind("FrameSettings/VolumeReprojection", content, ImpSettings.Rendering.VolumeReprojection);
        ImpToggle.Bind("FrameSettings/TransparentPrepass", content, ImpSettings.Rendering.TransparentPrepass);
        ImpToggle.Bind("FrameSettings/TransparentPostpass", content, ImpSettings.Rendering.TransparentPostpass);
        ImpToggle.Bind("PostProcessing/CELToggle", content, ImpSettings.Rendering.CelShading);
        ImpToggle.Bind("PlayerOverlays/StarsToggle", content, ImpSettings.Rendering.StarsOverlay);
        ImpToggle.Bind("PlayerOverlays/HUDVisorToggle", content, ImpSettings.Rendering.HUDVisor);
        ImpToggle.Bind("PlayerOverlays/HUDToggle", content, ImpSettings.Rendering.PlayerHUD);
        ImpToggle.Bind("PlayerFilters/FearFilterToggle", content, ImpSettings.Rendering.FearFilter);
        ImpToggle.Bind("PlayerFilters/FlashbangFilterToggle", content, ImpSettings.Rendering.FlashbangFilter);
        ImpToggle.Bind("PlayerFilters/UnderwaterFilterToggle", content, ImpSettings.Rendering.UnderwaterFilter);
        ImpToggle.Bind("PlayerFilters/DrunknessFilterToggle", content, ImpSettings.Rendering.DrunknessFilter);
        ImpToggle.Bind("PlayerFilters/ScanSphereToggle", content, ImpSettings.Rendering.ScanSphere);
    }
}
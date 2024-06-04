#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Util;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.VisualizationUI.Windows;

internal class VisualizerSettingsWindows : BaseWindow
{
    protected override void RegisterWindow()
    {
        ImpToggle.Bind(
            "General/SmoothAnimations",
            content,
            ImpSettings.Visualizations.SmoothAnimations,
            themeBinding
        );
        ImpToggle.Bind(
            "SSOverlays/AlwaysOnTop",
            content,
            ImpSettings.Visualizations.SSAlwaysOnTop,
            themeBinding
        );
        ImpToggle.Bind(
            "SSOverlays/AutoScale",
            content,
            ImpSettings.Visualizations.SSAutoScale,
            themeBinding
        );
        ImpToggle.Bind(
            "SSOverlays/HideInactive",
            content,
            ImpSettings.Visualizations.SSHideInactive,
            themeBinding
        );

        ImpSlider.Bind(
            path: "OverlayScale",
            container: content,
            valueBinding: ImpSettings.Visualizations.SSOverlayScale,
            indicatorFormatter: Formatting.FormatFloatToThreeDigits,
            theme: themeBinding
        );
    }
}
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
            Imperium.Settings.Visualization.SmoothAnimations,
            themeBinding
        );
        ImpToggle.Bind(
            "SSOverlays/AlwaysOnTop",
            content,
            Imperium.Settings.Visualization.SSAlwaysOnTop,
            themeBinding
        );
        ImpToggle.Bind(
            "SSOverlays/AutoScale",
            content,
            Imperium.Settings.Visualization.SSAutoScale,
            themeBinding
        );
        ImpToggle.Bind(
            "SSOverlays/HideInactive",
            content,
            Imperium.Settings.Visualization.SSHideInactive,
            themeBinding
        );

        ImpSlider.Bind(
            path: "OverlayScale",
            container: content,
            valueBinding: Imperium.Settings.Visualization.SSOverlayScale,
            indicatorFormatter: Formatting.FormatFloatToThreeDigits,
            theme: themeBinding
        );
    }
}
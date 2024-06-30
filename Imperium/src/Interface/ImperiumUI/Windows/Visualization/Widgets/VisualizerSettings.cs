#region

using Imperium.Interface.Common;
using Imperium.Util;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Visualization.Widgets;

internal class VisualizerSettings : ImpWidget
{
    protected override void InitWidget()
    {
        ImpToggle.Bind(
            "General/SmoothAnimations",
            transform,
            Imperium.Settings.Visualization.SmoothAnimations,
            theme
        );
        ImpToggle.Bind(
            "SSOverlays/AlwaysOnTop",
            transform,
            Imperium.Settings.Visualization.SSAlwaysOnTop,
            theme
        );
        ImpToggle.Bind(
            "SSOverlays/AutoScale",
            transform,
            Imperium.Settings.Visualization.SSAutoScale,
            theme
        );
        ImpToggle.Bind(
            "SSOverlays/HideInactive",
            transform,
            Imperium.Settings.Visualization.SSHideInactive,
            theme
        );

        ImpSlider.Bind(
            path: "OverlayScale",
            container: transform,
            valueBinding: Imperium.Settings.Visualization.SSOverlayScale,
            indicatorFormatter: Formatting.FormatFloatToThreeDigits,
            theme: theme
        );
    }
}
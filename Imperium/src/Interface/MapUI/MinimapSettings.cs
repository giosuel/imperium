#region

using Imperium.Interface.Common;
using Imperium.Types;

#endregion

namespace Imperium.Interface.MapUI;

internal class MinimapSettings : BaseUI
{
    protected override void InitUI()
    {
        ImpToggle.Bind("Content/Gizmos/ShowInfoPanel", transform, Imperium.Settings.Map.MinimapInfoPanel, theme);
        ImpToggle.Bind("Content/Gizmos/ShowLocationPanel", transform, Imperium.Settings.Map.MinimapLocationPanel, theme);

        ImpSlider.Bind(
            path: "Content/Scale",
            container: transform,
            valueBinding: Imperium.Settings.Map.MinimapScale,
            indicatorFormatter: value => $"{value:0.0}",
            theme: theme
        );
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Content/Width/Overlay", Variant.LIGHTER),
            new StyleOverride("Content/Height/Overlay", Variant.LIGHTER)
        );
    }
}
#region

using Imperium.Interface.Common;
using Imperium.Types;

#endregion

namespace Imperium.Interface.MapUI;

internal class MinimapSettings : BaseUI
{
    protected override void InitUI()
    {
        ImpToggle.Bind("Content/Gizmos/ShowInfoPanel", container, Imperium.Settings.Map.MinimapInfoPanel, theme);
        ImpToggle.Bind("Content/Gizmos/ShowLocationPanel", container, Imperium.Settings.Map.MinimapLocationPanel, theme);

        ImpSlider.Bind(
            path: "Content/Scale",
            container: container,
            valueBinding: Imperium.Settings.Map.MinimapScale,
            indicatorFormatter: value => $"{value:0.0}",
            theme: theme
        );

        ImpButton.Bind(
            "TitleBox/Close",
            container,
            onClick: Close,
            theme: theme,
            isIconButton: true
        );
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("TitleBox", Variant.DARKER),
            new StyleOverride("Border", Variant.DARKER),
            new StyleOverride("Content/Width/Overlay", Variant.LIGHTER),
            new StyleOverride("Content/Height/Overlay", Variant.LIGHTER)
        );

        ImpThemeManager.StyleText(
            themeUpdate,
            container,
            new StyleOverride("TitleBox/Title", Variant.FOREGROUND)
        );
    }
}
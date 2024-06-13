#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MinimapSettings;

internal class MinimapSettings : SingleplexUI
{
    protected override void InitUI()
    {
        ImpToggle.Bind("Gizmos/ShowInfoPanel", content, Imperium.Settings.Map.MinimapInfoPanel, theme);
        ImpToggle.Bind("Gizmos/ShowLocationPanel", content, Imperium.Settings.Map.MinimapLocationPanel, theme);

        ImpSlider.Bind(
            path: "Scale",
            container: content,
            valueBinding: Imperium.Settings.Map.MinimapScale,
            indicatorFormatter: value => $"{value:0.0}",
            theme: theme
        );
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            content,
            new StyleOverride("Width/Overlay", Variant.LIGHTER),
            new StyleOverride("Height/Overlay", Variant.LIGHTER)
        );
    }
}
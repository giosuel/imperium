using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util.Binding;

namespace Imperium.MonoBehaviours.ImpUI.MinimapSettings;

internal class MinimapSettings : SingleplexUI
{
    protected override void InitUI()
    {
        ImpToggle.Bind("Gizmos/ShowInfoPanel", content, ImpSettings.Map.MinimapInfoPanel, theme);

        ImpSlider.Bind("Width", content, ImpSettings.Map.MinimapWidth, theme);
        ImpSlider.Bind("Height", content, ImpSettings.Map.MinimapHeight, theme);
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
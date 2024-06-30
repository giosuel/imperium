#region

using Imperium.Interface.ImperiumUI.Windows.Visualization.Widgets;
using Imperium.Types;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Visualization;

internal class VisualizationWindow : ImperiumWindow
{
    protected override void InitWindow()
    {
        RegisterWidget<Widgets.Visualizers>(transform, "Visualizers");
        RegisterWidget<VisualizerSettings>(transform, "Settings");
        RegisterWidget<ObjectVisualizers>(transform, "Objects");
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdated)
    {
        ImpThemeManager.Style(
            themeUpdated,
            transform,
            new StyleOverride("Border", Variant.DARKER),
            new StyleOverride("Visualizers", Variant.DARKER),
            new StyleOverride("Settings/Border", Variant.DARKER)
        );
    }
}
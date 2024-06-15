#region

using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumDock;

public class ImperiumDock : BaseUI
{
    protected override void InitUI()
    {
        ImpButton.Bind("Top/Left/TeleportUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Top/Left/SpawningUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Top/Left/ShipUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Top/Center/ImperiumUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Top/Right/WeatherUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Top/Right/OracleUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Top/Right/MapUI", container, () =>
        {
        }, theme);

        ImpButton.Bind("Side/VisualizerUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Side/MoonUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Side/RenderUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Side/TimeUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Side/SaveUI", container, () =>
        {
        }, theme);
        ImpButton.Bind("Side/SettingsUI", container, () =>
        {
        }, theme);
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("Top/Left", Variant.BACKGROUND),
            new StyleOverride("Top/Center", Variant.BACKGROUND),
            new StyleOverride("Top/Side", Variant.BACKGROUND),
            new StyleOverride("Side", Variant.BACKGROUND),
            new StyleOverride("Top/Left/Border", Variant.DARKER),
            new StyleOverride("Top/Center/Border", Variant.DARKER),
            new StyleOverride("Top/Right/Border", Variant.DARKER),
            new StyleOverride("Side/Border", Variant.DARKER)
        );
    }
}
#region

using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.MoonControl.Widgets;

public class TimeManipulation : ImpWidget
{
    protected override void InitWidget()
    {
        ImpSlider.Bind(
            path: "TimeSpeed",
            container: transform,
            valueBinding: Imperium.MoonManager.TimeSpeed,
            indicatorFormatter: Formatting.FormatFloatToThreeDigits,
            useLogarithmicScale: true,
            debounceTime: 0.05f,
            theme: theme,
            interactableBindings: Imperium.MoonManager.TimeIsPaused,
            interactableInvert: true
        );

        ImpToggle.Bind(
            "TimeSettings/Pause",
            transform,
            Imperium.MoonManager.TimeIsPaused,
            theme
        );

        ImpToggle.Bind("TimeSettings/RealtimeClock", transform, Imperium.Settings.Time.RealtimeClock, theme);
        ImpToggle.Bind("TimeSettings/PermanentClock", transform, Imperium.Settings.Time.PermanentClock, theme);
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("TimeContainer/Time", Variant.DARKER)
        );
    }
}
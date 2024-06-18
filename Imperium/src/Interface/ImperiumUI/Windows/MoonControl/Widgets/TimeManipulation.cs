using Imperium.MonoBehaviours.ImpUI;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;

namespace Imperium.Interface.ImperiumUI.Windows.MoonControl.Widgets;

public class TimeManipulation : ImpWidget
{
    protected override void InitWidget()
    {
        var timeScaleInteractable = new ImpBinding<bool>(false);
        Imperium.MoonManager.TimeIsPaused.onUpdate += isPaused =>
        {
            timeScaleInteractable.Set(!isPaused && Imperium.IsSceneLoaded.Value);
        };
        Imperium.IsSceneLoaded.onUpdate += isSceneLoaded =>
        {
            timeScaleInteractable.Set(isSceneLoaded && !Imperium.MoonManager.TimeIsPaused.Value);
        };

        ImpSlider.Bind(
            path: "TimeSpeed",
            container: transform,
            valueBinding: Imperium.MoonManager.TimeSpeed,
            indicatorFormatter: Formatting.FormatFloatToThreeDigits,
            useLogarithmicScale: true,
            debounceTime: 0.05f,
            theme: theme,
            interactableBindings: timeScaleInteractable
        );
        ImpToggle.Bind(
            "TimeSettings/Pause",
            transform,
            Imperium.MoonManager.TimeIsPaused,
            theme,
            interactableBindings: Imperium.IsSceneLoaded
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
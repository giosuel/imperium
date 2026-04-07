#region

using Imperium.Core.Lifecycle;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.MoonControl.Widgets;
using Imperium.Types;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.MoonControl;

internal class MoonControlWindow : ImperiumWindow
{
    protected override void InitWindow()
    {
        InitSpawnPropertyFields();
        InitMapObstacleButtons();
        InitEntitySpawning();

        RegisterWidget<WeatherForecaster>(transform, "Right/Weather");
        RegisterWidget<TimeManipulation>(transform, "Right/Time");
        RegisterWidget<LevelGeneration>(transform, "Left/Generation");
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Border", Variant.DARKER),
            new StyleOverride("Right", Variant.DARKER)
        );
    }

    private void InitEntitySpawning()
    {
        ImpToggle.Bind(
            "Left/PauseSpawning/Indoor", transform,
            Imperium.MoonManager.IndoorSpawningPaused,
            theme
        );

        ImpToggle.Bind(
            "Left/PauseSpawning/Outdoor", transform,
            Imperium.MoonManager.OutdoorSpawningPaused,
            theme
        );

        ImpToggle.Bind(
            "Left/PauseSpawning/Daytime", transform,
            Imperium.MoonManager.DaytimeSpawningPaused,
            theme
        );
    }

    private void InitSpawnPropertyFields()
    {
        ImpInput.Bind(
            "Left/EntitySpawning/MinIndoorSpawns/Input",
            transform,
            Imperium.MoonManager.MinIndoorSpawns,
            theme
        );
        ImpInput.Bind(
            "Left/EntitySpawning/MinOutdoorSpawns/Input",
            transform,
            Imperium.MoonManager.MinOutdoorSpawns,
            theme
        );
        ImpInput.Bind(
            "Left/EntitySpawning/MaxIndoorPower/Input",
            transform,
            Imperium.MoonManager.MaxIndoorPower,
            theme
        );
        ImpInput.Bind(
            "Left/EntitySpawning/MaxOutdoorPower/Input",
            transform,
            Imperium.MoonManager.MaxOutdoorPower,
            theme
        );
        ImpInput.Bind(
            "Left/EntitySpawning/MaxDaytimePower/Input",
            transform,
            Imperium.MoonManager.MaxDaytimePower,
            theme
        );
        ImpInput.Bind(
            "Left/EntitySpawning/IndoorDeviation/Input",
            transform,
            Imperium.MoonManager.IndoorDeviation,
            theme
        );
        ImpInput.CreateStatic(
            "Left/EntitySpawning/OutdoorDeviation/Input",
            transform,
            "3",
            theme
        );
        ImpInput.Bind(
            "Left/EntitySpawning/DaytimeDeviation/Input",
            transform,
            Imperium.MoonManager.DaytimeDeviation,
            theme
        );

        ImpInput.Bind("Left/WeatherVariables/Variable1/Input", transform, Imperium.MoonManager.WeatherVariable1, theme);
        ImpInput.Bind("Left/WeatherVariables/Variable2/Input", transform, Imperium.MoonManager.WeatherVariable2, theme);
    }

    private void InitMapObstacleButtons()
    {
        ImpButton.Bind(
            "Left/MapObstacles/Doors/Open",
            transform,
            () => MoonManager.ToggleDoors(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Doors/Close",
            transform,
            () => MoonManager.ToggleDoors(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Doors/Lock",
            transform,
            () => Imperium.MoonManager.ToggleDoorLocks(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Doors/Unlock",
            transform,
            () => Imperium.MoonManager.ToggleDoorLocks(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Breakers/On",
            transform,
            () => MoonManager.ToggleBreakers(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Breakers/Off",
            transform,
            () => MoonManager.ToggleBreakers(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Breakers/Flicker",
            transform,
            () => Imperium.MoonManager.FlickerLights(),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Security/Open",
            transform,
            () => MoonManager.ToggleSecurityDoors(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Security/Close",
            transform,
            () => MoonManager.ToggleSecurityDoors(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/IndoorFog/On",
            transform,
            () => Imperium.MoonManager.ToggleIndoorFog(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/IndoorFog/Off",
            transform,
            () => Imperium.MoonManager.ToggleIndoorFog(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Gunk/Clean",
            transform,
            () => Imperium.MoonManager.CleanFloor(),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );
    }

    protected override void OnOpen()
    {
        Imperium.GameManager.ProfitQuota.Refresh();
        Imperium.GameManager.GroupCredits.Refresh();
        Imperium.GameManager.QuotaDeadline.Refresh();
    }
}
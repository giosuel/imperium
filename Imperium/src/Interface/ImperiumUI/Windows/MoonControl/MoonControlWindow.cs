#region

using Imperium.Core.Lifecycle;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.MoonControl.Widgets;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.MoonControl;

internal class MoonControlWindow : ImperiumWindow
{
    protected override void InitWindow()
    {
        InitSpawnPropertyFields();
        InitMapObstacleButtons();

        RegisterWidget<WeatherForecaster>(transform, "Right/Weather");
        RegisterWidget<TimeManipulation>(transform, "Right/Time");
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
            "Left/MapObstacles/Doors/Left/OpenDoors",
            transform,
            () => MoonManager.ToggleDoors(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Doors/Left/CloseDoors",
            transform,
            () => MoonManager.ToggleDoors(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Doors/Right/LockDoors",
            transform,
            () => MoonManager.ToggleDoorLocks(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Doors/Right/UnlockDoors",
            transform,
            () => MoonManager.ToggleDoorLocks(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Electronics/Left/OpenSecurity",
            transform,
            () => MoonManager.ToggleSecurityDoors(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );
        ImpButton.Bind(
            "Left/MapObstacles/Electronics/Left/CloseSecurity",
            transform,
            () => MoonManager.ToggleSecurityDoors(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Electronics/Right/TurnOnBreakers",
            transform,
            () => MoonManager.ToggleBreakers(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/Electronics/Right/TurnOffBreakers",
            transform,
            () => MoonManager.ToggleBreakers(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/MapHazards/Left/EnableTurrets",
            transform,
            () => MoonManager.ToggleTurrets(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/MapHazards/Left/DisableTurrets",
            transform,
            () => MoonManager.ToggleTurrets(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/MapHazards/Right/EnableLandmines",
            transform,
            () => MoonManager.ToggleLandmines(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );

        ImpButton.Bind(
            "Left/MapObstacles/MapHazards/Right/DisableLandmines",
            transform,
            () => MoonManager.ToggleLandmines(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: theme
        );
    }
}
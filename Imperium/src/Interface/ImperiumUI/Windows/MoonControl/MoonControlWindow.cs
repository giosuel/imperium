#region

using Imperium.Core.Lifecycle;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.MoonControl.Widgets;
using Imperium.Types;
using Imperium.Util;
using TMPro;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.MoonControl;

internal class MoonControlWindow : ImperiumWindow
{
    private TMP_InputField levelSeedInput;
    private TMP_Text levelSeedTitle;
    private TMP_Text levelSeedText;

    protected override void InitWindow()
    {
        InitSpawnPropertyFields();
        InitMapObstacleButtons();
        InitEntitySpawning();
        InitGeneration();

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

    private void InitEntitySpawning()
    {
        ImpToggle.Bind(
            "Left/PauseIndoorSpawning", transform,
            Imperium.MoonManager.IndoorSpawningPaused,
            theme
        );

        ImpToggle.Bind(
            "Left/PauseOutdoorSpawning", transform,
            Imperium.MoonManager.OutdoorSpawningPaused,
            theme
        );

        ImpToggle.Bind(
            "Left/PauseDaytimeSpawning", transform,
            Imperium.MoonManager.DaytimeSpawningPaused,
            theme
        );
    }

    private void InitGeneration()
    {
        levelSeedTitle = transform.Find("Right/Generation/Seed/Title").GetComponent<TMP_Text>();
        levelSeedText = transform.Find("Right/Generation/Seed/Input/Text Area/Text").GetComponent<TMP_Text>();

        ImpButton.Bind(
            "Right/Generation/Seed/Reset",
            transform,
            () => Imperium.GameManager.CustomSeed.Reset(),
            theme: theme,
            interactableInvert: true,
            interactableBindings: Imperium.IsSceneLoaded
        );

        levelSeedInput = ImpInput.Bind(
            "Right/Generation/Seed/Input",
            transform,
            Imperium.GameManager.CustomSeed,
            theme: theme,
            interactableInvert: true,
            interactableBindings: Imperium.IsSceneLoaded
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

    protected override void OnOpen()
    {
        levelSeedInput.text = Imperium.IsSceneLoaded.Value
            ? Imperium.StartOfRound.randomMapSeed.ToString()
            : Imperium.GameManager.CustomSeed.Value != -1
                ? Imperium.GameManager.CustomSeed.Value.ToString()
                : "";

        Imperium.GameManager.ProfitQuota.Refresh();
        Imperium.GameManager.GroupCredits.Refresh();
        Imperium.GameManager.QuotaDeadline.Refresh();

        levelSeedInput.interactable = !Imperium.IsSceneLoaded.Value;
        ImpUtils.Interface.ToggleTextActive(levelSeedTitle, !Imperium.IsSceneLoaded.Value);
        ImpUtils.Interface.ToggleTextActive(levelSeedText, !Imperium.IsSceneLoaded.Value);
    }
}
#region

using Imperium.Core.Lifecycle;
using Imperium.MonoBehaviours.ImpUI.Common;
using TMPro;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI.Windows;

internal class ControlCenterWindow : BaseWindow
{
    private TMP_Text title;

    protected override void RegisterWindow()
    {
        title = titleBox.Find("Title").GetComponent<TMP_Text>();

        InitMapObstacleButtons();
        InitSpawnPropertyFields();
    }

    protected override void OnOpen()
    {
        title.text = Imperium.RoundManager.currentLevel.PlanetName;
    }

    private void InitSpawnPropertyFields()
    {
        ImpInput.Bind(
            "EntitySpawning/MinIndoorSpawns/Input",
            content,
            Imperium.MoonManager.MinIndoorSpawns,
            themeBinding
        );
        ImpInput.Bind(
            "EntitySpawning/MinOutdoorSpawns/Input",
            content,
            Imperium.MoonManager.MinOutdoorSpawns,
            themeBinding
        );
        ImpInput.Bind(
            "EntitySpawning/MaxIndoorPower/Input",
            content,
            Imperium.MoonManager.MaxIndoorPower,
            themeBinding
        );
        ImpInput.Bind(
            "EntitySpawning/MaxOutdoorPower/Input",
            content,
            Imperium.MoonManager.MaxOutdoorPower,
            themeBinding
        );
        ImpInput.Bind(
            "EntitySpawning/MaxDaytimePower/Input",
            content,
            Imperium.MoonManager.MaxDaytimePower,
            themeBinding
        );
        ImpInput.Bind(
            "EntitySpawning/IndoorDeviation/Input",
            content,
            Imperium.MoonManager.IndoorDeviation,
            themeBinding
        );
        ImpInput.CreateStatic(
            "EntitySpawning/OutdoorDeviation/Input",
            content,
            "3",
            themeBinding
        );
        ImpInput.Bind(
            "EntitySpawning/DaytimeDeviation/Input",
            content,
            Imperium.MoonManager.DaytimeDeviation,
            themeBinding
        );

        ImpInput.Bind("WeatherVariables/Variable1/Input", content, Imperium.MoonManager.WeatherVariable1, themeBinding);
        ImpInput.Bind("WeatherVariables/Variable2/Input", content, Imperium.MoonManager.WeatherVariable2, themeBinding);
    }

    private void InitMapObstacleButtons()
    {
        ImpButton.Bind(
            "MapObstacles/Doors/Left/OpenDoors",
            content,
            () => MoonManager.ToggleDoors(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/Doors/Left/CloseDoors",
            content,
            () => MoonManager.ToggleDoors(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/Doors/Right/LockDoors",
            content,
            () => MoonManager.ToggleDoorLocks(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/Doors/Right/UnlockDoors",
            content,
            () => MoonManager.ToggleDoorLocks(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/Electronics/Left/OpenSecurity",
            content,
            () => MoonManager.ToggleSecurityDoors(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );
        ImpButton.Bind(
            "MapObstacles/Electronics/Left/CloseSecurity",
            content,
            () => MoonManager.ToggleSecurityDoors(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/Electronics/Right/TurnOnBreakers",
            content,
            () => MoonManager.ToggleBreakers(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/Electronics/Right/TurnOffBreakers",
            content,
            () => MoonManager.ToggleBreakers(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/MapHazards/Left/EnableTurrets",
            content,
            () => MoonManager.ToggleTurrets(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/MapHazards/Left/DisableTurrets",
            content,
            () => MoonManager.ToggleTurrets(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/MapHazards/Right/EnableLandmines",
            content,
            () => MoonManager.ToggleLandmines(true),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );

        ImpButton.Bind(
            "MapObstacles/MapHazards/Right/DisableLandmines",
            content,
            () => MoonManager.ToggleLandmines(false),
            interactableBindings: Imperium.IsSceneLoaded,
            theme: themeBinding
        );
    }
}
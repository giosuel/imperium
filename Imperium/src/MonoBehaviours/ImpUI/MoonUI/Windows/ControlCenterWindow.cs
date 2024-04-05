#region

using Imperium.Core;
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
        ImpInput.Bind("EntitySpawning/MinIndoorSpawns/Input", content, Imperium.GameManager.MinIndoorSpawns);
        ImpInput.Bind("EntitySpawning/MinOutdoorSpawns/Input", content, Imperium.GameManager.MinOutdoorSpawns);

        ImpInput.Bind("EntitySpawning/MaxIndoorPower/Input", content, Imperium.GameManager.MaxIndoorPower);
        ImpInput.Bind("EntitySpawning/MaxOutdoorPower/Input", content, Imperium.GameManager.MaxOutdoorPower);
        ImpInput.Bind("EntitySpawning/MaxDaytimePower/Input", content, Imperium.GameManager.MaxDaytimePower);

        ImpInput.Bind("EntitySpawning/IndoorDeviation/Input", content, Imperium.GameManager.IndoorDeviation);
        ImpInput.CreateStatic("EntitySpawning/OutdoorDeviation/Input", content, "3");
        ImpInput.Bind("EntitySpawning/DaytimeDeviation/Input", content, Imperium.GameManager.DaytimeDeviation);

        ImpInput.Bind("WeatherVariables/Variable1/Input", content, Imperium.GameManager.WeatherVariable1);
        ImpInput.Bind("WeatherVariables/Variable2/Input", content, Imperium.GameManager.WeatherVariable2);
    }

    private void InitMapObstacleButtons()
    {
        ImpButton.Bind(
            "MapObstacles/Doors/Left/OpenDoors",
            content,
            () => GameManager.ToggleDoors(true),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/Doors/Left/CloseDoors",
            content,
            () => GameManager.ToggleDoors(false),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/Doors/Right/LockDoors",
            content,
            () => GameManager.ToggleDoorLocks(true),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/Doors/Right/UnlockDoors",
            content,
            () => GameManager.ToggleDoorLocks(false),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/Electronics/Left/OpenSecurity",
            content,
            () => GameManager.ToggleSecurityDoors(true),
            interactableBindings: Imperium.IsSceneLoaded
        );
        ImpButton.Bind(
            "MapObstacles/Electronics/Left/CloseSecurity",
            content,
            () => GameManager.ToggleSecurityDoors(false),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/Electronics/Right/TurnOnBreakers",
            content,
            () => GameManager.ToggleBreakers(true),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/Electronics/Right/TurnOffBreakers",
            content,
            () => GameManager.ToggleBreakers(false),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/MapHazards/Left/EnableTurrets",
            content,
            () => GameManager.ToggleTurrets(true),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/MapHazards/Left/DisableTurrets",
            content,
            () => GameManager.ToggleTurrets(false),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/MapHazards/Right/EnableLandmines",
            content,
            () => GameManager.ToggleLandmines(true),
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpButton.Bind(
            "MapObstacles/MapHazards/Right/DisableLandmines",
            content,
            () => GameManager.ToggleLandmines(false),
            interactableBindings: Imperium.IsSceneLoaded
        );
    }
}
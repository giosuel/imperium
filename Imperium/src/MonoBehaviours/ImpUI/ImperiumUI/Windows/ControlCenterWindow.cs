#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.MonoBehaviours.ImpUI.SaveUI;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.Windows;

internal class ControlCenterWindow : BaseWindow
{
    private TMP_InputField levelSeedInput;
    private TMP_Text levelSeedTitle;
    private TMP_Text levelSeedText;

    protected override void RegisterWindow()
    {
        titleBox.Find("Settings").GetComponent<Button>().onClick.AddListener(OpenSettingsUI);
        titleBox.Find("SaveFileEditor").GetComponent<Button>().onClick.AddListener(OpenSaveUI);

        content.Find("RenderSettings").GetComponent<Button>().onClick.AddListener(OpenRenderingUI);
        content.Find("MoonSettings").GetComponent<Button>().onClick.AddListener(OpenMoonUI);

        InitQuotaAndCredits();
        InitEntitySpawning();
        InitGeneration();
        InitPlayerSettings();
        InitGameSettings();
        InitOverlays();
        InitTimeSpeed();
    }

    private void InitGeneration()
    {
        levelSeedTitle = content.Find("Left/Seed/Title").GetComponent<TMP_Text>();
        levelSeedText = content.Find("Left/Seed/Input/Text Area/Text").GetComponent<TMP_Text>();

        ImpButton.Bind(
            "Left/Seed/Reset",
            content,
            Imperium.GameManager.CustomSeed.Reset,
            interactableInvert: true,
            interactableBindings: Imperium.IsSceneLoaded
        );

        levelSeedInput = ImpInput.Bind(
            "Left/Seed/Input",
            content,
            Imperium.GameManager.CustomSeed,
            interactableInvert: true,
            interactableBindings: Imperium.IsSceneLoaded
        );
    }

    protected override void OnOpen()
    {
        levelSeedInput.text = Imperium.IsSceneLoaded.Value
            ? Imperium.StartOfRound.randomMapSeed.ToString()
            : Imperium.GameManager.CustomSeed.Value != -1
                ? Imperium.GameManager.CustomSeed.Value.ToString()
                : "";

        levelSeedInput.interactable = !Imperium.IsSceneLoaded.Value;
        ImpUtils.Interface.ToggleTextActive(levelSeedTitle, !Imperium.IsSceneLoaded.Value);
        ImpUtils.Interface.ToggleTextActive(levelSeedText, !Imperium.IsSceneLoaded.Value);
    }

    private void InitEntitySpawning()
    {
        ImpToggle.Bind(
            "Left/PauseIndoorSpawning", content,
            Imperium.GameManager.IndoorSpawningPaused
        );

        ImpToggle.Bind(
            "Left/PauseOutdoorSpawning", content,
            Imperium.GameManager.OutdoorSpawningPaused
        );

        ImpToggle.Bind(
            "Left/PauseDaytimeSpawning", content,
            Imperium.GameManager.DaytimeSpawningPaused
        );
    }

    private void InitQuotaAndCredits()
    {
        ImpInput.Bind("Left/GroupCredits/Input", content, Imperium.GameManager.GroupCredits, min: 0);
        ImpInput.Bind("Left/ProfitQuota/Input", content, Imperium.GameManager.ProfitQuota, min: 0);
        ImpInput.Bind("Left/QuotaDeadline/Input", content, Imperium.GameManager.QuotaDeadline, min: 0);

        ImpButton.Bind("Left/ProfitButtons/FulfillQuota", content, Imperium.GameManager.FulfillQuota);
        ImpButton.Bind("Left/ProfitButtons/ResetQuota", content, Imperium.GameManager.ResetQuota);
    }

    private void InitGameSettings()
    {
        ImpToggle.Bind("Right/GameSettings/OverwriteShipDoors", content, ImpSettings.Game.OverwriteShipDoors);
        ImpToggle.Bind("Right/GameSettings/MuteShipSpeaker", content, ImpSettings.Game.MuteShipSpeaker);
    }

    private void InitOverlays()
    {
        ImpToggle.Bind("Right/Colliders/Employees", content, ImpSettings.Visualizations.Employees);
        ImpToggle.Bind("Right/Colliders/Entities", content, ImpSettings.Visualizations.Entities);
        ImpToggle.Bind("Right/Colliders/MapHazards", content, ImpSettings.Visualizations.MapHazards);
        ImpToggle.Bind("Right/Colliders/Props", content, ImpSettings.Visualizations.Props);
        ImpToggle.Bind("Right/Colliders/Vents", content, ImpSettings.Visualizations.Vents);
        ImpToggle.Bind("Right/Colliders/Foliage", content, ImpSettings.Visualizations.Foliage);

        ImpToggle.Bind("Right/Overlays/Players", content, ImpSettings.Visualizations.PlayerInfo);
        ImpToggle.Bind("Right/Overlays/Entities", content, ImpSettings.Visualizations.EntityInfo);
        ImpToggle.Bind("Right/Overlays/AINodesIndoor", content, ImpSettings.Visualizations.AINodesIndoor);
        ImpToggle.Bind("Right/Overlays/AINodesOutdoor", content, ImpSettings.Visualizations.AINodesOutdoor);
        ImpToggle.Bind("Right/Overlays/BeeSpawns", content, ImpSettings.Visualizations.BeeSpawns);
        ImpToggle.Bind("Right/Overlays/TileBorders", content, ImpSettings.Visualizations.TileBorders);

        ImpToggle.Bind("Left/Gizmos/SpawnIndicators", content, ImpSettings.Visualizations.SpawnIndicators);
        ImpToggle.Bind("Left/Gizmos/SpawnTimers", content, ImpSettings.Visualizations.SpawnTimers);

        ImpToggle.Bind("Left/CastingIndicators/Shotguns", content, ImpSettings.Visualizations.ShotgunIndicators);
        ImpToggle.Bind("Left/CastingIndicators/Shovels", content, ImpSettings.Visualizations.ShovelIndicators);
        ImpToggle.Bind("Left/CastingIndicators/Knives", content, ImpSettings.Visualizations.KnifeIndicators);
        ImpToggle.Bind("Left/CastingIndicators/Landmines", content, ImpSettings.Visualizations.LandmineIndicators);
        ImpToggle.Bind("Left/CastingIndicators/SpikeTraps", content, ImpSettings.Visualizations.SpikeTrapIndicators);
    }

    private void InitPlayerSettings()
    {
        ImpToggle.Bind("Right/PlayerSettings/GodMode", content, ImpSettings.Player.GodMode);
        ImpToggle.Bind("Right/PlayerSettings/Muted", content, ImpSettings.Player.Muted);
        ImpToggle.Bind("Right/PlayerSettings/InfiniteSprint", content, ImpSettings.Player.InfiniteSprint);
        ImpToggle.Bind("Right/PlayerSettings/Invisibility", content, ImpSettings.Player.Invisibility);
        ImpToggle.Bind("Right/PlayerSettings/DisableLocking", content, ImpSettings.Player.DisableLocking);
        ImpToggle.Bind("Right/PlayerSettings/InfiniteBattery", content, ImpSettings.Player.InfiniteBattery);
        ImpToggle.Bind("Right/PlayerSettings/PickupOverwrite", content, ImpSettings.Player.PickupOverwrite);

        ImpSlider.Bind(
            path: "Right/FieldOfView",
            container: content,
            valueBinding: ImpSettings.Player.CustomFieldOfView,
            indicatorDefaultValue: ImpConstants.DefaultFOV,
            indicatorUnit: "°"
        );

        ImpSlider.Bind(
            path: "Right/MovementSpeed",
            container: content,
            valueBinding: ImpSettings.Player.MovementSpeed
        );

        ImpSlider.Bind(
            path: "Right/JumpForce",
            container: content,
            valueBinding: ImpSettings.Player.JumpForce
        );

        ImpSlider.Bind(
            path: "Right/NightVision",
            container: content,
            valueBinding: ImpSettings.Player.NightVision,
            indicatorUnit: "%"
        );
    }

    public void InitTimeSpeed()
    {
        var timeScaleInteractable = new ImpBinding<bool>(false);
        Imperium.GameManager.TimeIsPaused.onUpdate += isPaused =>
        {
            timeScaleInteractable.Set(!isPaused && Imperium.IsSceneLoaded.Value);
        };
        Imperium.IsSceneLoaded.onUpdate += isSceneLoaded =>
        {
            timeScaleInteractable.Set(isSceneLoaded && !Imperium.GameManager.TimeIsPaused.Value);
        };

        ImpSlider.Bind(
            path: "Right/TimeSpeed",
            container: content,
            valueBinding: Imperium.GameManager.TimeSpeed,
            indicatorFormatter: ImpUtils.Math.FormatFloatToThreeDigits,
            useLogarithmicScale: true,
            debounceTime: 0.05f,
            interactableBindings: timeScaleInteractable
        );
        ImpToggle.Bind("Right/TimeSettings/Pause", content, Imperium.GameManager.TimeIsPaused);

        ImpToggle.Bind("Right/TimeSettings/RealtimeClock", content, ImpSettings.Time.RealtimeClock);
        ImpToggle.Bind("Right/TimeSettings/PermanentClock", content, ImpSettings.Time.PermanentClock);
    }

    private static void OpenMoonUI() => Imperium.Interface.Open<MoonUI.MoonUI>();
    private static void OpenRenderingUI() => Imperium.Interface.Open<RenderingUI.RenderingUI>();
    private static void OpenSettingsUI() => Imperium.Interface.Open<SettingsUI.SettingsUI>();
    private static void OpenSaveUI() => Imperium.Interface.Open<ConfirmationUI>();
}
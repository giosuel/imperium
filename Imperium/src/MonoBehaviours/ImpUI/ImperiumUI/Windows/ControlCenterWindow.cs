#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.MonoBehaviours.ImpUI.SaveUI;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.Windows;

internal class ControlCenterWindow : BaseWindow
{
    private TMP_InputField levelSeedInput;
    private TMP_Text levelSeedTitle;
    private TMP_Text levelSeedText;

    protected override void RegisterWindow()
    {
        ImpButton.Bind("Settings", titleBox, OpenSettingsUI, theme: themeBinding, isIconButton: true);
        ImpButton.Bind("SaveFileEditor", titleBox, OpenSaveUI, theme: themeBinding, isIconButton: true);

        ImpButton.Bind("RenderSettings", content, OpenRenderingUI, theme: themeBinding);
        ImpButton.Bind("MoonSettings", content, OpenMoonUI, theme: themeBinding);
        ImpButton.Bind("Visualization", content, OpenVisualizationUI, theme: themeBinding);

        InitQuotaAndCredits();
        InitEntitySpawning();
        InitGeneration();
        InitPlayerSettings();
        InitGameSettings();
        InitTimeSpeed();
    }

    protected override void OnThemeUpdate(ImpTheme theme)
    {
        ImpThemeManager.Style(
            theme,
            content,
            new StyleOverride("Separator", Variant.DARKER)
        );
    }

    private void InitGeneration()
    {
        levelSeedTitle = content.Find("Left/Seed/Title").GetComponent<TMP_Text>();
        levelSeedText = content.Find("Left/Seed/Input/Text Area/Text").GetComponent<TMP_Text>();

        ImpButton.Bind(
            "Left/Seed/Reset",
            content,
            Imperium.GameManager.CustomSeed.Reset,
            theme: themeBinding,
            interactableInvert: true,
            interactableBindings: Imperium.IsSceneLoaded
        );

        levelSeedInput = ImpInput.Bind(
            "Left/Seed/Input",
            content,
            Imperium.GameManager.CustomSeed,
            theme: themeBinding,
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
            Imperium.GameManager.IndoorSpawningPaused,
            themeBinding
        );

        ImpToggle.Bind(
            "Left/PauseOutdoorSpawning", content,
            Imperium.GameManager.OutdoorSpawningPaused,
            themeBinding
        );

        ImpToggle.Bind(
            "Left/PauseDaytimeSpawning", content,
            Imperium.GameManager.DaytimeSpawningPaused,
            themeBinding
        );
    }

    private void InitQuotaAndCredits()
    {
        ImpInput.Bind(
            "Left/GroupCredits/Input",
            content,
            Imperium.GameManager.GroupCredits,
            min: 0,
            theme: themeBinding
        );
        ImpInput.Bind(
            "Left/ProfitQuota/Input",
            content,
            Imperium.GameManager.ProfitQuota,
            min: 0,
            theme: themeBinding
        );
        ImpInput.Bind(
            "Left/QuotaDeadline/Input",
            content,
            Imperium.GameManager.QuotaDeadline,
            min: 0,
            theme: themeBinding
        );
        ImpButton.Bind(
            "Left/ProfitButtons/FulfillQuota",
            content,
            Imperium.GameManager.FulfillQuota,
            theme: themeBinding
        );
        ImpButton.Bind(
            "Left/ProfitButtons/ResetQuota",
            content,
            Imperium.GameManager.ResetQuota,
            theme: themeBinding
        );
    }

    private void InitGameSettings()
    {
        ImpToggle.Bind(
            "Right/GameSettings/OverwriteShipDoors",
            content,
            ImpSettings.Game.OverwriteShipDoors,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/GameSettings/MuteShipSpeaker",
            content, ImpSettings.Game.MuteShipSpeaker,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/GameSettings/PreventShipLeave",
            content, ImpSettings.Game.PreventShipLeave,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/GameSettings/AllPlayersDead",
            content, Imperium.GameManager.AllPlayersDead,
            theme: themeBinding
        );
    }

    private void InitPlayerSettings()
    {
        ImpToggle.Bind("Right/PlayerSettings/GodMode", content, ImpSettings.Player.GodMode, themeBinding);
        ImpToggle.Bind("Right/PlayerSettings/Muted", content, ImpSettings.Player.Muted, themeBinding);
        ImpToggle.Bind("Right/PlayerSettings/InfiniteSprint", content, ImpSettings.Player.InfiniteSprint, themeBinding);
        ImpToggle.Bind("Right/PlayerSettings/Invisibility", content, ImpSettings.Player.Invisibility, themeBinding);
        ImpToggle.Bind("Right/PlayerSettings/DisableLocking", content, ImpSettings.Player.DisableLocking, themeBinding);
        ImpToggle.Bind(
            "Right/PlayerSettings/InfiniteBattery",
            content,
            ImpSettings.Player.InfiniteBattery,
            themeBinding
        );
        ImpToggle.Bind(
            "Right/PlayerSettings/PickupOverwrite",
            content,
            ImpSettings.Player.PickupOverwrite,
            themeBinding
        );
        ImpSlider.Bind(
            path: "Right/FieldOfView",
            container: content,
            valueBinding: ImpSettings.Player.CustomFieldOfView,
            indicatorDefaultValue: ImpConstants.DefaultFOV,
            indicatorUnit: "°",
            theme: themeBinding
        );

        ImpSlider.Bind(
            path: "Right/MovementSpeed",
            container: content,
            theme: themeBinding,
            valueBinding: ImpSettings.Player.MovementSpeed
        );

        ImpSlider.Bind(
            path: "Right/JumpForce",
            container: content,
            theme: themeBinding,
            valueBinding: ImpSettings.Player.JumpForce
        );

        ImpSlider.Bind(
            path: "Right/NightVision",
            container: content,
            theme: themeBinding,
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
            theme: themeBinding,
            interactableBindings: timeScaleInteractable
        );
        ImpToggle.Bind(
            "Right/TimeSettings/Pause",
            content,
            Imperium.GameManager.TimeIsPaused,
            themeBinding,
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpToggle.Bind("Right/TimeSettings/RealtimeClock", content, ImpSettings.Time.RealtimeClock, themeBinding);
        ImpToggle.Bind("Right/TimeSettings/PermanentClock", content, ImpSettings.Time.PermanentClock, themeBinding);
    }

    private static void OpenMoonUI() => Imperium.Interface.Open<MoonUI.MoonUI>();
    private static void OpenRenderingUI() => Imperium.Interface.Open<RenderingUI.RenderingUI>();
    private static void OpenSettingsUI() => Imperium.Interface.Open<SettingsUI.SettingsUI>();
    private static void OpenSaveUI() => Imperium.Interface.Open<ConfirmationUI>();
    private static void OpenVisualizationUI() => Imperium.Interface.Open<VisualizationUI.VisualizationUI>();
}
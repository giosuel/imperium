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
        // InitTimeSpeed();
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
            () => Imperium.GameManager.CustomSeed.Reset(),
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

        Imperium.GameManager.ProfitQuota.Refresh();
        Imperium.GameManager.GroupCredits.Refresh();
        Imperium.GameManager.QuotaDeadline.Refresh();

        levelSeedInput.interactable = !Imperium.IsSceneLoaded.Value;
        ImpUtils.Interface.ToggleTextActive(levelSeedTitle, !Imperium.IsSceneLoaded.Value);
        ImpUtils.Interface.ToggleTextActive(levelSeedText, !Imperium.IsSceneLoaded.Value);
    }

    private void InitEntitySpawning()
    {
        ImpToggle.Bind(
            "Left/PauseIndoorSpawning", content,
            Imperium.MoonManager.IndoorSpawningPaused,
            themeBinding
        );

        ImpToggle.Bind(
            "Left/PauseOutdoorSpawning", content,
            Imperium.MoonManager.OutdoorSpawningPaused,
            themeBinding
        );

        ImpToggle.Bind(
            "Left/PauseDaytimeSpawning", content,
            Imperium.MoonManager.DaytimeSpawningPaused,
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
            max: 3,
            theme: themeBinding,
            interactableInvert: true,
            interactableBindings: Imperium.GameManager.DisableQuota
        );
        ImpButton.Bind(
            "Left/ProfitButtons/FulfillQuota",
            content,
            () => Imperium.GameManager.FulfillQuotaEvent.DispatchToServer(),
            theme: themeBinding
        );
        ImpButton.Bind(
            "Left/ProfitButtons/ResetQuota",
            content,
            () => Imperium.GameManager.ProfitQuota.Reset(),
            theme: themeBinding
        );

        ImpToggle.Bind("Left/DisableQuota", content, Imperium.GameManager.DisableQuota, theme: themeBinding);
    }

    private void InitGameSettings()
    {
        ImpToggle.Bind(
            "Right/GameSettings/UnlockShop",
            content,
            Imperium.Settings.Game.UnlockShop,
            theme: themeBinding
        );

        ImpToggle.Bind(
            "Right/ShipSettings/InstantLanding",
            content,
            Imperium.Settings.Ship.InstantLanding,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/ShipSettings/InstantTakeoff",
            content,
            Imperium.Settings.Ship.InstantTakeoff,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/ShipSettings/OverwriteDoors",
            content,
            Imperium.Settings.Ship.OverwriteDoors,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/ShipSettings/MuteSpeaker",
            content,
            Imperium.Settings.Ship.MuteSpeaker,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/ShipSettings/PreventLeave",
            content,
            Imperium.Settings.Ship.PreventLeave,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/ShipSettings/DisableAbandoned",
            content,
            Imperium.Settings.Ship.DisableAbandoned,
            theme: themeBinding
        );

        ImpToggle.Bind(
            "Right/AnimationSettings/Scoreboard",
            content,
            Imperium.Settings.AnimationSkipping.Scoreboard,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/AnimationSettings/PlayerSpawn",
            content,
            Imperium.Settings.AnimationSkipping.PlayerSpawn,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/AnimationSettings/InteractHold",
            content,
            Imperium.Settings.AnimationSkipping.InteractHold,
            theme: themeBinding
        );
        ImpToggle.Bind(
            "Right/AnimationSettings/Interact",
            content,
            Imperium.Settings.AnimationSkipping.Interact,
            theme: themeBinding
        );
    }

    private void InitPlayerSettings()
    {
        ImpToggle.Bind("Right/PlayerSettings/GodMode", content, Imperium.Settings.Player.GodMode, themeBinding);
        ImpToggle.Bind("Right/PlayerSettings/Muted", content, Imperium.Settings.Player.Muted, themeBinding);
        ImpToggle.Bind("Right/PlayerSettings/InfiniteSprint", content, Imperium.Settings.Player.InfiniteSprint, themeBinding);
        ImpToggle.Bind("Right/PlayerSettings/Invisibility", content, Imperium.Settings.Player.Invisibility, themeBinding);
        ImpToggle.Bind("Right/PlayerSettings/DisableLocking", content, Imperium.Settings.Player.DisableLocking, themeBinding);
        ImpToggle.Bind(
            "Right/PlayerSettings/InfiniteBattery",
            content,
            Imperium.Settings.Player.InfiniteBattery,
            themeBinding
        );
        ImpToggle.Bind(
            "Right/PlayerSettings/PickupOverwrite",
            content,
            Imperium.Settings.Player.PickupOverwrite,
            themeBinding
        );
        ImpToggle.Bind(
            "Right/PlayerSettings/DisableOOB",
            content,
            Imperium.Settings.Player.DisableOOB,
            themeBinding
        );
        ImpToggle.Bind(
            "Right/PlayerSettings/EnableFlying",
            content,
            Imperium.Settings.Player.EnableFlying,
            themeBinding
        );
        ImpToggle.Bind(
            "Right/PlayerSettings/FlyingNoClip",
            content,
            Imperium.Settings.Player.FlyingNoClip,
            themeBinding,
            interactableBindings: Imperium.Settings.Player.EnableFlying
        );

        ImpSlider.Bind(
            path: "Right/FieldOfView",
            container: content,
            valueBinding: Imperium.Settings.Player.CustomFieldOfView,
            indicatorDefaultValue: ImpConstants.DefaultFOV,
            indicatorUnit: "°",
            theme: themeBinding
        );

        ImpSlider.Bind(
            path: "Right/MovementSpeed",
            container: content,
            theme: themeBinding,
            valueBinding: Imperium.Settings.Player.MovementSpeed
        );

        ImpSlider.Bind(
            path: "Right/JumpForce",
            container: content,
            theme: themeBinding,
            valueBinding: Imperium.Settings.Player.JumpForce
        );

        ImpSlider.Bind(
            path: "Right/NightVision",
            container: content,
            theme: themeBinding,
            valueBinding: Imperium.Settings.Player.NightVision,
            indicatorUnit: "%"
        );
    }

    public void InitTimeSpeed()
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
            path: "Right/TimeSpeed",
            container: content,
            valueBinding: Imperium.MoonManager.TimeSpeed,
            indicatorFormatter: Formatting.FormatFloatToThreeDigits,
            useLogarithmicScale: true,
            debounceTime: 0.05f,
            theme: themeBinding,
            interactableBindings: timeScaleInteractable
        );
        ImpToggle.Bind(
            "Right/TimeSettings/Pause",
            content,
            Imperium.MoonManager.TimeIsPaused,
            themeBinding,
            interactableBindings: Imperium.IsSceneLoaded
        );

        ImpToggle.Bind("Right/TimeSettings/RealtimeClock", content, Imperium.Settings.Time.RealtimeClock, themeBinding);
        ImpToggle.Bind("Right/TimeSettings/PermanentClock", content, Imperium.Settings.Time.PermanentClock, themeBinding);
    }

    private static void OpenMoonUI() => Imperium.Interface.Open<MoonUI.MoonUI>();
    private static void OpenRenderingUI() => Imperium.Interface.Open<RenderingUI.RenderingUI>();
    private static void OpenSettingsUI() => Imperium.Interface.Open<SettingsUI.SettingsUI>();
    private static void OpenSaveUI() => Imperium.Interface.Open<ConfirmationUI>();
    private static void OpenVisualizationUI() => Imperium.Interface.Open<VisualizationUI.VisualizationUI>();
}
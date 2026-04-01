#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Preferences;

internal class PreferencesWindow : ImperiumWindow
{
    private Transform content;
    private TMP_Dropdown launchModeDropdown;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        var general = content.Find("Grid/General/General");
        ImpToggle.Bind("LoggingToggle", general, Imperium.Settings.Preferences.GeneralLogging, theme);
        ImpToggle.Bind("OracleLoggingToggle", general, Imperium.Settings.Preferences.OracleLogging, theme);
        ImpToggle.Bind("LeftHandedToggle", general, Imperium.Settings.Preferences.LeftHandedMode, theme);
        ImpToggle.Bind("CustomWelcome", general, Imperium.Settings.Preferences.CustomWelcome, theme);
        ImpToggle.Bind("TooltipsToggle", general, Imperium.Settings.Preferences.ShowTooltips, theme);

        // Play click sounds needs to be the opposite of the setting here, as we are about to toggle it
        ImpToggle.Bind(
            "SoundsToggle",
            general,
            Imperium.Settings.Preferences.PlaySounds,
            theme,
            playClickSound:
            !Imperium.Settings.Preferences.PlaySounds.Value
        );

        ImpToggle.Bind(
            "UEMouseFixToggle",
            general,
            Imperium.Settings.Preferences.UnityExplorerMouseFix,
            theme,
            interactableBindings: new ImpBinding<bool>(Chainloader.PluginInfos.ContainsKey("com.sinai.unityexplorer")),
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Unity Explorer Mouse Fix",
                Description = "Makes it so the camera doesn't follow the mouse\nwhen unity explorer is open.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "DisableFlipCamera",
            general,
            Imperium.Settings.Preferences.DisableFlipCamera,
            theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Disable Camera Flipping",
                Description = "Disables the camera flipping effect on 1st of April.",
                Tooltip = tooltip
            }
        );

        var hosting = content.Find("Grid/Hosting/Hosting");
        ImpToggle.Bind(
            "AllowClients",
            hosting,
            Imperium.Settings.Preferences.AllowClients,
            theme,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Allow Imperium Clients",
                Description = "Whether clients are allowed to use Imperium in the current lobby.",
                Tooltip = tooltip
            }
        );

        var notifications = content.Find("Grid/Notifications/Notifications");
        ImpToggle.Bind("GodModeToggle", notifications, Imperium.Settings.Preferences.NotificationsGodMode, theme);
        ImpToggle.Bind("OracleToggle", notifications, Imperium.Settings.Preferences.NotificationsOracle, theme);
        ImpToggle.Bind("SpawnReportsToggle", notifications, Imperium.Settings.Preferences.NotificationsSpawnReports, theme);
        ImpToggle.Bind("ConfirmationToggle", notifications, Imperium.Settings.Preferences.NotificationsConfirmation, theme);
        ImpToggle.Bind("EntitiesToggle", notifications, Imperium.Settings.Preferences.NotificationsEntities, theme);
        ImpToggle.Bind("SpawningToggle", notifications, Imperium.Settings.Preferences.NotificationsSpawning, theme);
        ImpToggle.Bind("AccessControl", notifications, Imperium.Settings.Preferences.NotificationsAccessControl, theme);
        ImpToggle.Bind("OtherToggle", notifications, Imperium.Settings.Preferences.NotificationsOther, theme);

        InitQuickload();
        InitThemes();

        ImpButton.Bind("Buttons/FactoryReset", content, Imperium.Settings.FactoryReset, theme);
        ImpButton.Bind("Buttons/ResetUI", content, Imperium.Interface.ResetUI, theme);
    }

    private void InitQuickload()
    {
        var quickloadToggles = content.Find("Grid/Quickload/QuickloadToggles");
        ImpToggle.Bind(
            "SkipSplashToggle",
            quickloadToggles,
            Imperium.Settings.Preferences.QuickloadSkipSplash,
            theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Quickload Skip Splash",
                Description = "Whether to skip all splash screens during start-up.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "AutoLaunchToggle",
            quickloadToggles,
            Imperium.Settings.Preferences.QuickloadAutoLaunch,
            theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Quickload Auto Launch",
                Description = "Whether to auto launch into a mode on start-up.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "AutoLoadToggle",
            quickloadToggles,
            Imperium.Settings.Preferences.QuickloadAutoLoad,
            theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Quickload Auto Load",
                Description = "Whether to auto load a save file on start-up.",
                Tooltip = tooltip
            }
        );
        ImpToggle.Bind(
            "CleanSaveToggle",
            quickloadToggles,
            Imperium.Settings.Preferences.QuickloadCleanSave,
            theme,
            interactableBindings: Imperium.Settings.Preferences.QuickloadAutoLoad,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Quickload Clean Save",
                Description = "Whether to delete the auto loaded save on start-up.",
                Tooltip = tooltip
            }
        );

        var quickloadBottom = content.Find("Grid/Quickload/QuickloadBottom");
        quickloadBottom.Find("SaveFile/Title").gameObject.AddComponent<ImpTooltipTrigger>().Init(new TooltipDefinition
        {
            Title = "Quickload Save File",
            Description = "The save file that's auto loaded on start-up.",
            Tooltip = tooltip
        });

        ImpInput.Bind(
            "SaveFile/Input",
            quickloadBottom,
            Imperium.Settings.Preferences.QuickloadSaveNumber,
            theme,
            interactableBindings: Imperium.Settings.Preferences.QuickloadAutoLoad
        );
        ImpUtils.Interface.BindInputInteractable(
            Imperium.Settings.Preferences.QuickloadAutoLoad,
            quickloadBottom.Find("SaveFile")
        );

        launchModeDropdown = quickloadBottom.Find("LaunchMode/Dropdown").GetComponent<TMP_Dropdown>();
        launchModeDropdown.options = Enum.GetValues(typeof(LaunchMode))
            .Cast<LaunchMode>()
            .OrderBy(mode => mode)
            .Select(mode => new TMP_Dropdown.OptionData(mode.ToString()))
            .ToList();

        launchModeDropdown.onValueChanged.AddListener(value =>
        {
            Imperium.Settings.Preferences.QuickloadLaunchMode.Set((LaunchMode)value);
        });
        launchModeDropdown.value = (int)Imperium.Settings.Preferences.QuickloadLaunchMode.Value;

        ImpUtils.Interface.BindDropdownInteractable(
            Imperium.Settings.Preferences.QuickloadAutoLaunch,
            quickloadBottom.Find("LaunchMode")
        );
    }

    private void InitThemes()
    {
        var themeContainer = content.Find("Appearance");
        var hoveredTheme = new ImpBinding<string>();

        for (var i = 0; i < themeContainer.childCount; i++)
        {
            var themeObject = themeContainer.GetChild(i);
            var themeName = themeObject.Find("Text").GetComponent<TMP_Text>().text;

            ImpMultiSelectEntry.Bind(
                themeName,
                themeContainer.GetChild(i).gameObject,
                Imperium.Settings.Preferences.Theme,
                hoveredTheme
            );
        }
    }

    protected override void OnThemeUpdate(ImpTheme updatedTheme)
    {
        ImpThemeManager.Style(
            updatedTheme,
            content,
            new StyleOverride("Appearance", Variant.DARKER)
        );

        // Launch mode dropdown
        ImpThemeManager.Style(
            updatedTheme,
            launchModeDropdown.transform,
            new StyleOverride("", Variant.FOREGROUND),
            new StyleOverride("Arrow", Variant.FOREGROUND),
            new StyleOverride("Template", Variant.DARKER),
            new StyleOverride("Template/Viewport/Content/Item/Background", Variant.DARKER),
            new StyleOverride("Template/Scrollbar", Variant.DARKEST),
            new StyleOverride("Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );

        // Theme entries
        var themeContainer = content.Find("Appearance");
        for (var i = 0; i < themeContainer.childCount; i++)
        {
            ImpThemeManager.Style(
                updatedTheme,
                themeContainer.GetChild(i),
                new StyleOverride("Selected", Variant.FADED),
                new StyleOverride("Hover", Variant.DARKER)
            );
        }
    }
}
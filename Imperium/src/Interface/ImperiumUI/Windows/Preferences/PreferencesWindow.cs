#region

using BepInEx.Bootstrap;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Preferences;

internal class PreferencesWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        var general = content.Find("Grid/General/General");
        ImpToggle.Bind("LoggingToggle", general, Imperium.Settings.Preferences.GeneralLogging, theme);
        ImpToggle.Bind("OracleLoggingToggle", general, Imperium.Settings.Preferences.OracleLogging, theme);
        ImpToggle.Bind("LeftHandedToggle", general, Imperium.Settings.Preferences.LeftHandedMode, theme);
        ImpToggle.Bind("CustomWelcome", general, Imperium.Settings.Preferences.CustomWelcome, theme);
        ImpToggle.Bind("OptimizeLogsToggle", general, Imperium.Settings.Preferences.OptimizeLogs, theme);
        ImpToggle.Bind("TooltipsToggle", general, Imperium.Settings.Preferences.ShowTooltips, theme);

        ImpToggle.Bind(
            "UEMouseFixToggle",
            general,
            Imperium.Settings.Preferences.UnityExplorerMouseFix,
            theme,
            interactableBindings: new ImpBinding<bool>(Chainloader.PluginInfos.ContainsKey("com.sinai.unityexplorer"))
        );

        var hosting = content.Find("Grid/Hosting/Hosting");
        ImpToggle.Bind(
            "AllowClients",
            hosting,
            Imperium.Settings.Preferences.AllowClients,
            theme,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost)
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

        var quickload = content.Find("Grid/Quickload/Quickload");
        ImpToggle.Bind("SkipStartToggle", quickload, Imperium.Settings.Preferences.QuickloadSkipStart, theme);
        ImpToggle.Bind("SkipMenuToggle", quickload, Imperium.Settings.Preferences.QuickloadSkipMenu, theme);
        ImpToggle.Bind("ReloadOnQuitToggle", quickload, Imperium.Settings.Preferences.QuickloadOnQuit, theme);
        ImpToggle.Bind("CleanFileToggle", quickload, Imperium.Settings.Preferences.QuickloadCleanFile, theme);

        ImpInput.Bind(
            "SaveFileContainer/SaveFileNumber/Input",
            content,
            Imperium.Settings.Preferences.QuickloadSaveNumber,
            theme
        );

        ImpButton.Bind("Buttons/FactoryReset", content, Imperium.Settings.FactoryReset, theme);
        ImpButton.Bind("Buttons/ResetUI", content, Imperium.Interface.ResetUI, theme);

        InitThemes();
    }

    protected override void OnThemeUpdate(ImpTheme updatedTheme)
    {
        ImpThemeManager.Style(
            updatedTheme,
            content,
            new StyleOverride("Appearance", Variant.DARKER)
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
}
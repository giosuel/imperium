#region

using System;
using BepInEx.Bootstrap;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.MonoBehaviours.ImpUI.MapUI;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using Unity.Netcode;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SettingsUI;

internal class SettingsUI : SingleplexUI
{
    protected override void InitUI()
    {
        var general = content.Find("Grid/General/General");
        ImpToggle.Bind("LoggingToggle", general, ImpSettings.Preferences.GeneralLogging, theme);
        ImpToggle.Bind("OracleLoggingToggle", general, ImpSettings.Preferences.OracleLogging, theme);
        ImpToggle.Bind("LeftHandedToggle", general, ImpSettings.Preferences.LeftHandedMode, theme);
        ImpToggle.Bind("CustomWelcome", general, ImpSettings.Preferences.CustomWelcome, theme);
        ImpToggle.Bind("OptimizeLogsToggle", general, ImpSettings.Preferences.OptimizeLogs, theme);
        ImpToggle.Bind(
            "UEMouseFixToggle",
            general,
            ImpSettings.Preferences.UnityExplorerMouseFix,
            theme,
            new ImpBinding<bool>(Chainloader.PluginInfos.ContainsKey("com.sinai.unityexplorer"))
        );

        var hosting = content.Find("Grid/Hosting/Hosting");
        ImpToggle.Bind(
            "AllowClients",
            hosting,
            ImpSettings.Preferences.AllowClients,
            theme,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost)
        );

        var notifications = content.Find("Grid/Notifications/Notifications");
        ImpToggle.Bind("GodModeToggle", notifications, ImpSettings.Preferences.NotificationsGodMode, theme);
        ImpToggle.Bind("OracleToggle", notifications, ImpSettings.Preferences.NotificationsOracle, theme);
        ImpToggle.Bind("SpawnReportsToggle", notifications, ImpSettings.Preferences.NotificationsSpawnReports, theme);
        ImpToggle.Bind("ConfirmationToggle", notifications, ImpSettings.Preferences.NotificationsConfirmation, theme);
        ImpToggle.Bind("EntitiesToggle", notifications, ImpSettings.Preferences.NotificationsEntities, theme);
        ImpToggle.Bind("SpawningToggle", notifications, ImpSettings.Preferences.NotificationsSpawning, theme);
        ImpToggle.Bind("AccessControl", notifications, ImpSettings.Preferences.NotificationsAccessControl, theme);
        ImpToggle.Bind("OtherToggle", notifications, ImpSettings.Preferences.NotificationsOther, theme);

        var quickload = content.Find("Grid/Quickload/Quickload");
        ImpToggle.Bind("SkipStartToggle", quickload, ImpSettings.Preferences.QuickloadSkipStart, theme);
        ImpToggle.Bind("SkipMenuToggle", quickload, ImpSettings.Preferences.QuickloadSkipMenu, theme);
        ImpToggle.Bind("ReloadOnQuitToggle", quickload, ImpSettings.Preferences.QuickloadOnQuit, theme);
        ImpToggle.Bind("CleanFileToggle", quickload, ImpSettings.Preferences.QuickloadCleanFile, theme);

        ImpInput.Bind(
            "SaveFileContainer/SaveFileNumber/Input",
            content,
            ImpSettings.Preferences.QuickloadSaveNumber,
            theme
        );

        ImpButton.Bind("Buttons/FactoryReset", content, ImpSettings.FactoryReset, theme);

        InitThemes();
    }

    protected override void OnThemeUpdate(ImpTheme theme)
    {
        ImpThemeManager.Style(
            theme,
            content,
            new StyleOverride("Appearance", Variant.DARKER),
            new StyleOverride("Appearance", Variant.DARKER)
        );

        // Theme entries
        var themeContainer = content.Find("Appearance");
        for (var i = 0; i < themeContainer.childCount; i++)
        {
            ImpThemeManager.Style(
                theme,
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
                ImpSettings.Preferences.Theme,
                hoveredTheme
            );
        }
    }
}
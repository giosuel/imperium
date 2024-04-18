#region

using BepInEx.Bootstrap;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Util.Binding;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SettingsUI;

internal class SettingsUI : StandaloneUI
{
    public override void Awake()
    {
        InitializeUI(Imperium.Interface);
    }

    protected override void InitUI()
    {
        ImpButton.Bind("Buttons/FactoryReset", content, ImpSettings.FactoryReset);

        ImpToggle.Bind("General/LoggingToggle", content, ImpSettings.Preferences.GeneralLogging);
        ImpToggle.Bind("General/OracleLoggingToggle", content, ImpSettings.Preferences.OracleLogging);
        ImpToggle.Bind("General/LeftHandedToggle", content, ImpSettings.Preferences.LeftHandedMode);
        ImpToggle.Bind(
            "General/UEMouseFixToggle",
            content,
            ImpSettings.Preferences.UnityExplorerMouseFix,
            new ImpBinding<bool>(Chainloader.PluginInfos.ContainsKey("com.sinai.unityexplorer"))
        );
        ImpToggle.Bind("General/OptimizeLogsToggle", content, ImpSettings.Preferences.OptimizeLogs);

        ImpToggle.Bind("Notifications/GodModeToggle", content, ImpSettings.Preferences.NotificationsGodMode);
        ImpToggle.Bind("Notifications/OracleToggle", content, ImpSettings.Preferences.NotificationsOracle);
        ImpToggle.Bind("Notifications/SpawnReportsToggle", content, ImpSettings.Preferences.NotificationsSpawnReports);
        ImpToggle.Bind("Notifications/ConfirmationToggle", content, ImpSettings.Preferences.NotificationsConfirmation);
        ImpToggle.Bind("Notifications/EntitiesToggle", content, ImpSettings.Preferences.NotificationsEntities);
        ImpToggle.Bind("Notifications/OtherToggle", content, ImpSettings.Preferences.NotificationsOther);

        ImpToggle.Bind("Quickload/SkipStartToggle", content, ImpSettings.Preferences.QuickloadSkipStart);
        ImpToggle.Bind("Quickload/SkipMenuToggle", content, ImpSettings.Preferences.QuickloadSkipMenu);
        ImpToggle.Bind("Quickload/ReloadOnQuitToggle", content, ImpSettings.Preferences.QuickloadOnQuit);
        ImpToggle.Bind("Quickload/CleanFileToggle", content, ImpSettings.Preferences.QuickloadCleanFile);

        ImpInput.Bind("SaveFileContainer/SaveFileNumber/Input", content, ImpSettings.Preferences.QuickloadSaveNumber);
    }
}
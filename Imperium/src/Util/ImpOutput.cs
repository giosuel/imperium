#region

using System.Collections.Generic;
using Imperium.Core;
using Imperium.Netcode;
using Imperium.Util.Binding;

#endregion

namespace Imperium.Util;

internal abstract class ImpOutput
{
    private static readonly Dictionary<NotificationType, ImpConfig<bool>> NotificationSettings = new()
    {
        { NotificationType.GodMode, ImpSettings.Preferences.NotificationsGodMode },
        { NotificationType.OracleUpdate, ImpSettings.Preferences.NotificationsOracle },
        { NotificationType.Confirmation, ImpSettings.Preferences.NotificationsConfirmation },
        { NotificationType.SpawnReport, ImpSettings.Preferences.NotificationsSpawnReports },
        { NotificationType.Entities, ImpSettings.Preferences.NotificationsEntities },
        { NotificationType.Server, ImpSettings.Preferences.NotificationsServer },
        { NotificationType.Other, ImpSettings.Preferences.NotificationsOther }
    };

    internal static void SendToClients(
        string text,
        string title = "Imperium",
        bool isWarning = false
    )
    {
        if (!ImpNetworkManager.IsHost.Value) return;
        ImpNetCommunication.Instance.SendClientRpc(text, title, isWarning);
    }

    internal static void Send(
        string text,
        string title = "Imperium",
        bool isWarning = false,
        NotificationType notificationType = NotificationType.Other
    )
    {
        if (!HUDManager.Instance)
        {
            Imperium.Log.LogError($"Failed to send notification, HUDManager is not defined, message: {text}");
            return;
        }

        // Disable notifications if turned off or during loading of settings
        if (!NotificationSettings[notificationType].Value || ImpSettings.IsLoading) return;

        if (notificationType == NotificationType.OracleUpdate) title = "Oracle";

        HUDManager.Instance.DisplayTip(title, text, isWarning);
    }
}

internal enum NotificationType
{
    // God mode notifications on taking damage and dying
    GodMode,

    // Oracle spawn prediction updates
    OracleUpdate,

    // Confirmation dialogs following user interaction
    Confirmation,

    // Spawn report every cycle
    SpawnReport,

    // Entity related notifications (e.g. Entity took damage)
    Entities,

    // Any notifications coming from the host
    Server,
    Other
}
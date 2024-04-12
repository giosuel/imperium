#region

using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using Imperium.Core;
using Imperium.Netcode;
using Imperium.Util.Binding;
using UnityEngine;

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

    internal static void LogBlock(List<string> lines, string title = "Imperium Monitoring")
    {
        if (!ImpSettings.Preferences.GeneralLogging.Value) return;

        var output = "[MON] Imperium message block :)\n";
        title = "< " + title + " >";
        var width = Mathf.Max(lines.Max(line => line.Length) + 4, 20);
        var fullWidth = string.Concat(Enumerable.Repeat("\u2550", width - 2));
        var titlePaddingCount = (width - title.Length) / 2 - 1;
        if ((width - title.Length) / 2 % 2 == 0) titlePaddingCount++;

        var titlePadding = string.Concat(Enumerable.Repeat(" ", titlePaddingCount));


        output += "\u2552" + fullWidth + "\u2555\n";
        output += "\u2502" + titlePadding + title + titlePadding + "\u2502\n";
        output += "\u255e" + fullWidth + "\u2561\n";
        output = lines.Aggregate(output,
            (current, line) => current + $"\u2502 {line}".PadRight(width - 2) + " \u2502\n");
        output += "\u2558" + fullWidth + "\u255b";

        Imperium.Log.Log(LogLevel.Message, output);
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
#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using Imperium.API.Types.Networking;
using Imperium.Core;
using Imperium.Util.Binding;
using LogLevel = BepInEx.Logging.LogLevel;

#endregion

namespace Imperium.Util;

internal class ImpOutput(ManualLogSource logger)
{
    // Notification settings will be bound later on when Imperium is loaded
    private Dictionary<NotificationType, ImpBinding<bool>> NotificationSettings = new();

    internal void BindNotificationSettings(ImpSettings settings)
    {
        NotificationSettings = new Dictionary<NotificationType, ImpBinding<bool>>
        {
            { NotificationType.GodMode, settings.Preferences.NotificationsGodMode },
            { NotificationType.OracleUpdate, settings.Preferences.NotificationsOracle },
            { NotificationType.Confirmation, settings.Preferences.NotificationsConfirmation },
            { NotificationType.SpawnReport, settings.Preferences.NotificationsSpawnReports },
            { NotificationType.Entities, settings.Preferences.NotificationsEntities },
            { NotificationType.Server, settings.Preferences.NotificationsServer },
            { NotificationType.AccessControl, settings.Preferences.NotificationsAccessControl },
            { NotificationType.Spawning, settings.Preferences.NotificationsSpawning },
            { NotificationType.Required, new ImpBinding<bool>(true) },
            { NotificationType.Other, settings.Preferences.NotificationsOther }
        };
    }

    private bool IsNotificationEnabled(NotificationType type)
    {
        return NotificationSettings.GetValueOrDefault(type, null)?.Value ?? false;
    }

    internal void Status(string text) => HUDManager.Instance.DisplayStatusEffect(text);
    internal void Debug(string text) => HUDManager.Instance.SetDebugText(text);

    internal void Send(
        string text,
        string title = "Imperium",
        bool isWarning = false,
        NotificationType type = NotificationType.Other
    )
    {
        if (!HUDManager.Instance)
        {
            LogError($"Failed to send notification, HUDManager is not yet instantiated, message: {text}");
            return;
        }

        // Disable notifications if turned off or during loading of settings
        if (!IsNotificationEnabled(type) || Imperium.Settings.IsLoading) return;

        HUDManager.Instance.DisplayTip(title, text, isWarning);
    }

    private const string ELLIPSIS = "...";
    private const int ELLIPSIS_WIDTH = 3;

    private static void Repeat(StringBuilder stringBuilder, char c, int count)
    {
        if (count <= 0) return;
        for (var i = 0; i < count; i++)
        {
            stringBuilder.Append(c);
        }
    }

    private static int Elide(StringBuilder stringBuilder, string value, int maxLength)
    {
        if (maxLength <= 0)
        {
            // No space at all
            return 0;
        }

        if (value.Length <= maxLength)
        {
            // Everything fits
            stringBuilder.Append(value);
            return value.Length;
        }

        // width of the visible text before the ellipsys...
        var elidedLength = maxLength - ELLIPSIS_WIDTH;
        if (elidedLength <= 0)
        {
            // Not enough space to elide
            stringBuilder.Append(value.AsSpan(0, maxLength));
        }
        else
        {
            stringBuilder.Append(value.AsSpan(0, elidedLength));
            stringBuilder.Append(ELLIPSIS);
        }

        return maxLength;
    }

    // Calculate left and right padding around content.
    // Biased to align left if padding is not evenly divisible, i.e. right padding might be larger.
    // Returned padding is non-negative, even if the content overflows.
    private static (int, int) SplitPadding(int contentSize, int maxSize)
    {
        var padding = maxSize - contentSize;
        if (padding <= 0)
        {
            return (0, 0);
        }

        var before = padding / 2;
        var after = padding % 2 == 0 ? before : before + 1;
        return (before, after);
    }

    internal void LogBlock(List<string> lines, string title = "Imperium Monitoring")
    {
        if (!Imperium.Settings.Preferences.GeneralLogging.Value) return;

        // ANSI box table formatting
        //
        // Example 1:
        // ╒════════╕
        // │< abcd >│
        // ╞════════╡
        // │ abcd12 │
        // ╘════════╛
        //
        // Example 2:
        // ╒══════════════════════╕
        // │       < abcd >       │
        // ╞══════════════════════╡
        // │ Shorter line         │
        // │ Slightly longer line │
        // ╘══════════════════════╛
        //
        // Example 3 (with a simulated low limit on line width):
        //    ┌      title line width      ┐
        // ┌< │     padded title width     │ >┐
        // ╒══════════════════════════════════╕
        // │             < abcd >             │
        // ╞══════════════════════════════════╡
        // │ Amazingly long elided excepti... │
        // ╘══════════════════════════════════╛
        // └││           box width       │  ││┘
        //  └│       padded line width   │  │┘
        //   ├      content line width   │  ┘
        //   └   max elided line width   ┘
        //
        // Title is wrapped in angle brackets, adding 4 characters to its width,
        // but title may touch the borders, so title with brackets == padded line width.
        //
        // Content lines are padded with a single whitespace on each side,
        // adding 2 characters to their padded line width.
        //
        // If any line exceeds content line width,
        // the line is trimmed to the max elided line width, and an ellipsis is appended.
        const int BOX_MAX_WIDTH = 512;
        const char BOX_DRAWINGS_LIGHT_VERTICAL = '\u2502';
        // Border width on one side
        const int BOX_BORDER_WIDTH = 1;
        const int PADDED_MAX_WIDTH = BOX_MAX_WIDTH - 2 * BOX_BORDER_WIDTH;
        // Max padded title width is the same, because it touches the borders
        const int PADDED_TITLE_MAX_WIDTH = PADDED_MAX_WIDTH;
        // Title padding on one side (angle bracket and a whitespace)
        const int TITLE_PADDING = 2;
        const int TITLE_TEXT_MAX_WIDTH = PADDED_TITLE_MAX_WIDTH - 2 * TITLE_PADDING;
        // Content padding on one side (just a whitespace)
        const int CONTENT_PADDING = 1;
        // Max content line width is 510 - 2 = 508.
        const int CONTENT_TEXT_MAX_WIDTH = PADDED_TITLE_MAX_WIDTH - 2 * CONTENT_PADDING;

        // First pass: estimate box width
        var paddedWidth = title.Length + 2 * TITLE_PADDING;
        // Note: lines.Max() would crash if empty
        foreach (var line in lines)
        {
            paddedWidth = Math.Max(paddedWidth, line.Length + 2 * CONTENT_PADDING);
        }

        paddedWidth = Math.Min(paddedWidth, PADDED_MAX_WIDTH);
        var boxWidth = paddedWidth + 2 * BOX_BORDER_WIDTH;
        var paddedWidthHorizontalBorder = string.Concat(Enumerable.Repeat('\u2550', paddedWidth));
        var boxBorderTop = '\u2552' + paddedWidthHorizontalBorder + '\u2555';
        var boxBorderMiddle = '\u255e' + paddedWidthHorizontalBorder + '\u2561';
        var boxBorderBottom = '\u2558' + paddedWidthHorizontalBorder + '\u255b';

        // Second pass: elide and pad lines
        List<string> table = new(lines.Count + 4); // 4 = 3 borders + 1 title line
        table.Add(boxBorderTop);
        {
            var titleTextWidth = Math.Min(title.Length, TITLE_TEXT_MAX_WIDTH);
            var paddedTitleWidth = titleTextWidth + 2 * TITLE_PADDING;
            var (paddingLeft, paddingRight) = SplitPadding(paddedTitleWidth, paddedWidth);

            StringBuilder row = new(boxWidth);

            row.Append(BOX_DRAWINGS_LIGHT_VERTICAL);
            Repeat(row, ' ', paddingLeft);
            row.Append("< ");
            Elide(row, title, TITLE_TEXT_MAX_WIDTH);
            row.Append(" >");
            Repeat(row, ' ', paddingRight);
            row.Append(BOX_DRAWINGS_LIGHT_VERTICAL);

            table.Add(row.ToString());
        }
        table.Add(boxBorderMiddle);
        foreach (var line in lines)
        {
            var availableWidth = paddedWidth - 2 * CONTENT_PADDING;
            StringBuilder row = new(boxWidth);

            row.Append(BOX_DRAWINGS_LIGHT_VERTICAL);
            row.Append(' ');
            var usedWidth = Elide(row, line, CONTENT_TEXT_MAX_WIDTH);
            Repeat(row, ' ', availableWidth - usedWidth);
            row.Append(' ');
            row.Append(BOX_DRAWINGS_LIGHT_VERTICAL);

            table.Add(row.ToString());
        }

        table.Add(boxBorderBottom);

        foreach (var row in table)
        {
            Log(LogLevel.Message, row);
        }
    }

    internal void Log(LogLevel level, string text) => logger.Log(level, text);
    internal void LogInfo(string text) => logger.LogInfo(text);
    internal void LogDebug(string text) => logger.LogDebug(text);
    internal void LogWarning(string text) => logger.LogWarning(text);
    internal void LogError(string text) => logger.LogError(text);
}
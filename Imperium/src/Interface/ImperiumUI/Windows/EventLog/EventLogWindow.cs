#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Core.EventLogging;
using Imperium.Interface.Common;
using Imperium.Types;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.EventLog;

internal class EventLogWindow : ImperiumWindow
{
    private GameObject placeholder;
    private GameObject entryTemplate;
    private readonly List<EventLogEntry> entryInstances = [];
    private List<EventLogMessage> visibleMessages = [];

    private RectTransform contentRect;
    private Transform content;
    private ScrollRect scrollRect;

    private float viewHeight;
    private float contentHeight;
    private int itemCount;
    private const float itemHeight = 16;

    protected override void InitWindow()
    {
        content = transform.Find("Content/Viewport/Content");
        entryTemplate = content.Find("Template").gameObject;
        entryTemplate.AddComponent<EventLogEntry>();
        entryTemplate.SetActive(false);

        placeholder = transform.Find("Content/Placeholder").gameObject;
        placeholder.SetActive(true);
        contentRect = content.GetComponent<RectTransform>();

        scrollRect = transform.Find("Content").GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);

        InitLogList();

        ImpButton.Bind(
            "Clear",
            transform,
            OnClear,
            theme: theme
        );
        ImpButton.Bind(
            "ClearPrevious",
            transform,
            OnClearPrevious,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Clear Previous",
                Description = "Clear logs prior to the current day.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind("Footer/Entities", transform, Imperium.Settings.EventLog.EntityLogs, theme: theme);
        ImpToggle.Bind("Footer/Players", transform, Imperium.Settings.EventLog.PlayerLogs, theme: theme);
        ImpToggle.Bind("Footer/Game", transform, Imperium.Settings.EventLog.GameLogs, theme: theme);
        ImpToggle.Bind("Footer/Custom", transform, Imperium.Settings.EventLog.CustomLogs, theme: theme);

        Imperium.Settings.EventLog.EntityLogs.onTrigger += OnTypeToggle;
        Imperium.Settings.EventLog.PlayerLogs.onTrigger += OnTypeToggle;
        Imperium.Settings.EventLog.GameLogs.onTrigger += OnTypeToggle;
        Imperium.Settings.EventLog.CustomLogs.onTrigger += OnTypeToggle;

        Imperium.EventLog.Log.onUpdate += OnLogUpdate;
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdated)
    {
        ImpThemeManager.Style(
            themeUpdated,
            transform,
            new StyleOverride("Content/Background", Variant.BACKGROUND_DARKER),
            new StyleOverride("Content/Viewport/Content/Template/Hover", Variant.FADED),
            new StyleOverride("Content/Scrollbar", Variant.DARKEST),
            new StyleOverride("Content/Scrollbar/SlidingArea/Handle", Variant.FOREGROUND)
        );

        foreach (var entry in entryInstances)
        {
            ImpThemeManager.Style(
                themeUpdated,
                entry.transform,
                new StyleOverride("Hover", Variant.FADED)
            );
        }
    }

    private void OnScroll(Vector2 _) => UpdateList();

    private float originalViewHeight;

    private void InitLogList()
    {
        originalViewHeight = Math.Abs(transform.Find("Content").GetComponent<RectTransform>().sizeDelta.y);
        itemCount = Mathf.CeilToInt(originalViewHeight / itemHeight) + 2;
        viewHeight = itemHeight * itemCount;

        for (var i = 0; i < itemCount; i++)
        {
            var obj = Instantiate(entryTemplate, content);
            var entry = obj.AddComponent<EventLogEntry>();
            obj.gameObject.SetActive(true);
            entryInstances.Add(entry);
        }

        UpdateList();
    }

    private void UpdateList(bool pauseScrolling = false)
    {
        placeholder.SetActive(visibleMessages.Count == 0);

        contentHeight = itemHeight * visibleMessages.Count;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);

        if (pauseScrolling && scrollRect.verticalNormalizedPosition is > 0 and < 1)
        {
            scrollRect.verticalNormalizedPosition -= Mathf.Clamp(itemHeight / (contentHeight - originalViewHeight), 0, 1);
        }

        var position = Math.Max((1 - scrollRect.verticalNormalizedPosition) * (contentHeight - originalViewHeight), 0);

        for (var i = 0; i < entryInstances.Count; i++)
        {
            var pageOffset = i * itemHeight;
            var page = (int)((position + (entryInstances.Count - i) * itemHeight) / viewHeight);
            var entryPosition = page * viewHeight + pageOffset - itemHeight;

            var index = (int)(entryPosition / itemHeight);

            if (index >= visibleMessages.Count)
            {
                entryInstances[i].ClearItem(entryPosition);
            }
            else
            {
                entryInstances[i].SetItem(visibleMessages[index], tooltip, entryPosition);
            }
        }
    }

    private static void OnClear()
    {
        Imperium.EventLog.Log.Value.Clear();
        Imperium.EventLog.Log.Refresh();
    }

    private static void OnClearPrevious()
    {
        Imperium.EventLog.Log.Set(
            Imperium.EventLog.Log.Value.Where(log => log.Day >= Imperium.StartOfRound.gameStats.daysSpent).ToList()
        );
    }

    private void OnTypeToggle()
    {
        visibleMessages = Imperium.EventLog.Log.Value.Where(message => IsMessageVisible(message.Type)).Reverse().ToList();
        UpdateList();
    }

    public void OnLogUpdate(List<EventLogMessage> log)
    {
        visibleMessages = log.Where(message => IsMessageVisible(message.Type)).Reverse().ToList();
        UpdateList(pauseScrolling: true);
    }

    protected override void OnClose()
    {
        foreach (var entry in entryInstances) entry.OnClose();
    }

    private static bool IsMessageVisible(EventLogType type)
    {
        return type == EventLogType.Entity && Imperium.Settings.EventLog.EntityLogs.Value
               || type == EventLogType.Player && Imperium.Settings.EventLog.PlayerLogs.Value
               || type == EventLogType.Game && Imperium.Settings.EventLog.GameLogs.Value
               || type == EventLogType.Custom && Imperium.Settings.EventLog.CustomLogs.Value;
    }
}
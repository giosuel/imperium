#region

using System.Linq;
using Imperium.Core.EventLogging;
using Imperium.Interface.Common;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.EventLog;

internal class EventLogEntry : MonoBehaviour
{
    private TMP_Text time;
    private TMP_Text objectName;
    private TMP_Text message;
    private TMP_Text count;
    private GameObject hover;

    private RectTransform rect;
    private ImpTooltip tooltip;

    private EventLogMessage? currentLog;

    private void Awake()
    {
        rect = gameObject.GetComponent<RectTransform>();

        time = transform.Find("Time/Text").GetComponent<TMP_Text>();
        objectName = transform.Find("Object").GetComponent<TMP_Text>();
        message = transform.Find("Message").GetComponent<TMP_Text>();
        count = transform.Find("Count").GetComponent<TMP_Text>();

        hover = transform.Find("Hover").gameObject;
        var interactable = gameObject.AddComponent<ImpInteractable>();

        interactable.onOver += OnOver;
        interactable.onExit += OnExit;
    }

    internal void ClearItem(float positionY)
    {
        OnExit();

        time.text = "";
        objectName.text = "";
        message.text = "";
        count.text = "";

        tooltip = null;
        currentLog = null;

        rect.anchoredPosition = new Vector2(0, -positionY);
    }

    internal void SetItem(EventLogMessage log, ImpTooltip impTooltip, float positionY)
    {
        time.text = $"[{log.Time}]";
        objectName.text = log.ObjectName;
        message.text = log.Message;
        count.text = log.Count > 0 ? $"({log.Count}x)" : "";

        tooltip = impTooltip;
        currentLog = log;

        rect.anchoredPosition = new Vector2(0, -positionY);
    }

    private void OnOver(Vector2 mousePosition)
    {
        if (currentLog == null || !tooltip) return;

        hover.SetActive(true);
        var detailsString = string.Join(
            "\n", currentLog.Value.Details.Select(detail => $"{detail.Title}: {RichText.Size(detail.Text, 7)}")
        );
        tooltip.SetPosition(currentLog.Value.DetailsTitle, detailsString, mousePosition);
    }

    private void OnExit()
    {
        hover.SetActive(false);
        if (tooltip) tooltip.Deactivate();
    }

    internal void OnClose() => hover.SetActive(false);
}
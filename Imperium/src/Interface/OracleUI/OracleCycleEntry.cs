#region

using System.Collections.Generic;
using Imperium.API.Types;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.OracleUI;

public class OracleCycleEntry : MonoBehaviour
{
    private TMP_Text title;
    private Transform indoorList;
    private Transform outdoorList;
    private Transform daytimeList;

    private readonly List<GameObject> reports = [];

    private GameObject entryTemplate;

    public void Awake()
    {
        title = transform.Find("CycleTitle").GetComponent<TMP_Text>();

        indoorList = transform.Find("Panel/Indoor/List");
        outdoorList = transform.Find("Panel/Outdoor/List");
        daytimeList = transform.Find("Panel/Daytime/List");

        entryTemplate = transform.Find("Panel/Indoor/List/Item").gameObject;
        entryTemplate.SetActive(false);
    }

    internal void Initialize(ImpBinding<ImpTheme> theme)
    {
        theme.onUpdate += OnThemeUpdate;
    }

    private void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("", Variant.DARKER)
        );

        ImpThemeManager.Style(
            themeUpdate,
            entryTemplate.transform,
            new StyleOverride("", Variant.BACKGROUND),
            new StyleOverride("Border", Variant.DARKER)
        );

        foreach (var report in reports)
        {
            ImpThemeManager.Style(
                themeUpdate,
                report.transform,
                new StyleOverride("", Variant.BACKGROUND),
                new StyleOverride("Border", Variant.DARKER)
            );
        }
    }

    public void SetState(OracleState state, int cycleIndex)
    {
        reports.ForEach(Destroy);
        reports.Clear();

        title.text = $"Cycle #{cycleIndex} ({Formatting.FormatDayTime(state.Cycles[cycleIndex].CycleTime)})";

        state.IndoorCycles[cycleIndex].ForEach(entry => AddReport(entry, indoorList));
        if (outdoorList) state.OutdoorCycles[cycleIndex].ForEach(entry => AddReport(entry, outdoorList));
        if (daytimeList) state.DaytimeCycles[cycleIndex].ForEach(entry => AddReport(entry, daytimeList));
    }

    private void AddReport(SpawnReport report, Transform list)
    {
        var reportObject = Instantiate(entryTemplate, list, true);
        reportObject.SetActive(true);
        reportObject.transform.Find("Name").GetComponent<TMP_Text>().text =
            Imperium.ObjectManager.GetDisplayName(report.Entity.enemyName);
        reportObject.transform.Find("Time").GetComponent<TMP_Text>().text =
            Formatting.FormatDayTime(report.SpawnTime);
        reportObject.transform.Find("GhostSpawn").gameObject.SetActive(report.IsGhostSpawn);

        var clickableText = reportObject.transform.Find("Position").gameObject.AddComponent<ImpClickableText>();
        clickableText.Init(
            Formatting.FormatVector(report.Position, 1),
            () => Imperium.PlayerManager.TeleportLocalPlayer(report.Position)
        );

        reports.Add(reportObject);
    }
}
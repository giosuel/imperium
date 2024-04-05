#region

using System.Collections.Generic;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Oracle;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.OracleUI;

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

    public void SetState(OracleState state, int cycleIndex)
    {
        reports.ForEach(Destroy);
        reports.Clear();

        title.text = $"Cycle #{cycleIndex} ({ImpUtils.FormatDayTime(state.cycles[cycleIndex].cycleTime)})";

        state.indoorCycles[cycleIndex].ForEach(entry => AddReport(entry, indoorList));
        if (outdoorList) state.outdoorCycles[cycleIndex].ForEach(entry => AddReport(entry, outdoorList));
        if (daytimeList) state.daytimeCycles[cycleIndex].ForEach(entry => AddReport(entry, daytimeList));
    }

    private void AddReport(SpawnReport report, Transform list)
    {
        var reportObject = Instantiate(entryTemplate, list, true);
        reportObject.SetActive(true);
        reportObject.transform.Find("Name").GetComponent<TMP_Text>().text =
            Imperium.ObjectManager.GetDisplayName(report.entity.enemyName);
        reportObject.transform.Find("Time").GetComponent<TMP_Text>().text =
            ImpUtils.FormatDayTime(report.spawnTime);

        var clickableText = reportObject.transform.Find("Position").gameObject.AddComponent<ImpClickableText>();
        clickableText.Init(
            ImpUtils.FormatVector(report.position, 2),
            () => PlayerManager.TeleportTo(report.position)
        );

        reports.Add(reportObject);
    }
}
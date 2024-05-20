#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Oracle;

public record CycleInformation
{
    public float cycleTime;
    public float nextCycleTime;

    public float minSpawnTime;
    public float maxSpawnTime;
}

public record SpawnReport
{
    public EnemyType entity;
    public Vector3 position;

    public int spawnTime;
}

public record OracleState
{
    public int currentCycle;

    public readonly CycleInformation[] cycles =
    [
        new CycleInformation(), new CycleInformation(),
        new CycleInformation(), new CycleInformation(),
        new CycleInformation(), new CycleInformation(),
        new CycleInformation(), new CycleInformation(),
        new CycleInformation(), new CycleInformation()
    ];

    public readonly List<SpawnReport>[] indoorCycles = [[], [], [], [], [], [], [], [], [], []];
    public readonly List<SpawnReport>[] outdoorCycles = [[], [], [], [], [], [], [], [], [], []];
    public readonly List<SpawnReport>[] daytimeCycles = [[], [], [], [], [], [], [], [], [], []];

    internal void PrintLog()
    {
        List<string> output = [];

        for (var i = 0; i < 10; i++)
        {
            var currentCycleTime = ImpUtils.FormatTime(ImpUtils.TimeToNormalized(cycles[i].cycleTime));
            var cycleHeading = $" > CYCLE #{i} ({currentCycleTime})";
            if (i == currentCycle) cycleHeading += " (CURRENT)";
            output.Add(cycleHeading);
            var minSpawnTime = ImpUtils.FormatTime(ImpUtils.TimeToNormalized(cycles[i].minSpawnTime));
            var maxSpawnTime = ImpUtils.FormatTime(ImpUtils.TimeToNormalized(cycles[i].maxSpawnTime));
            var spawnTimeString = $"({minSpawnTime} - {maxSpawnTime})";
            output = output.Concat(BuildReportLog(indoorCycles, $"   Indoor Spawns {spawnTimeString}", i)).ToList();
            output = output.Concat(BuildReportLog(outdoorCycles, "   Outdoor Spawns", i)).ToList();
            output = output.Concat(BuildReportLog(daytimeCycles, "   Daytime Spawns", i)).ToList();
            var nextCycle = ImpUtils.FormatTime(ImpUtils.TimeToNormalized(cycles[i].nextCycleTime));
            output.Add($"   Next Cycle: {nextCycle}");
        }

        if (ImpSettings.Preferences.OracleLogging.Value) ImpOutput.LogBlock(output, title: "Oracle Prediction Report");
    }

    private static IEnumerable<string> BuildReportLog(
        IReadOnlyList<List<SpawnReport>>
            stateCycles,
        string title,
        int cycleNumber
    )
    {
        if (stateCycles[cycleNumber].Count < 1) return [title + ": -"];

        List<string> lines = [title + ":"];
        lines.AddRange(
            from report in stateCycles[cycleNumber]
            let positionString = ImpUtils.FormatVector(report.position, roundDigits: 1)
            let timeString = report.spawnTime > 0 ? ImpUtils.FormatDayTime(report.spawnTime) : "NOW"
            select $"     - {report.entity.enemyName} at {timeString} | {positionString}"
        );

        return lines;
    }
}
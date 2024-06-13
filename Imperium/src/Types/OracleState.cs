#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Types;

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
            var currentCycleTime = Formatting.FormatTime(Formatting.TimeToNormalized(cycles[i].cycleTime));
            var cycleHeading = $" > CYCLE #{i} ({currentCycleTime})";
            if (i == currentCycle) cycleHeading += " (CURRENT)";
            output.Add(cycleHeading);
            var minSpawnTime = Formatting.FormatTime(Formatting.TimeToNormalized(cycles[i].minSpawnTime));
            var maxSpawnTime = Formatting.FormatTime(Formatting.TimeToNormalized(cycles[i].maxSpawnTime));
            var spawnTimeString = $"({minSpawnTime} - {maxSpawnTime})";
            output = output.Concat(BuildReportLog(indoorCycles, $"   Indoor Spawns {spawnTimeString}", i)).ToList();
            output = output.Concat(BuildReportLog(outdoorCycles, "   Outdoor Spawns", i)).ToList();
            output = output.Concat(BuildReportLog(daytimeCycles, "   Daytime Spawns", i)).ToList();
            var nextCycle = Formatting.FormatTime(Formatting.TimeToNormalized(cycles[i].nextCycleTime));
            output.Add($"   Next Cycle: {nextCycle}");
        }

        if (Imperium.Settings.Preferences.OracleLogging.Value) Imperium.IO.LogBlock(output, title: "Oracle Prediction Report");
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
            let positionString = Formatting.FormatVector(report.position, roundDigits: 1)
            let timeString = report.spawnTime > 0 ? Formatting.FormatDayTime(report.spawnTime) : "NOW"
            select $"     - {report.entity.enemyName} at {timeString} | {positionString}"
        );

        return lines;
    }
}
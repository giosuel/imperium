#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.API.Types;

public record SpawnReport
{
    public EnemyType Entity { get; init; }
    public Vector3 Position { get; init; }

    public int SpawnTime { get; init; }
}

public record CycleInformation
{
    public float CycleTime { get; set; }
    public float NextCycleTime { get; set; }

    public float MinSpawnTime { get; set; }
    public float MaxSpawnTime { get; set; }
}

public record OracleState
{
    public int CurrentCycle { get; set; }

    public IReadOnlyList<CycleInformation> Cycles { get; } =
    [
        new CycleInformation(), new CycleInformation(),
        new CycleInformation(), new CycleInformation(),
        new CycleInformation(), new CycleInformation(),
        new CycleInformation(), new CycleInformation(),
        new CycleInformation(), new CycleInformation()
    ];

    public List<SpawnReport>[] IndoorCycles { get; } = [[], [], [], [], [], [], [], [], [], []];
    public List<SpawnReport>[] OutdoorCycles { get; } = [[], [], [], [], [], [], [], [], [], []];
    public List<SpawnReport>[] DaytimeCycles { get; } = [[], [], [], [], [], [], [], [], [], []];

    internal void PrintLog()
    {
        List<string> output = [];

        for (var i = 0; i < 10; i++)
        {
            var currentCycleTime = Formatting.FormatTime(Formatting.TimeToNormalized(Cycles[i].CycleTime));
            var cycleHeading = $" > CYCLE #{i} ({currentCycleTime})";
            if (i == CurrentCycle) cycleHeading += " (CURRENT)";
            output.Add(cycleHeading);
            var minSpawnTime = Formatting.FormatTime(Formatting.TimeToNormalized(Cycles[i].MinSpawnTime));
            var maxSpawnTime = Formatting.FormatTime(Formatting.TimeToNormalized(Cycles[i].MaxSpawnTime));
            var spawnTimeString = $"({minSpawnTime} - {maxSpawnTime})";
            output = output.Concat(BuildReportLog(IndoorCycles, $"   Indoor Spawns {spawnTimeString}", i)).ToList();
            output = output.Concat(BuildReportLog(OutdoorCycles, "   Outdoor Spawns", i)).ToList();
            output = output.Concat(BuildReportLog(DaytimeCycles, "   Daytime Spawns", i)).ToList();
            var nextCycle = Formatting.FormatTime(Formatting.TimeToNormalized(Cycles[i].NextCycleTime));
            output.Add($"   Next Cycle: {nextCycle}");
        }

        if (Imperium.Settings.Preferences.OracleLogging.Value)
            Imperium.IO.LogBlock(output, title: "Oracle Prediction Report");
    }

    private static IEnumerable<string> BuildReportLog(
        IReadOnlyList<List<SpawnReport>> stateCycles,
        string title,
        int cycleNumber
    )
    {
        if (stateCycles[cycleNumber].Count < 1) return [title + ": -"];

        List<string> lines = [title + ":"];
        lines.AddRange(
            from report in stateCycles[cycleNumber]
            let positionString = Formatting.FormatVector(report.Position, roundDigits: 1)
            let timeString = report.SpawnTime > 0 ? Formatting.FormatDayTime(report.SpawnTime) : "NOW"
            select $"     - {report.Entity.enemyName} at {timeString} | {positionString}"
        );

        return lines;
    }
}
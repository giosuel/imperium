using System.Collections.Generic;
using System.Linq;
using Imperium.Util;

namespace Imperium.Core;

/// <summary>
/// Static class used by entity spawning patches to generate the spawn reports.
/// </summary>
public static class ImpSpawnTracker
{
    private static HashSet<EnemyAI> spawnedEntitiesBeforeCycle = [];

    private static void PrintSpawnReport(IEnumerable<EnemyAI> spawnedEntities, bool initial = false)
    {
        var currentHour = Reflection.Get<RoundManager, int>(Imperium.RoundManager, "currentHour");
        var num = Imperium.TimeOfDay.lengthOfHours * currentHour / Imperium.TimeOfDay.totalTime;

        var output = spawnedEntities
            .Select(entity => $"{entity.enemyType.enemyName} at ({ImpUtils.FormatVector(entity.transform.position)})")
            .ToList();

        if (output.Count < 1)
        {
            ImpOutput.Send(
                "Nothing spawned >.<", $"Spawn Report Cycle #{num * 9}",
                notificationType: NotificationType.SpawnReport
            );
            ImpUtils.LogBlock(["Nothing Spawned"], title: "Spawn Tracker");
        }
        else
        {
            ImpOutput.Send(
                output.Aggregate((a, b) => $"{a}\n{b}"), $"Spawn Report Cycle #{num * 9}",
                notificationType: NotificationType.SpawnReport
            );
            ImpUtils.LogBlock(output, title: "Spawn Tracker");
        }
    }

    internal static void StartCycle(RoundManager roundManager)
    {
        spawnedEntitiesBeforeCycle = roundManager.SpawnedEnemies.ToHashSet();
    }

    internal static void EndCycle(RoundManager roundManager)
    {
        PrintSpawnReport(spawnedEntitiesBeforeCycle.Except(roundManager.SpawnedEnemies.ToHashSet()));
    }
}
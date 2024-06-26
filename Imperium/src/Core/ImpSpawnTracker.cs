#region

using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Util;

#endregion

namespace Imperium.Core;

/// <summary>
///     Static class used by entity spawning patches to generate the spawn reports.
/// </summary>
public static class ImpSpawnTracker
{
    private static HashSet<EnemyAI> spawnedEntitiesBeforeCycle = [];

    private static void PrintSpawnReport(IEnumerable<EnemyAI> spawnedEntities, bool initial = false)
    {
        var currentHour = Reflection.Get<RoundManager, int>(Imperium.RoundManager, "currentHour");
        var num = Imperium.TimeOfDay.lengthOfHours * currentHour / Imperium.TimeOfDay.totalTime;

        var output = spawnedEntities
            .Select(entity => $"{entity.enemyType.enemyName} at ({Formatting.FormatVector(entity.transform.position)})")
            .ToList();

        if (output.Count < 1)
        {
            Imperium.IO.Send(
                "Nothing spawned >.<", $"Spawn Report Cycle #{num * 9}",
                type: NotificationType.SpawnReport
            );
            Imperium.IO.LogBlock(["Nothing Spawned"], title: "Spawn Tracker");
        }
        else
        {
            Imperium.IO.Send(
                output.Aggregate((a, b) => $"{a}\n{b}"), $"Spawn Report Cycle #{num * 9}",
                type: NotificationType.SpawnReport
            );
            Imperium.IO.LogBlock(output, title: "Spawn Tracker");
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
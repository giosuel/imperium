#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Core;

/// <summary>
///     Static class used by entity spawning patches to generate the spawn reports.
/// </summary>
public static class ImpSpawnTracker
{
    private static HashSet<EnemyAI> spawnedEntitiesBeforeCycle = [];
    private static readonly HashSet<Tuple<EnemyType, Vector3>> ventEntities = [];

    private static void PrintSpawnReport(bool initial = false)
    {
        var currentHour = Reflection.Get<RoundManager, int>(Imperium.RoundManager, "currentHour");
        var num = Imperium.TimeOfDay.lengthOfHours * currentHour / Imperium.TimeOfDay.totalTime;

        var output = GetSpawnedEntitiesThisCycle();

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

    internal static List<string> GetSpawnedEntitiesThisCycle()
    {
        return Imperium.RoundManager.SpawnedEnemies.ToHashSet().Except(spawnedEntitiesBeforeCycle)
            .Select(entity => $"{entity.enemyType.enemyName} at {Formatting.FormatVector(entity.transform.position, 0)}")
            .Concat(ventEntities.Select(entry => $"{entry.Item1.enemyName} at {Formatting.FormatVector(entry.Item2, 0)}"))
            .ToList();
    }

    internal static void StartCycle(RoundManager roundManager)
    {
        spawnedEntitiesBeforeCycle = roundManager.SpawnedEnemies.ToHashSet();
        ventEntities.Clear();
    }

    internal static void AddVentEntity(EnemyType entityType, Vector3 position)
    {
        ventEntities.Add(new Tuple<EnemyType, Vector3>(entityType, position));
    }

    internal static void EndCycle(RoundManager roundManager)
    {
        PrintSpawnReport();
    }
}
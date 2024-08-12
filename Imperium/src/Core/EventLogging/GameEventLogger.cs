using Imperium.Util;

namespace Imperium.Core.EventLogging;

internal class GameEventLogger(ImpEventLog log)
{
    private void LogGameEvent(string message, string action = null, params EventLogDetail[] details)
    {
        log.AddLog(new EventLogMessage
        {
            ObjectName = "SYSTEM",
            Message = message,
            DetailsTitle = action,
            Details = details,
            Type = EventLogType.Game
        });
    }

    internal void AdvanceHourAndSpawnNewBatchOfEnemiesPrefix(bool isInitial)
    {
        LogGameEvent(
            isInitial ? "Executing initial spawn cycle." : "Executing spawn cycle.",
            "Entity Spawning",
            new EventLogDetail
            {
                Title = "Indoor Power Level",
                Text =
                    $"{Imperium.RoundManager.currentEnemyPower:0.0}/{Imperium.RoundManager.currentMaxInsidePower}"
            },
            new EventLogDetail
            {
                Title = "Outdoor Power Level",
                Text =
                    $"{Imperium.RoundManager.currentOutsideEnemyPower:0.0}/{Imperium.RoundManager.currentMaxOutsidePower}"
            },
            new EventLogDetail
            {
                Title = "Outdoor Power Level",
                Text =
                    $"{Imperium.RoundManager.currentDaytimeEnemyPower:0.0}/{Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount}"
            },
            new EventLogDetail
            {
                Title = "Current Time",
                Text = $"{Formatting.FormatDayTime(Imperium.TimeOfDay.currentDayTime)}"
            },
            new EventLogDetail
            {
                Title = "Cycle Hour",
                Text = $"{Imperium.RoundManager.currentHour}"
            }
        );
    }

    internal void AdvanceHourAndSpawnNewBatchOfEnemiesPostfix(bool isInitial)
    {
        var spawnedEntities = ImpSpawnTracker.GetSpawnedEntitiesThisCycle();
        
        LogGameEvent(
            isInitial ? "Finished executing initial spawn cycle." : "Finished executing spawn cycle.",
            "Entity Spawning",
            new EventLogDetail
            {
                Title = "Indoor Power Level",
                Text =
                    $"{Imperium.RoundManager.currentEnemyPower:0.0}/{Imperium.RoundManager.currentMaxInsidePower}"
            },
            new EventLogDetail
            {
                Title = "Outdoor Power Level",
                Text =
                    $"{Imperium.RoundManager.currentOutsideEnemyPower:0.0}/{Imperium.RoundManager.currentMaxOutsidePower}"
            },
            new EventLogDetail
            {
                Title = "Outdoor Power Level",
                Text =
                    $"{Imperium.RoundManager.currentDaytimeEnemyPower:0.0}/{Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount}"
            },
            new EventLogDetail
            {
                Title = "Current Time",
                Text = $"{Formatting.FormatDayTime(Imperium.TimeOfDay.currentDayTime)}"
            },
            new EventLogDetail
            {
                Title = "Cycle Hour",
                Text = $"{Imperium.RoundManager.currentHour}"
            },
            new EventLogDetail
            {
                Title = "Spawned Entities",
                Text = spawnedEntities.Count > 0 ? "\n" + string.Join("\n", spawnedEntities) : "-"
            }
        );
    }
}
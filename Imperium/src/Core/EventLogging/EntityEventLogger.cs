#region

using System.Linq;
using GameNetcodeStuff;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Core.EventLogging;

internal class EntityEventLogger(ImpEventLog log)
{
    private void LogEntityEvent(EnemyAI instance, string message, string action = null, params EventLogDetail[] details)
    {
        var entityPersonalName = $"({Imperium.ObjectManager.GetEntityName(instance)})";

        log.AddLog(new EventLogMessage
        {
            ObjectName = $"{instance.enemyType.enemyName} {RichText.Size(entityPersonalName, 10)}",
            Message = message,
            DetailsTitle = action,
            Details = details,
            Type = EventLogType.Entity
        });
    }

    internal void Start(EnemyAI instance)
    {
        LogEntityEvent(instance, "Started existing.", action: "Start");
    }

    internal void SwitchBehaviourState(EnemyAI instance, int oldState, int newState)
    {
        var oldStateName = instance.enemyBehaviourStates[oldState].name;
        var oldStateString = string.IsNullOrEmpty(oldStateName)
            ? oldState.ToString()
            : $"{oldState}/{oldStateName}";

        var newStateName = instance.enemyBehaviourStates[newState].name;
        var newStateString = string.IsNullOrEmpty(newStateName)
            ? newState.ToString()
            : $"{newState}/{newStateName}";

        if (string.IsNullOrEmpty(newStateString)) newStateString = "unknown";

        LogEntityEvent(
            instance, $"Switched state ({oldStateString} -> {newStateString}).",
            action: "Switch Behaviour State",
            new EventLogDetail
            {
                Title = "Function",
                Text = "EnemyAI.SwitchToBehaviourState"
            },
            new EventLogDetail
            {
                Title = "Previous State",
                Text = oldStateString
            },
            new EventLogDetail
            {
                Title = "Current State",
                Text = newStateString
            }
        );
    }

    internal void TargetClosestPlayer(EnemyAI instance)
    {
        var closestPlayerName = instance.targetPlayer.playerUsername ?? instance.targetPlayer.playerClientId.ToString();
        LogEntityEvent(instance,
            $"Targeted closest player '{closestPlayerName}' at {instance.mostOptimalDistance:0.0}u away.",
            action: "Targeting",
            new EventLogDetail
            {
                Title = "Function",
                Text = "EnemyAI.TargetClosestPlayer"
            },
            new EventLogDetail
            {
                Title = "Player",
                Text = $"{closestPlayerName} (ID: {instance.targetPlayer.playerClientId})"
            },
            new EventLogDetail
            {
                Title = "Position",
                Text = Formatting.FormatVector(instance.targetPlayer.transform.position)
            }
        );
    }

    internal void GetAllPlayersInLineOfSight(EnemyAI instance, PlayerControllerB[] players)
    {
        var playersString = string.Join(", ", players.Select(player => player.playerUsername).ToList());
        LogEntityEvent(
            instance, $"Spotted players ({playersString}).",
            action: "Line Of Sight",
            new EventLogDetail
            {
                Title = "Function",
                Text = "EnemyAI.GetAllPlayersInLineOfSight"
            },
            new EventLogDetail
            {
                Title = "Players",
                Text = playersString
            }
        );
    }

    internal void CheckLineOfSightForPlayer(EnemyAI instance, PlayerControllerB player)
    {
        var playerName = player.playerUsername ?? player.playerClientId.ToString();
        LogEntityEvent(
            instance, $"Spotted player '{playerName}'.",
            action: "Line Of Sight",
            new EventLogDetail
            {
                Title = "Function",
                Text = "EnemyAI.CheckLineOfSightForPlayer"
            },
            new EventLogDetail
            {
                Title = "Player",
                Text = playerName
            }
        );
    }

    internal void CheckLineOfSightForClosestPlayer(EnemyAI instance, PlayerControllerB player)
    {
        var playerName = player.playerUsername ?? player.playerClientId.ToString();
        LogEntityEvent(
            instance, $"Spotted closest player '{playerName}' at {instance.mostOptimalDistance:0.0}u away.",
            action: "Line Of Sight",
            new EventLogDetail
            {
                Title = "Function",
                Text = "EnemyAI.CheckLineOfSightForClosestPlayer"
            },
            new EventLogDetail
            {
                Title = "Player",
                Text = playerName
            }
        );
    }

    internal void CheckLineOfSightForPosition(EnemyAI instance, Vector3 position)
    {
        LogEntityEvent(
            instance, $"Looked at position {Formatting.FormatVector(position, 1)}.",
            action: "Line Of Sight",
            new EventLogDetail
            {
                Title = "Function",
                Text = "EnemyAI.CheckLineOfSightForPosition"
            },
            new EventLogDetail
            {
                Title = "Position",
                Text = Formatting.FormatVector(position, 1)
            }
        );
    }

    internal void DetectNoise(EnemyAI instance, Vector3 noisePosition, int noiseID, int timesHeard, float noiseLoudness)
    {
        LogEntityEvent(instance,
            $"Detected noise at position {Formatting.FormatVector(noisePosition, 1)}.",
            action: "Noise Detection",
            new EventLogDetail
            {
                Title = "Function",
                Text = "IHittable.DetectNoise"
            },
            new EventLogDetail
            {
                Title = "Position",
                Text = Formatting.FormatVector(noisePosition, 1)
            },
            new EventLogDetail
            {
                Title = "Loudness",
                Text = $"{noiseLoudness:0.0}"
            },
            new EventLogDetail
            {
                Title = "Times Heard",
                Text = timesHeard.ToString()
            },
            new EventLogDetail
            {
                Title = "Noise ID",
                Text = noiseID.ToString()
            }
        );
    }

    internal void DetectHit(EnemyAI instance, Vector3 hitDirection, PlayerControllerB player, int force)
    {
        LogEntityEvent(
            instance, $"Was struck with force {force}.",
            action: "Hit Detection",
            new EventLogDetail
            {
                Title = "Function",
                Text = "IHittable.Hit"
            },
            new EventLogDetail
            {
                Title = "Force",
                Text = force.ToString()
            },
            new EventLogDetail
            {
                Title = "Direction",
                Text = Formatting.FormatVector(hitDirection, 1)
            },
            new EventLogDetail
            {
                Title = "Player",
                Text = player?.playerUsername ?? player?.playerClientId.ToString() ?? "-"
            }
        );
    }
}
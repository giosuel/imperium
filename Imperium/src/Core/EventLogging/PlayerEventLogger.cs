#region

using GameNetcodeStuff;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Core.EventLogging;

internal class PlayerEventLogger(ImpEventLog log)
{
    private void LogPlayerEvent(
        PlayerControllerB instance, string message, string action = null,
        params EventLogDetail[] details
    )
    {
        var playerName = instance.playerUsername ?? "unknown";

        log.AddLog(new EventLogMessage
        {
            ObjectName = $"{playerName} ({instance.playerClientId})",
            Message = message,
            DetailsTitle = action,
            Details = details,
            Type = EventLogType.Player
        });
    }

    internal void DamagePlayer(
        PlayerControllerB instance,
        int damageNumber,
        CauseOfDeath causeOfDeath,
        bool isFallDamage,
        Vector3 impactForce
    )
    {
        var message = Imperium.Settings.Player.GodMode.Value
            ? $"Took {damageNumber} damage from {causeOfDeath.ToString()} (negated)."
            : $"Took {damageNumber} damage from {causeOfDeath.ToString()}.";

        LogPlayerEvent(
            instance, message,
            action: "Player Damage",
            new EventLogDetail
            {
                Title = "Function",
                Text = "PlayerControllerB.DamagePlayer"
            },
            new EventLogDetail
            {
                Title = "Damage",
                Text = damageNumber.ToString()
            },
            new EventLogDetail
            {
                Title = "Cause Of Death",
                Text = causeOfDeath.ToString()
            },
            new EventLogDetail
            {
                Title = "Fall Damage",
                Text = isFallDamage.ToString()
            },
            new EventLogDetail
            {
                Title = "Impact Force",
                Text = Formatting.FormatVector(impactForce, 1)
            },
            new EventLogDetail
            {
                Title = "Negated by God Mode",
                Text = Imperium.Settings.Player.GodMode.Value.ToString()
            }
        );
    }

    internal void KillPlayer(
        PlayerControllerB instance,
        CauseOfDeath causeOfDeath,
        bool spawnBody,
        Vector3 bodyVelocity,
        Vector3 positionOffset
    )
    {
        var message = Imperium.Settings.Player.GodMode.Value
            ? $"Died to {causeOfDeath.ToString()} (negated)."
            : $"Died to {causeOfDeath.ToString()}.";

        LogPlayerEvent(
            instance, message,
            action: "Player Death",
            new EventLogDetail
            {
                Title = "Function",
                Text = "PlayerControllerB.KillPlayer"
            },
            new EventLogDetail
            {
                Title = "Cause Of Death",
                Text = causeOfDeath.ToString()
            },
            new EventLogDetail
            {
                Title = "Spawn Body",
                Text = spawnBody.ToString()
            },
            new EventLogDetail
            {
                Title = "Body Velocity",
                Text = Formatting.FormatVector(bodyVelocity, 1)
            },
            new EventLogDetail
            {
                Title = "Body Offset",
                Text = Formatting.FormatVector(positionOffset, 1)
            },
            new EventLogDetail
            {
                Title = "Negated by God Mode",
                Text = Imperium.Settings.Player.GodMode.Value.ToString()
            }
        );
    }
}
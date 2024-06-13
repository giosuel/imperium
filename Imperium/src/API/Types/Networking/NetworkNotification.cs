// ReSharper disable Unity.RedundantAttributeOnTarget

#region

using UnityEngine;

#endregion

namespace Imperium.API.Types.Networking;

public readonly struct NetworkNotification()
{
    [SerializeField] public string Message { get; init; }
    [SerializeField] public string Title { get; init; } = null;
    [SerializeField] public bool IsWarning { get; init; } = false;
    [SerializeField] public NotificationType Type { get; init; } = NotificationType.Server;
}

public enum NotificationType
{
    // God mode notifications on taking damage and dying.
    GodMode,

    // Oracle spawn prediction updates.
    OracleUpdate,

    // Confirmation dialogs following user interaction.
    Confirmation,

    // Spawn report every cycle.
    SpawnReport,

    // Entity related notifications (e.g. Entity took damage).
    Entities,

    // Imperium item / entity spawning.
    Spawning,

    // Access control / lobby control related things.
    AccessControl,

    // Any notifications coming from the host.
    Server,

    // Required notifications for important user-triggered events (can't be turned off).
    Required,

    Other
}
// ReSharper disable Unity.RedundantAttributeOnTarget

#region

using JetBrains.Annotations;
using UnityEngine;

#endregion

namespace Imperium.API.Types.Networking;

public readonly struct EntitySpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public string PrefabName { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public int Amount { get; init; } = 1;
    [SerializeField] public int Health { get; init; } = -1;
    [SerializeField] public bool SendNotification { get; init; } = false;

    /*
     * Masked specific parameters
     */
    [SerializeField] public long MaskedPlayerId { get; init; } = -1;
    [SerializeField] public string MaskedName { get; init; } = null;
}

public readonly struct ItemSpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public string PrefabName { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public int Amount { get; init; } = 1;
    [SerializeField] public int Value { get; init; } = -1;
    [SerializeField] public bool SpawnInInventory { get; init; } = true;
    [SerializeField] public bool SendNotification { get; init; } = false;
}

public readonly struct MapHazardSpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public int Amount { get; init; } = 1;
    [SerializeField] public bool SendNotification { get; init; } = false;
}

public readonly struct StaticPrefabSpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public int Amount { get; init; } = 1;
    [SerializeField] public bool SendNotification { get; init; } = false;
    [SerializeField] public ulong UniqueIdentifier { get; init; }
}

public readonly struct CompanyCruiserSpawnRequest()
{
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public bool SendNotification { get; init; } = false;
}

public readonly struct ObjectTeleportRequest()
{
    // This can be either the network ID or the imperium unique identifier assigned when spawning.
    [SerializeField] public ulong NetworkId { get; init; } = default;
    [SerializeField] public Vector3 Destination { get; init; }
}
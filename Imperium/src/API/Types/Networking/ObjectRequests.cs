// ReSharper disable Unity.RedundantAttributeOnTarget

#region

using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.API.Types.Networking;

public readonly struct EntitySpawnRequest()
{
    [SerializeField] public string Name { get; init; }
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

public readonly struct EntityDespawnRequest
{
    [SerializeField] public ulong NetId { get; init; }
    [SerializeField] public bool IsRespawn { get; init; }
}

public readonly struct ItemSpawnRequest()
{
    [SerializeField] public string Name { get; init; }
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
}

public readonly struct VehicleSpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public bool SendNotification { get; init; } = false;
}

public readonly struct VehicleSpawnResponse
{
    [SerializeField] public NetworkObjectReference NetObj { get; init; }
}

public readonly struct VehicleDespawnRequest
{
    [SerializeField] public ulong NetId { get; init; }
    [SerializeField] public bool IsRespawn { get; init; }
}

public readonly struct ObjectTeleportRequest()
{
    // This can be either the network ID or the imperium unique identifier assigned when spawning.
    [SerializeField] public ulong NetworkId { get; init; } = 0;
    [SerializeField] public Vector3 Destination { get; init; }
}

public readonly struct LocalObjectDespawnRequest
{
    [SerializeField] public LocalObjectType Type { get; init; }
    [SerializeField] public Vector3 Position { get; init; }
}

public readonly struct LocalObjectTeleportRequest
{
    [SerializeField] public LocalObjectType Type { get; init; }
    [SerializeField] public Vector3 Position { get; init; }
    [SerializeField] public Vector3 Destination { get; init; }
}

public readonly struct VentToggleRequest
{
    [SerializeField] public ulong NetworkId { get; init; }
    [SerializeField] public bool IsEnabled { get; init; }
}

public readonly struct BurstCadaverBloomRequest
{
    [SerializeField] public ulong PlayerId { get; init; }
    [SerializeField] public Vector3 Position { get; init; }
    [SerializeField] public NetworkObjectReference NetObj { get; init; }
}

public enum LocalObjectType
{
    OutsideObject,
    VainShroud
}

public enum ObjectType
{
    BreakerBox,
    Vehicle,
    Entity,
    Item,
    Landmine,
    Player,
    SpiderWeb,
    SpikeTrap,
    SteamValve,
    SecurityDoor,
    OutsideObject,
    VainShroud,
    StoryLog,
    Turret,
    Vent
}
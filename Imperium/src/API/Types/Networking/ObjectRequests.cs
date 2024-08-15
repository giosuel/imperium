// ReSharper disable Unity.RedundantAttributeOnTarget

#region

using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;
using JetBrains.Annotations;
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

public enum LocalObjectType
{
    VainShroud,
    OutsideObject
}

public enum ObjectType
{
    BreakerBox,
    Cruiser,
    Entity,
    Item,
    Landmine,
    VainShroud,
    Player,
    SpiderWeb,
    SpikeTrap,
    SteamValve,
    OutsideObject,
    Turret,
    Vent
}
// ReSharper disable Unity.RedundantAttributeOnTarget
using System;
using UnityEngine;

namespace Imperium.API.Types.Networking;

public readonly struct EntitySpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public string PrefabName { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public int Amount { get; init; } = 1;
    [SerializeField] public int Health { get; init; } = -1;
    [SerializeField] public bool SendNotification { get; init; } = false;
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
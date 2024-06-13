// ReSharper disable Unity.RedundantAttributeOnTarget

#region

using UnityEngine;

#endregion

namespace Imperium.API.Types.Networking;

public readonly struct TeleportPlayerRequest
{
    [SerializeField] public ulong PlayerId { get; init; }
    [SerializeField] public Vector3 Destination { get; init; }
}

public readonly struct DropItemRequest
{
    [SerializeField] public ulong PlayerId { get; init; }
    [SerializeField] public int ItemIndex { get; init; }
}
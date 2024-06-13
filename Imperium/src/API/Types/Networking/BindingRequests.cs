// ReSharper disable Unity.RedundantAttributeOnTarget

#region

using UnityEngine;

#endregion

namespace Imperium.API.Types.Networking;

public readonly struct BindingUpdateRequest<T>
{
    [SerializeField] public T Payload { get; init; }
    [SerializeField] public bool InvokeUpdate { get; init; }
    [SerializeField] public bool InvokeServerUpdate { get; init; }
}
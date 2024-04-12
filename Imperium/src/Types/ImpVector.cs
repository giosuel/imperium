#region

using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Types;

/// <summary>
///     Simple UnityEngine.Vector3 wrapper to make it serializable for utilization in server and client RPCs
/// </summary>
public class ImpVector : INetworkSerializable
{
    private Vector3 vector3;

    public ImpVector()
    {
    }

    public ImpVector(Vector3 vector3)
    {
        this.vector3 = vector3;
    }

    public Vector3 Vector3()
    {
        return vector3;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref vector3);
    }
}
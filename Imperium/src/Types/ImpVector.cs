#region

using System;
using UnityEngine;

#endregion

namespace Imperium.Types;

/// <summary>
/// Simple UnityEngine.Vector3 wrapper to make it serializable for utilization in server and client RPCs
/// </summary>
/// <param name="vector3"></param>
[Serializable]
public class ImpVector(Vector3 vector3)
{
    public float x = vector3.x, y = vector3.y, z = vector3.z;

    public Vector3 Vector3()
    {
        return new Vector3(x, y, z);
    }
}
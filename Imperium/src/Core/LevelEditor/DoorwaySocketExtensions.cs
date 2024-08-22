#region

using DunGen;
using UnityEngine;

#endregion

namespace Imperium.Core.LevelEditor;

internal static class DoorwaySocketExtensions
{
    internal static bool IsCompatible(this DoorwaySocket socket, DoorwaySocket other)
    {
        return socket == other || Mathf.Abs(socket.Size.magnitude - other.Size.magnitude) < 1f;
    }
}
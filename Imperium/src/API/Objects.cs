using Imperium.Core.Lifecycle;
using UnityEngine;

namespace Imperium.API;

public static class Objects
{
    /// <summary>
    /// Teleports an item to a specified location.
    /// </summary>
    public static void TeleportItem(GrabbableObject item, Vector3 position)
    {
        APIHelpers.AssertImperiumReady();

        ObjectManager.TeleportItem(item, position);
    }
}
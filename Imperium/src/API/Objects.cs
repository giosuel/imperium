#region

using Imperium.API.Types.Networking;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.API;

public static class Objects
{
    /// <summary>
    ///     Teleports an item to a specified location.
    /// </summary>
    public static void TeleportItem(GrabbableObject item, Vector3 position)
    {
        APIHelpers.AssertImperiumReady();

        Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
        {
            NetworkObj = item.GetComponent<NetworkObject>(),
            Destination = position
        });
    }

    /// <summary>
    ///     Teleports an entity to a specified location.
    /// </summary>
    public static void TeleportEntity(EnemyAI entity, Vector3 position)
    {
        APIHelpers.AssertImperiumReady();

        Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
        {
            NetworkObj = entity.GetComponent<NetworkObject>(),
            Destination = position
        });
    }
}
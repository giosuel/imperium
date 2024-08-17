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
            NetworkId = item.GetComponent<NetworkObject>().NetworkObjectId,
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
            NetworkId = entity.GetComponent<NetworkObject>().NetworkObjectId,
            Destination = position
        });
    }
}
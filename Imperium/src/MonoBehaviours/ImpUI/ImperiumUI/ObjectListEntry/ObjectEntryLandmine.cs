#region

using Imperium.Core;
using Imperium.Netcode;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryLandmine : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;

    protected override bool CanTeleportHere() => true;

    protected override void Respawn()
    {
        Destroy();
        ObjectManager.SpawnMapHazard("Landmine", containerObject.transform.position);
    }

    public override void Destroy()
    {
        base.Destroy();
        ImpNetSpawning.Instance.DespawnMapHazardServerRpc(
            containerObject.GetComponent<NetworkObject>().NetworkObjectId
        );
    }

    protected override void TeleportHere()
    {
        Imperium.PositionIndicator.Activate(position => GetContainerObject().transform.position = position);
    }

    protected override string GetObjectName() => $"Landmine <i>{component.GetInstanceID()}</i>";
    protected override GameObject GetContainerObject() => component.transform.parent.gameObject;
}
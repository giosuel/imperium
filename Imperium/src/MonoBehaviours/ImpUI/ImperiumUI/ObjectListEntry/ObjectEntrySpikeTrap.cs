#region

using Imperium.Core;
using Imperium.Netcode;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntrySpikeTrap : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;
    protected override bool CanTeleportHere() => true;

    protected override void Respawn()
    {
        Destroy();
        ObjectManager.SpawnMapHazard("Spike Trap", containerObject.transform.position);
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
        Imperium.ImpPositionIndicator.Activate(position => GetContainerObject().transform.position = position);
    }

    protected override string GetObjectName() => $"Spike Trap <i>{component.GetInstanceID()}</i>";
    protected override GameObject GetContainerObject() => component.transform.parent.parent.parent.gameObject;
}
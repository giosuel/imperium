#region

using Imperium.Core;
using Imperium.Netcode;
using Unity.Netcode;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryEntity : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;
    protected override bool CanTeleportHere() => true;

    protected override void Respawn()
    {
        base.Destroy();
        var spawnPosition = containerObject.transform.position;
        ImpNetSpawning.Instance.DespawnEntityServerRpc(
            containerObject.GetComponent<NetworkObject>().NetworkObjectId);
        ObjectManager.SpawnEntity(objectName, spawnPosition);
    }

    public override void Destroy()
    {
        base.Destroy();
        ImpNetSpawning.Instance.DespawnEntityServerRpc(
            containerObject.GetComponent<NetworkObject>().NetworkObjectId);
    }

    protected override void TeleportHere()
    {
        Imperium.PositionIndicator.Activate(position => component.transform.position = position);
    }

    protected override string GetObjectName() => ((EnemyAI)component).enemyType.enemyName;
}
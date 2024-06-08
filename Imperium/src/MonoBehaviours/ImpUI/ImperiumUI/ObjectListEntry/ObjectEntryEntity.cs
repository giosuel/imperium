#region

using Imperium.Core;
using Imperium.Netcode;
using Imperium.Util;
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
            containerObject.GetComponent<NetworkObject>().NetworkObjectId
        );
        ObjectManager.SpawnEntity(
            objectName,
            ((EnemyAI)component).enemyType.enemyPrefab.name,
            spawnPosition,
            PlayerManager.LocalPlayerId
        );
    }

    public override void Destroy()
    {
        base.Destroy();
        ImpNetSpawning.Instance.DespawnEntityServerRpc(
            containerObject.GetComponent<NetworkObject>().NetworkObjectId
        );
    }

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position => component.transform.position = position, origin);
    }

    protected override string GetObjectName()
    {
        var entity = (EnemyAI)component;
        var entityName = entity.enemyType.enemyName;
        return entity.isEnemyDead ? RichText.Strikethrough(entityName) : entityName;
    }
}
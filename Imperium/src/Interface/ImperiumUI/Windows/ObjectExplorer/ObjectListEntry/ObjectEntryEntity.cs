#region

using Imperium.API.Types.Networking;
using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;
using Imperium.Util;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryEntity : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;
    protected override bool CanTeleportHere() => true;

    protected override void Respawn()
    {
        Destroy();

        Imperium.ObjectManager.SpawnEntity(new EntitySpawnRequest
        {
            Name = objectName,
            PrefabName = ((EnemyAI)component).enemyType.enemyPrefab.name,
            SpawnPosition = containerObject.transform.position
        });
    }

    public override void Destroy()
    {
        base.Destroy();
        Imperium.ObjectManager.DespawnEntity(objectNetId!.Value);
    }

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position => component.transform.position = position, origin);
    }

    protected override void ToggleObject(bool isActive)
    {
        var entity = (EnemyAI)component;
        entity.enabled = isActive;
        entity.agent.isStopped = !isActive;
        if (entity.creatureAnimator) entity.creatureAnimator.enabled = isActive;
    }

    protected override string GetObjectName()
    {
        var entity = (EnemyAI)component;
        var entityName = entity.enemyType.enemyName;
        return entity.isEnemyDead ? RichText.Strikethrough(entityName) : entityName;
    }
}
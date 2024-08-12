#region

using Imperium.API.Types.Networking;
using Imperium.Util;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

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
        Imperium.ImpPositionIndicator.Activate(position =>
        {
            Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
            {
                Destination = position,
                NetworkId = objectNetId!.Value
            });
        }, origin);
    }

    protected override void ToggleObject(bool isActive)
    {
        if (!component) return;

        var entity = (EnemyAI)component;
        entity.enabled = isActive;
        entity.agent.isStopped = !isActive;
        if (entity.creatureAnimator) entity.creatureAnimator.enabled = isActive;
    }

    protected override string GetObjectName()
    {
        var entity = (EnemyAI)component;
        var personalName = $"({Imperium.ObjectManager.GetEntityName(entity)})";
        var entityName = $"{entity.enemyType.enemyName} {RichText.Size(personalName, 10)}";
        return entity.isEnemyDead ? RichText.Strikethrough(entityName) : entityName;
    }
}
#region

using Imperium.API.Types.Networking;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntrySpiderWeb : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;
    protected override bool CanTeleportHere() => true;

    protected override void Respawn()
    {
        Destroy();
        Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
        {
            Name = "SpiderWeb",
            SpawnPosition = containerObject.transform.position
        });
    }

    public override void Destroy()
    {
        Imperium.ObjectManager.DespawnObstacle(objectNetId!.Value);
        base.Destroy();
    }

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position => GetContainerObject().transform.position = position, origin);
    }

    protected override string GetObjectName() => $"Spider Web <i>{component.GetInstanceID()}</i>";
}
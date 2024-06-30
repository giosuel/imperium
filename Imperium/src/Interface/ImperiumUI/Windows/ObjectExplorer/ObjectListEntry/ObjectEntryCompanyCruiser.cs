#region

using Imperium.API.Types.Networking;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectCompanyCruiser : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanTeleportHere() => true;
    protected override bool CanToggle() => false;

    protected override void Respawn()
    {
        Destroy();

        Imperium.ObjectManager.SpawmCompanyCruiser(new CompanyCruiserSpawnRequest
        {
            SpawnPosition = containerObject.transform.position
        });
    }

    public override void Destroy()
    {
        base.Destroy();
        Imperium.ObjectManager.DespawnObstacle(objectNetId!.Value);
    }

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position =>
        {
            GetContainerObject().transform.position = position + Vector3.up * 5f;
        }, origin);
    }

    protected override string GetObjectName() => $"Cruiser <i>{component.GetInstanceID()}</i>";
}
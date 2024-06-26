#region

using Imperium.API.Types.Networking;
using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;
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

        Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
        {
            Name = "Landmine",
            SpawnPosition = containerObject.transform.position
        });
    }

    public override void Destroy()
    {
        base.Destroy();
        Imperium.ObjectManager.DespawnObstacle(objectNetId!.Value);
    }

    protected override void ToggleObject(bool isActive) => ((Landmine)component).ToggleMine(isActive);

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position => GetContainerObject().transform.position = position, origin);
    }

    protected override string GetObjectName() => $"Landmine <i>{component.GetInstanceID()}</i>";
    protected override GameObject GetContainerObject() => component.transform.parent.gameObject;
}
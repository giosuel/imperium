#region

using Imperium.API.Types.Networking;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntryTurret : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;
    protected override bool CanTeleportHere() => true;

    protected override void Respawn()
    {
        Destroy();
        Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
        {
            Name = "Turret",
            SpawnPosition = containerObject.transform.position
        });
    }

    protected override void ToggleObject(bool isActive) => ((Turret)component).ToggleTurretEnabled(isActive);

    public override void Destroy()
    {
        base.Destroy();
        Imperium.ObjectManager.DespawnEntity(containerObject.GetComponent<NetworkObject>().NetworkObjectId);
    }

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position => GetContainerObject().transform.position = position, origin);
    }

    protected override string GetObjectName() => $"Turret <i>{component.GetInstanceID()}</i>";
    protected override GameObject GetContainerObject() => component.transform.parent.gameObject;
}
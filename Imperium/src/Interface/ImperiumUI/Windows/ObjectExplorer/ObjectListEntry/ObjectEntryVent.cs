#region

using Imperium.API.Types.Networking;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntryVent : ObjectEntry
{
    protected override bool CanRespawn() => false;
    protected override bool CanDrop() => false;
    protected override bool CanDestroy() => true;
    protected override bool CanTeleportHere() => true;

    protected override string GetObjectName()
    {
        var vent = (EnemyVent)component;
        if (vent.occupied && vent.enemyType)
        {
            return $"Vent <i>{component.GetInstanceID()}</i> ({((EnemyVent)component).enemyType.enemyName})";
        }

        return $"Vent <i>{component.GetInstanceID()}</i>";
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
            Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
            {
                Destination = position,
                NetworkId = objectNetId!.Value
            });
        }, origin, castGround: false);
    }

    protected override Vector3 GetTeleportPosition() => ((EnemyVent)component).floorNode.position;
}
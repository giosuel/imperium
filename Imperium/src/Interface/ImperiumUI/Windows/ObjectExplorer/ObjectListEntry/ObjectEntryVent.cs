#region

using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryVent : ObjectEntry
{
    protected override bool CanRespawn() => false;
    protected override bool CanDrop() => false;
    protected override bool CanDestroy() => false;
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

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position => GetContainerObject().transform.position = position, origin);
    }

    protected override Vector3 GetTeleportPosition() => ((EnemyVent)component).floorNode.position;
}
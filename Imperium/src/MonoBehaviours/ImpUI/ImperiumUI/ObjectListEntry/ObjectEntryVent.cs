#region

using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryVent : ObjectEntry
{
    protected override bool CanRespawn() => false;
    protected override bool CanDrop() => false;

    protected override string GetObjectName()
    {
        return $"{((EnemyVent)component).enemyType.enemyName} (Vent <i>{component.GetInstanceID()}</i>)";
    }

    public override void Destroy()
    {
        base.Destroy();
        Imperium.Log.LogInfo("Despawning vent on server");
    }

    protected override Vector3 GetTeleportPosition() => ((EnemyVent)component).floorNode.position;
}
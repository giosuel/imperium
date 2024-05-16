#region

using Imperium.Netcode;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryBreakerBox : ObjectEntry
{
    protected override bool CanRespawn() => false;
    protected override bool CanDrop() => false;
    protected override bool CanDestroy() => false;
    protected override bool CanTeleportHere() => true;

    public override void Destroy()
    {
        base.Destroy();
        ImpNetSpawning.Instance.OnMapHazardsChangedClientRpc();
    }

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position => GetContainerObject().transform.position = position, origin);
    }

    protected override string GetObjectName() => $"Breaker Box <i>{component.GetInstanceID()}</i>";
}
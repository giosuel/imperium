#region

using Imperium.Netcode;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryBreakerBox : ObjectEntry
{
    protected override bool CanRespawn() => false;
    protected override bool CanDrop() => false;

    public override void Destroy()
    {
        base.Destroy();
        ImpNetSpawning.Instance.OnMapHazardsChangedClientRpc();
    }

    protected override string GetObjectName() => $"Breaker Box <i>{component.GetInstanceID()}</i>";
}
#region

using Imperium.API.Types.Networking;
using Imperium.Core.Lifecycle;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntryBreakerBox : ObjectEntry
{
    protected override bool CanRespawn() => false;
    protected override bool CanDrop() => false;
    protected override bool CanDestroy() => true;
    protected override bool CanTeleportHere() => true;

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

    protected override void ToggleObject(bool isActive) => MoonManager.ToggleBreaker((BreakerBox)component, isActive);

    protected override string GetObjectName() => $"Breaker Box <i>{component.GetInstanceID()}</i>";
}
#region

using Imperium.API.Types.Networking;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntryMoldSpore : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanTeleportHere() => true;

    protected override bool CanDestroy() => Imperium.ObjectManager.StaticPrefabLookupMap.ContainsKey(containerObject);

    protected override void Respawn()
    {
    }

    public override void Destroy()
    {
        if (Imperium.ObjectManager.StaticPrefabLookupMap.TryGetValue(containerObject, out var moldObject))
        {
            Imperium.ObjectManager.DespawnObstacle(moldObject);
            base.Destroy();
        }
    }

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position => GetContainerObject().transform.position = position, origin);
    }

    protected override string GetObjectName() => $"Mold Spore <i>{component.GetInstanceID()}</i>";
}
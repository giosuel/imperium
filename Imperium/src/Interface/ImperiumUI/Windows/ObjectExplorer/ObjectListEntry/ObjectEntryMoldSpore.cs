#region

using Imperium.API.Types.Networking;
using Imperium.Interface.Common;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntryMoldSpore : ObjectEntry
{
    protected override bool CanRespawn() => true;
    protected override bool CanTeleportHere() => true;
    protected override bool CanDestroy() => true;

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
        Imperium.ImpPositionIndicator.Activate(position =>
        {
            if (Imperium.ObjectManager.StaticPrefabLookupMap.TryGetValue(containerObject, out var moldObject))
            {
                Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
                {
                    Destination = position,
                    NetworkId = moldObject
                });
            }
        }, origin);
    }

    protected override void InitEntry()
    {
        var canModify = Imperium.ObjectManager.StaticPrefabLookupMap.ContainsKey(containerObject);

        if (!canModify && tooltip)
        {
            destroyButton.interactable = false;
            teleportHereButton.interactable = false;

            if (!destroyButton.TryGetComponent<ImpInteractable>(out _))
            {
                var interactable = destroyButton.gameObject.AddComponent<ImpInteractable>();
                interactable.onEnter += () => tooltip.Activate(
                    "Local Object",
                    "Unable to destroy local objects instantiated by the game."
                );
                interactable.onExit += () => tooltip.Deactivate();
                interactable.onOver += position => tooltip.UpdatePosition(position);
            }

            if (!teleportHereButton.TryGetComponent<ImpInteractable>(out _))
            {
                var interactable = teleportHereButton.gameObject.AddComponent<ImpInteractable>();
                interactable.onEnter += () => tooltip.Activate(
                    "Local Object",
                    "Unable to teleport local objects instantiated by the game."
                );
                interactable.onExit += () => tooltip.Deactivate();
                interactable.onOver += position => tooltip.UpdatePosition(position);
            }
        }
    }

    protected override string GetObjectName() => $"Mold Spore <i>{component.GetInstanceID()}</i>";
}
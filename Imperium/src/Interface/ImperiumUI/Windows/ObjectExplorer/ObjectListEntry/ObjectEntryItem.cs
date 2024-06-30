#region

using Imperium.API.Types.Networking;
using Imperium.Core.Lifecycle;
using Imperium.Util;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntryItem : ObjectEntry
{
    protected override bool CanRespawn() => false;
    protected override bool CanDrop() => true;
    protected override bool CanTeleportHere() => true;

    private Image buttonIcon;

    private GrabbableObject item;

    protected override void Drop()
    {
        if (!item.isHeld || item.playerHeldBy is null) return;

        Imperium.PlayerManager.DropItem(new DropItemRequest
        {
            PlayerId = item.playerHeldBy.playerClientId,
            ItemIndex = PlayerManager.GetItemHolderSlot(item)
        });
    }

    public override void Destroy()
    {
        base.Destroy();
        Imperium.ObjectManager.DespawnItem(objectNetId!.Value);
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
        }, origin);
    }

    public override void UpdateEntry()
    {
        base.UpdateEntry();

        item = (GrabbableObject)component;

        var isInteractable = item.isHeld || item.heldByPlayerOnServer;
        if (!buttonIcon) buttonIcon = dropButton.transform.Find("Icon").GetComponent<Image>();

        ImpUtils.Interface.ToggleImageActive(buttonIcon, isInteractable);
        dropButton.interactable = isInteractable;
    }

    protected override string GetObjectName() => ((GrabbableObject)component).itemProperties.itemName;
}
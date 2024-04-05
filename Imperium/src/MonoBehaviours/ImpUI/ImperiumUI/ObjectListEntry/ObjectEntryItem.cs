#region

using Imperium.Core;
using Imperium.Netcode;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryItem : ObjectEntry
{
    protected override bool CanRespawn() => false;
    protected override bool CanDrop() => true;
    protected override bool CanTeleportHere() => true;

    private Image buttonIcon;

    protected override void Drop()
    {
        var item = (GrabbableObject)component;
        if (!item.isHeld || item.playerHeldBy is null) return;

        ImpNetPlayer.Instance.DiscardHotbarItemServerRpc(
            PlayerManager.GetPlayerID(item.playerHeldBy),
            PlayerManager.GetItemHolderSlot(item)
        );
    }

    public override void Destroy()
    {
        base.Destroy();
        ImpNetSpawning.Instance.DespawnItemServerRpc(containerObject.GetComponent<NetworkObject>().NetworkObjectId);
    }

    protected override void TeleportHere()
    {
        Imperium.PositionIndicator.Activate(position =>
        {
            var item = (GrabbableObject)component;
            Imperium.Log.LogInfo(
                $"teleported item {item.itemProperties.itemName} from {item.transform.position} to {ImpUtils.FormatVector(position)}");
            item.transform.position = position;
        });
    }

    public override void UpdateEntry()
    {
        var isInteractable = ((GrabbableObject)component).isHeld;
        if (!buttonIcon) buttonIcon = dropButton.transform.Find("Icon").GetComponent<Image>();

        ImpUtils.Interface.ToggleImageActive(buttonIcon, isInteractable);
        dropButton.interactable = isInteractable;
    }

    protected override string GetObjectName() => ((GrabbableObject)component).itemProperties.itemName;
}
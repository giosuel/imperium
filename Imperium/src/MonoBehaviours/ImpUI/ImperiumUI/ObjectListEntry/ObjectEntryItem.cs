#region

using Imperium.Core.Lifecycle;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine;
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

        Imperium.PlayerManager.DropItem(
            item.playerHeldBy.playerClientId,
            PlayerManager.GetItemHolderSlot(item)
        );
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
            var item = (GrabbableObject)component;
            var itemTransform = item.transform;
            itemTransform.position = position + Vector3.up;
            item.startFallingPosition = itemTransform.position;
            if (item.transform.parent != null)
            {
                item.startFallingPosition = item.transform.parent.InverseTransformPoint(item.startFallingPosition);
            }

            item.FallToGround();
        }, origin);
    }

    public override void UpdateEntry()
    {
        base.UpdateEntry();

        var isInteractable = ((GrabbableObject)component).isHeld;
        if (!buttonIcon) buttonIcon = dropButton.transform.Find("Icon").GetComponent<Image>();

        ImpUtils.Interface.ToggleImageActive(buttonIcon, isInteractable);
        dropButton.interactable = isInteractable;
    }

    protected override string GetObjectName() => ((GrabbableObject)component).itemProperties.itemName;
}
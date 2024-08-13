using System;
using Imperium.Extensions;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class DynamicObjectEntry : MonoBehaviour
{
    private TMP_Text objectNameText;

    internal Button dropButton { get; private set; }
    internal Button reviveButton { get; private set; }
    internal Button killButton { get; private set; }
    internal Button destroyButton { get; private set; }
    internal Button respawnButton { get; private set; }
    internal Button teleportHereButton { get; private set; }
    internal Button teleportToButton { get; private set; }
    internal Toggle activeToggle { get; private set; }

    internal string objectName { get; private set; }
    internal GameObject containerObject { get; private set; }
    internal Component component { get; private set; }

    internal ulong? objectNetId { get; private set; }

    internal ImpBinding<bool> IsObjectActive { get; private set; }

    internal ImpTooltip tooltip { get; private set; }

    internal ObjectEntryType entryType { get; private set; }

    internal Action layoutRebuildCallback;

    private RectTransform rect;

    internal void InitItem(ImpBinding<ImpTheme> theme)
    {
        rect = gameObject.GetComponent<RectTransform>();

        IsObjectActive = new ImpBinding<bool>(true);
        Imperium.ObjectManager.DisabledObjects.onUpdate += disabledObjects =>
        {
            if (!objectNetId.HasValue) return;
            DynamicObjectEntryTypeHelper.ToggleObject(this, !disabledObjects.Contains(objectNetId.Value));
        };

        objectNameText = transform.Find("Name").GetComponent<TMP_Text>();

        activeToggle = ImpToggle.Bind("Active", transform, IsObjectActive, theme);
        IsObjectActive.onUpdate += isOn => DynamicObjectEntryTypeHelper.ToggleObject(this, isOn);
        IsObjectActive.onTrigger += ToggleDisabledObject;

        // Teleport to button
        teleportToButton = ImpButton.Bind("TeleportTo", transform,
            () =>
            {
                Imperium.PlayerManager.TeleportLocalPlayer(DynamicObjectEntryTypeHelper.GetTeleportPosition(this));
                Imperium.Interface.Close();
            },
            theme,
            isIconButton: true
        );

        // Teleport here button
        teleportHereButton = ImpButton.Bind(
            "TeleportHere",
            transform,
            () => DynamicObjectEntryTypeHelper.TeleportHere(this),
            theme,
            isIconButton: true
        );

        // Destroy button (Unthemed, as it's red in any theme)
        destroyButton = ImpButton.Bind(
            "Destroy",
            transform,
            () => DynamicObjectEntryTypeHelper.Destroy(this)
        );

        // Respawn button
        respawnButton = ImpButton.Bind(
            "Respawn",
            transform,
            () => DynamicObjectEntryTypeHelper.Respawn(this),
            theme,
            isIconButton: true
        );

        // Drop button
        dropButton = ImpButton.Bind(
            "Drop",
            transform,
            () => DynamicObjectEntryTypeHelper.Drop(this),
            theme,
            isIconButton: true
        );

        // Kill button (Unthemes, as it is red in every theme)
        killButton = ImpButton.Bind("Kill", transform, () => DynamicObjectEntryTypeHelper.Kill(this));

        // Revive button (Unthemed, as it is blue in every theme)
        reviveButton = ImpButton.Bind("Revive", transform, () => DynamicObjectEntryTypeHelper.Revive(this));
    }

    private void ToggleDisabledObject()
    {
        if (!objectNetId.HasValue) return;
        Imperium.ObjectManager.DisabledObjects.Set(Imperium.ObjectManager.DisabledObjects.Value.Toggle(objectNetId.Value));
    }

    internal void ClearItem(int index, float positionY)
    {
        component = null;
        tooltip = null;

        objectNameText.text = "";
        teleportHereButton.gameObject.SetActive(false);
        teleportToButton.gameObject.SetActive(false);
        destroyButton.gameObject.SetActive(false);
        activeToggle.gameObject.SetActive(false);
        respawnButton.gameObject.SetActive(false);
        dropButton.gameObject.SetActive(false);
        killButton.gameObject.SetActive(false);
        reviveButton.gameObject.SetActive(false);

        rect.anchoredPosition = new Vector2(0, -positionY);
    }

    internal void SetItem(
        Component entryComponent, ObjectEntryType type, ImpTooltip tooltipObj, Action layoutRebuild, float positionY
    )
    {
        entryType = type;
        component = entryComponent;

        tooltip = tooltipObj;
        layoutRebuildCallback = layoutRebuild;

        rect.anchoredPosition = new Vector2(0, -positionY);

        objectName = DynamicObjectEntryTypeHelper.GetObjectName(this);
        containerObject = DynamicObjectEntryTypeHelper.GetContainerObject(this);
        objectNameText.text = objectName;

        objectNetId = containerObject.gameObject.GetComponent<NetworkObject>()?.NetworkObjectId;

        if (objectNetId.HasValue)
        {
            // Silently change binding to be consistent with the new object's active status
            if (IsObjectActive.Value == Imperium.ObjectManager.DisabledObjects.Value.Contains(objectNetId.Value))
            {
                IsObjectActive.Set(!Imperium.ObjectManager.DisabledObjects.Value.Contains(objectNetId.Value), false);
            }
        }

        teleportToButton.gameObject.SetActive(true);
        teleportHereButton.gameObject.SetActive(true);
        destroyButton.gameObject.SetActive(DynamicObjectEntryTypeHelper.CanDestroy(this));
        activeToggle.gameObject.SetActive(DynamicObjectEntryTypeHelper.CanToggle(this) && objectNetId.HasValue);
        respawnButton.gameObject.SetActive(DynamicObjectEntryTypeHelper.CanRespawn(this));
        dropButton.gameObject.SetActive(DynamicObjectEntryTypeHelper.CanDrop(this));
        killButton.gameObject.SetActive(DynamicObjectEntryTypeHelper.CanKill(this));
        reviveButton.gameObject.SetActive(DynamicObjectEntryTypeHelper.CanRevive(this));

        DynamicObjectEntryTypeHelper.InitObject(this);
    }

    private void Update()
    {
        DynamicObjectEntryTypeHelper.Update(this);
    }
}
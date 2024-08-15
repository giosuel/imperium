using System;
using Imperium.API.Types.Networking;
using Imperium.Extensions;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntry : MonoBehaviour
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

    internal ObjectType Type { get; private set; }

    private RectTransform rect;

    internal Action forceDelayedUpdate { get; private set; }

    private readonly ImpTimer intervalUpdateTimer = ImpTimer.ForInterval(0.2f);

    internal void InitItem(ImpBinding<ImpTheme> theme, Action forceDelayedUpdateCallback)
    {
        rect = gameObject.GetComponent<RectTransform>();
        forceDelayedUpdate = forceDelayedUpdateCallback;

        IsObjectActive = new ImpBinding<bool>(true);
        Imperium.ObjectManager.DisabledObjects.onUpdate += disabledObjects =>
        {
            if (!objectNetId.HasValue) return;
            ObjectEntryGenerator.ToggleObject(this, !disabledObjects.Contains(objectNetId.Value));
        };

        objectNameText = transform.Find("Name").GetComponent<TMP_Text>();

        activeToggle = ImpToggle.Bind("Active", transform, IsObjectActive, theme);
        IsObjectActive.onUpdate += isOn => ObjectEntryGenerator.ToggleObject(this, isOn);
        IsObjectActive.onTrigger += ToggleDisabledObject;

        // Teleport to button
        teleportToButton = ImpButton.Bind("TeleportTo", transform,
            () =>
            {
                Imperium.PlayerManager.TeleportLocalPlayer(ObjectEntryGenerator.GetTeleportPosition(this));
                Imperium.Interface.Close();
            },
            theme,
            isIconButton: true
        );

        // Teleport here button
        teleportHereButton = ImpButton.Bind(
            "TeleportHere",
            transform,
            () => ObjectEntryGenerator.TeleportObjectHere(this),
            theme,
            isIconButton: true
        );

        // Destroy button (Unthemed, as it's red in any theme)
        destroyButton = ImpButton.Bind(
            "Destroy",
            transform,
            () => ObjectEntryGenerator.DespawnObject(this)
        );

        // Respawn button
        respawnButton = ImpButton.Bind(
            "Respawn",
            transform,
            () => ObjectEntryGenerator.RespawnObject(this),
            theme,
            isIconButton: true
        );

        // Drop button
        dropButton = ImpButton.Bind(
            "Drop",
            transform,
            () => ObjectEntryGenerator.DropObject(this),
            theme,
            isIconButton: true
        );

        // Kill button (Unthemes, as it is red in every theme)
        killButton = ImpButton.Bind("Kill", transform, () => ObjectEntryGenerator.KillObject(this));

        // Revive button (Unthemed, as it is blue in every theme)
        reviveButton = ImpButton.Bind("Revive", transform, () => ObjectEntryGenerator.ReviveObject(this));

        forceDelayedUpdate += ClearItem;
    }

    private void ToggleDisabledObject()
    {
        if (!objectNetId.HasValue) return;
        Imperium.ObjectManager.DisabledObjects.Set(Imperium.ObjectManager.DisabledObjects.Value.Toggle(objectNetId.Value));
    }

    internal void ClearItem() => ClearItem(0);

    internal void ClearItem(float positionY)
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

    internal void SetItem(Component entryComponent, ObjectType type, ImpTooltip tooltipObj, float positionY)
    {
        Type = type;
        component = entryComponent;

        tooltip = tooltipObj;

        rect.anchoredPosition = new Vector2(0, -positionY);

        objectName = ObjectEntryGenerator.GetObjectName(this);
        containerObject = ObjectEntryGenerator.GetContainerObject(this);
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
        destroyButton.gameObject.SetActive(ObjectEntryGenerator.CanDestroy(this));
        activeToggle.gameObject.SetActive(ObjectEntryGenerator.CanToggle(this) && objectNetId.HasValue);
        respawnButton.gameObject.SetActive(ObjectEntryGenerator.CanRespawn(this));
        dropButton.gameObject.SetActive(ObjectEntryGenerator.CanDrop(this));
        killButton.gameObject.SetActive(ObjectEntryGenerator.CanKill(this));
        reviveButton.gameObject.SetActive(ObjectEntryGenerator.CanRevive(this));

        ObjectEntryGenerator.InitObject(this);
    }

    private void Update()
    {
        if (intervalUpdateTimer.Tick() && component)
        {
            // Update kill and revive buttons since they switch based on the player's alive status
            killButton.gameObject.SetActive(ObjectEntryGenerator.CanKill(this));
            reviveButton.gameObject.SetActive(ObjectEntryGenerator.CanRevive(this));

            ObjectEntryGenerator.IntervalUpdate(this);
        }
    }
}
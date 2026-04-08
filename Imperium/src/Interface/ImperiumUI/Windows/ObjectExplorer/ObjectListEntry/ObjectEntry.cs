#region

using System.Collections.Generic;
using Imperium.API.Types.Networking;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using JetBrains.Annotations;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntry : MonoBehaviour
{
    private TMP_Text objectNameText;

    internal Button dropButton { get; private set; }
    internal Button unlockButton { get; private set; }
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

    internal NetworkObjectReference? netObj { get; private set; }

    internal ImpBinding<bool> IsObjectActive { get; private set; }

    internal ImpTooltip tooltip { get; private set; }

    internal ObjectType Type { get; private set; }

    private RectTransform rect;

    private readonly ImpTimer intervalUpdateTimer = ImpTimer.ForInterval(0.2f);

    internal void Init(ImpBinding<ImpTheme> theme)
    {
        rect = gameObject.GetComponent<RectTransform>();

        IsObjectActive = new ImpBinding<bool>(true);
        IsObjectActive.onUpdateSecondary += ToggleObject;

        objectNameText = transform.Find("Name").GetComponent<TMP_Text>();
        activeToggle = ImpToggle.Bind("Active", transform, IsObjectActive, theme);

        // Teleport to button
        teleportToButton = ImpButton.Bind("TeleportTo", transform,
            () =>
            {
                Imperium.PlayerManager.TeleportLocalPlayer(ObjectEntryActions.GetTeleportPosition(this));
                Imperium.Interface.Close();
            },
            theme,
            isIconButton: true
        );

        // Teleport here button
        teleportHereButton = ImpButton.Bind(
            "TeleportHere",
            transform,
            () => ObjectEntryActions.TeleportObjectHere(this),
            theme,
            isIconButton: true
        );

        // Destroy button (Unthemed, as it's red in any theme)
        destroyButton = ImpButton.Bind(
            "Destroy",
            transform,
            () => ObjectEntryActions.DespawnObject(this));

        // Respawn button
        respawnButton = ImpButton.Bind(
            "Respawn",
            transform,
            () => ObjectEntryActions.RespawnObject(this),
            theme,
            isIconButton: true
        );

        // Drop button
        dropButton = ImpButton.Bind(
            "Drop",
            transform,
            () => ObjectEntryActions.DropObject(this),
            theme,
            isIconButton: true
        );

        // Unlock button
        unlockButton = ImpButton.Bind(
            "Unlock",
            transform,
            () => ObjectEntryActions.UnlockObject(this),
            theme,
            isIconButton: true
        );

        // Kill button (Unthemes, as it is red in every theme)
        killButton = ImpButton.Bind("Kill", transform, () => ObjectEntryActions.KillObject(this));

        // Revive button (Unthemed, as it is blue in every theme)
        reviveButton = ImpButton.Bind("Revive", transform, () => ObjectEntryActions.ReviveObject(this));
    }

    private void OnDisabledObjectsUpdate(HashSet<NetworkObjectReference> disabledObjects)
    {
        if (!netObj.HasValue) return;

        var isActive = !disabledObjects.Contains(netObj.Value);

        // Only update if value has not been changed locally already
        if (IsObjectActive.Value == isActive) return;

        ObjectEntryActions.ToggleObject(this, isActive);
        IsObjectActive.Set(isActive, invokeSecondary: false);
    }

    private void ToggleObject(bool isActive)
    {
        if (!netObj.HasValue) return;

        if (isActive)
        {
            Imperium.ObjectManager.DisabledObjects.Value.Remove(netObj.Value);
        }
        else
        {
            Imperium.ObjectManager.DisabledObjects.Value.Add(netObj.Value);
        }

        Imperium.ObjectManager.DisabledObjects.Set(Imperium.ObjectManager.DisabledObjects.Value);

        // Toggle manually on local client
        ObjectEntryActions.ToggleObject(this, isActive);
    }

    /**
     * Called when the object entry engine assigns a new virtual to this entry.
     */
    internal void SetItem([CanBeNull] Component entryComponent, ObjectType type, ImpTooltip tooltipObj, float positionY)
    {
        if (!entryComponent) return;
        rect.anchoredPosition = new Vector2(0, -positionY);

        if (entryComponent == component) return;

        Type = type;
        component = entryComponent;
        tooltip = tooltipObj;

        objectName = ObjectEntryActions.GetObjectName(this);
        containerObject = ObjectEntryActions.GetContainerObject(this);
        objectNameText.text = objectName;

        netObj = containerObject.GetComponent<NetworkObject>();

        if (netObj.HasValue)
        {
            // Silently change binding to be consistent with the new object's active status
            IsObjectActive.Set(
                !Imperium.ObjectManager.DisabledObjects.Value.Contains(netObj.Value),
                invokeSecondary: false
            );
        }

        teleportToButton.gameObject.SetActive(true);
        teleportHereButton.gameObject.SetActive(true);
        destroyButton.gameObject.SetActive(ObjectEntryActions.CanDestroy(this));
        activeToggle.gameObject.SetActive(ObjectEntryActions.CanToggle(this));
        respawnButton.gameObject.SetActive(ObjectEntryActions.CanRespawn(this));
        dropButton.gameObject.SetActive(ObjectEntryActions.CanDrop(this));
        unlockButton.gameObject.SetActive(ObjectEntryActions.CanUnlock(this));
        killButton.gameObject.SetActive(ObjectEntryActions.CanKill(this));
        reviveButton.gameObject.SetActive(ObjectEntryActions.CanRevive(this));

        ObjectEntryActions.InitObject(this);
    }

    /**
     * Called when this entry doesn't need to represent any virtual item. The entry will be cleared and hidden.
     */
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
        unlockButton.gameObject.SetActive(false);
        killButton.gameObject.SetActive(false);
        reviveButton.gameObject.SetActive(false);

        rect.anchoredPosition = new Vector2(0, -positionY);
    }

    private void OnEnable()
    {
        Imperium.ObjectManager.DisabledObjects.onUpdate += OnDisabledObjectsUpdate;
    }

    private void OnDisable()
    {
        Imperium.ObjectManager.DisabledObjects.onUpdate -= OnDisabledObjectsUpdate;
    }

    private void Update()
    {
        if (intervalUpdateTimer.Tick() && component)
        {
            // Update kill and revive buttons since they switch based on the player's alive status
            killButton.gameObject.SetActive(ObjectEntryActions.CanKill(this));
            reviveButton.gameObject.SetActive(ObjectEntryActions.CanRevive(this));

            ObjectEntryActions.IntervalUpdate(this);
        }
    }
}
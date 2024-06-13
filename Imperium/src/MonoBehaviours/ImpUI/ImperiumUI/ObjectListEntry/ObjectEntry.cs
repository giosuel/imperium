#region

using Imperium.Extensions;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntry : MonoBehaviour
{
    private TMP_Text objectNameText;
    private Toggle activeToggle;

    protected Button dropButton;
    protected Button reviveButton;
    protected Button killButton;

    protected string objectName;
    protected GameObject containerObject;
    protected Component component;

    protected ulong? objectNetId;

    protected ImpBinding<bool> IsObjectActive;

    internal void Init(Component objectComponent, ImpBinding<ImpTheme> theme)
    {
        component = objectComponent;

        objectNameText = transform.Find("Name").GetComponent<TMP_Text>();
        objectName = GetObjectName();
        containerObject = GetContainerObject();
        SetName(objectName);

        objectNetId = containerObject.gameObject.GetComponent<NetworkObject>()?.NetworkObjectId;

        if (objectNetId.HasValue)
        {
            Imperium.ObjectManager.DisabledObjects.onUpdate += disabledObjects =>
            {
                ToggleObject(!disabledObjects.Contains(objectNetId.Value));
            };
            IsObjectActive = new ImpBinding<bool>(
                !Imperium.ObjectManager.DisabledObjects.Value.Contains(objectNetId.Value)
            );
        }
        else
        {
            IsObjectActive = new ImpBinding<bool>(true);
        }

        // Active toggle
        activeToggle = ImpToggle.Bind("Active", transform, IsObjectActive, theme);
        activeToggle.gameObject.SetActive(CanToggle() && objectNetId.HasValue);
        IsObjectActive.onUpdate += ToggleObject;
        IsObjectActive.onTrigger += ToggleDisabledObject;

        // Teleport to button
        ImpButton.Bind("TeleportTo", transform,
            () =>
            {
                Imperium.PlayerManager.TeleportLocalPlayer(GetTeleportPosition());
                Imperium.Interface.Close();
            },
            theme,
            isIconButton: true
        );

        // Teleport here button
        var teleportHereButton = ImpButton.Bind("TeleportHere", transform, TeleportHere, theme, isIconButton: true);
        teleportHereButton.gameObject.SetActive(CanTeleportHere());

        // Destroy button (Unthemed, as it's red in any theme)
        var destroyButton = ImpButton.Bind("Destroy", transform, Destroy);
        destroyButton.gameObject.SetActive(CanDestroy());

        // Respawn button
        var respawnButton = ImpButton.Bind("Respawn", transform, Respawn, theme, isIconButton: true);
        respawnButton.gameObject.SetActive(CanRespawn());

        // Drop button
        dropButton = ImpButton.Bind("Drop", transform, Drop, theme, isIconButton: true);
        dropButton.gameObject.SetActive(CanDrop());

        // Kill button (Unthemes, as it is red in every theme)
        killButton = ImpButton.Bind("Kill", transform, Kill);
        killButton.gameObject.SetActive(CanKill());

        // Revive button (Unthemed, as it is blue in every theme)
        reviveButton = ImpButton.Bind("Revive", transform, Revive);
        reviveButton.gameObject.SetActive(CanRevive());

        InitEntry();
        UpdateEntry();
    }

    protected virtual void InitEntry()
    {
    }

    private void ToggleDisabledObject()
    {
        if (!objectNetId.HasValue) return;
        Imperium.ObjectManager.DisabledObjects.Set(Imperium.ObjectManager.DisabledObjects.Value.Toggle(objectNetId.Value));
    }

    public virtual void UpdateEntry()
    {
        SetName(GetObjectName());
    }

    public virtual void Destroy()
    {
        Destroy(gameObject);
    }

    private void SetName(string text)
    {
        objectNameText.text = text;
    }

    protected virtual string GetObjectName() => component.name;
    protected virtual GameObject GetContainerObject() => component.gameObject;
    protected virtual Vector3 GetTeleportPosition() => containerObject.transform.position;

    protected virtual void ToggleObject(bool isActive) => containerObject.SetActive(isActive);

    protected virtual void Respawn()
    {
    }

    protected virtual void Drop()
    {
    }

    protected virtual void Kill()
    {
    }

    protected virtual void Revive()
    {
    }

    protected virtual void TeleportHere()
    {
    }

    protected virtual bool CanDestroy() => true;
    protected virtual bool CanRespawn() => false;
    protected virtual bool CanDrop() => false;
    protected virtual bool CanKill() => false;
    protected virtual bool CanRevive() => false;
    protected virtual bool CanTeleportHere() => false;
    protected virtual bool CanToggle() => true;
}
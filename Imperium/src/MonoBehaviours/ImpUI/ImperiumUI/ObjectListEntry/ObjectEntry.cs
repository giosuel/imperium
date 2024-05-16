#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
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

    private ImpBinding<bool> IsObjectActive;

    internal void Init(Component objectComponent, ImpBinding<ImpTheme> theme)
    {
        objectNameText = transform.Find("Name").GetComponent<TMP_Text>();

        component = objectComponent;

        objectName = GetObjectName();
        containerObject = GetContainerObject();

        IsObjectActive = new ImpBinding<bool>(true, containerObject.SetActive);

        SetName(objectName);

        // Active toggle
        activeToggle = ImpToggle.Bind("Active", transform, IsObjectActive, theme);
        activeToggle.gameObject.SetActive(CanToggle());

        // Teleport to button
        ImpButton.Bind("TeleportTo", transform, () =>
        {
            PlayerManager.TeleportTo(GetTeleportPosition());
            Imperium.Interface.Close();
        }, theme, isIconButton: true);

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

        UpdateEntry();
    }

    public virtual void UpdateEntry()
    {
        SetName(GetObjectName());
        if (IsObjectActive.Value != containerObject.activeSelf) IsObjectActive.Set(containerObject.activeSelf, true);
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
    protected virtual Vector3 GetTeleportPosition() => component.gameObject.transform.position;

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
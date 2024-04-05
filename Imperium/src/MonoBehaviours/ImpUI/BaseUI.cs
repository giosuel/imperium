#region

using System;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.MonoBehaviours.ImpUI;

internal abstract class BaseUI : MonoBehaviour
{
    internal bool IsOpen { get; private set; }

    // Top-level component of the UI, containing all windows and or components
    protected Transform container;

    private bool closeOnMove { get; set; }
    public bool ignoreTab { get; private set; }

    protected event Action onOpen;

    internal ImpInterfaceManager interfaceManager;

    public virtual void Awake() => InitializeUI();

    /// <summary>
    /// For child UIs that are only opened through a click in a parent and not via key bind
    /// </summary>
    /// <param name="closeOnMovement">Whether the UI should be closed on movement inputs (WASD)</param>
    /// <param name="ignoreTabInput">Whether to ignore the tab close shortcut</param>
    public void InitializeUI(bool closeOnMovement = true, bool ignoreTabInput = false)
    {
        closeOnMove = closeOnMovement;
        ignoreTab = ignoreTabInput;
        container = transform.Find("Container");
        onOpen += OnOpen;

        InitUI();

        container.gameObject.SetActive(false);
        Imperium.Log.LogInfo($"[OK] Successfully loaded {GetType()} !");
    }

    /// <summary>
    /// This function will be overriden by the implementing UIs to initialize their UI parts at spawn time
    /// </summary>
    protected abstract void InitUI();

    private void CloseEvent(InputAction.CallbackContext _) => CloseUI();

    internal void OnUIClose()
    {
        container.gameObject.SetActive(false);
        IsOpen = false;

        if (closeOnMove)
        {
            Imperium.IngamePlayerSettings.playerInput.actions.FindAction("Move").performed -= CloseEvent;
        }
    }

    internal void OnUIOpen()
    {
        container.gameObject.SetActive(true);
        IsOpen = true;

        onOpen?.Invoke();
        ImpUtils.PlayClip(ImpAssets.ButtonClick);

        if (closeOnMove)
        {
            Imperium.IngamePlayerSettings.playerInput.actions.FindAction("Move").performed += CloseEvent;
        }
    }

    protected void CloseUI()
    {
        if (interfaceManager)
        {
            interfaceManager.Close();
        }
        else
        {
            OnUIClose();
        }
    }

    protected void OpenUI()
    {
        if (interfaceManager)
        {
            interfaceManager.Open(GetType());
        }
        else
        {
            OnUIOpen();
        }
    }

    protected virtual void OnOpen()
    {
    }

    public virtual bool CanOpen()
    {
        return true;
    }
}
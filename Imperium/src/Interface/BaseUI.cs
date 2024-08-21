#region

using System;
using Imperium.Core;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Interface;

/// <summary>
///     Basic Imperium UI. Can be used as part of an <see cref="ImpInterfaceManager" /> or as standalone UI.
/// </summary>
public abstract class BaseUI : MonoBehaviour
{
    internal bool IsOpen { get; private set; }

    /// <summary>
    ///     Reference to the container UI component
    /// </summary>
    protected Transform container;

    /// <summary>
    ///     Whether the UI should be closed when a movement input is detected
    /// </summary>
    private bool closeOnMove { get; set; } = true;

    /// <summary>
    ///     Whether the UI should not react to tab inputs
    /// </summary>
    internal bool IgnoreTab { get; private set; }

    /// <summary>
    ///     The binding that controls the theme of the UI component.
    ///     UIs can implement the function <see cref="OnThemeUpdate" /> to style components.
    ///     It is called every time the theme binding updates.
    /// </summary>
    protected ImpBinding<ImpTheme> theme;

    protected ImpTooltip tooltip;

    internal event Action onOpen;
    internal event Action onClose;

    /// <summary>
    ///     The interface manager this UI belongs to, if it belongs to a manager.
    /// </summary>
    [CanBeNull] internal ImpInterfaceManager interfaceManager;

    public virtual void InitUI(
        ImpBinding<ImpTheme> themeBinding,
        ImpTooltip impTooltip = null,
        bool closeOnMovement = false,
        bool ignoreTab = false
    )
    {
        container = transform.Find("Container");
        tooltip = impTooltip;
        closeOnMove = closeOnMovement;
        IgnoreTab = ignoreTab;

        onOpen += OnOpen;
        onClose += OnClose;

        theme = themeBinding;
        if (theme != null)
        {
            theme.onUpdate += OnThemeUpdate;
            ImpThemeManager.Style(
                theme.Value,
                container,
                // Window background color
                new StyleOverride("", Variant.BACKGROUND),
                // Titlebox border color
                new StyleOverride("TitleBox", Variant.DARKER),
                // Window border color
                new StyleOverride("Border", Variant.DARKER),
                new StyleOverride("Content", Variant.DARKER),
                new StyleOverride("Content/Border", Variant.DARKER)
            );

            // Window title
            ImpThemeManager.StyleText(
                theme.Value,
                container,
                new StyleOverride("TitleBox/Title", Variant.FOREGROUND)
            );
        }

        InitUI();

        // Style UI with the current theme
        OnThemeUpdate(themeBinding.Value);

        if (container) container.gameObject.SetActive(false);
        Imperium.IO.LogInfo($"[OK] Successfully loaded {GetType()} !");
    }

    /// <summary>
    ///     This function will be overriden by the implementing UIs to initialize their UI parts at spawn time
    /// </summary>
    protected virtual void InitUI()
    {
    }

    private void CloseEvent(InputAction.CallbackContext _) => Close();

    /// <summary>
    ///     Closes the UI.
    /// </summary>
    public void Close()
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

    /// <summary>
    ///     Opens the UI.
    /// </summary>
    public void Open()
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

    internal void OnUIClose()
    {
        if (container) container.gameObject.SetActive(false);
        IsOpen = false;

        onClose?.Invoke();
        if (closeOnMove)
        {
            Imperium.IngamePlayerSettings.playerInput.actions.FindAction("Move").performed -= CloseEvent;
        }
    }

    internal void OnUIOpen()
    {
        if (container) container.gameObject.SetActive(true);
        IsOpen = true;

        onOpen?.Invoke();
        GameUtils.PlayClip(ImpAssets.ButtonClick);

        if (closeOnMove)
        {
            Imperium.IngamePlayerSettings.playerInput.actions.FindAction("Move").performed += CloseEvent;
        }
    }

    /// <summary>
    ///     Manually invoke the on open functionality.
    /// </summary>
    internal void InvokeOnOpen() => onOpen?.Invoke();

    /// <summary>
    ///     Called every time the theme binding updates.
    /// </summary>
    /// <param name="themeUpdate">The updated theme</param>
    protected virtual void OnThemeUpdate(ImpTheme themeUpdate)
    {
    }

    /// <summary>
    ///     Called when the UI is opened.
    /// </summary>
    protected virtual void OnOpen()
    {
    }

    /// <summary>
    ///     Called when then UI is closed.
    /// </summary>
    protected virtual void OnClose()
    {
    }

    public virtual bool CanOpen()
    {
        return true;
    }
}
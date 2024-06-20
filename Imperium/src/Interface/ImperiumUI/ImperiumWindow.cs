#region

using System;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace Imperium.Interface.ImperiumUI;

internal abstract class ImperiumWindow : MonoBehaviour, ICloseable, IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerDownHandler
{
    protected ImpBinding<ImpTheme> theme;

    protected ImpTooltip tooltip;

    internal event Action onOpen;
    internal event Action onClose;
    internal event Action onFocus;

    protected Transform titleBox;

    private WindowDefinition windowDefinition;

    public void InitWindow(ImpBinding<ImpTheme> themeBinding, WindowDefinition definition, ImpTooltip impTootip)
    {
        theme = themeBinding;
        windowDefinition = definition;
        tooltip = impTootip;

        titleBox = transform.Find("TitleBox");
        if (titleBox) ImpButton.Bind("Close", titleBox, Close, theme: this.theme, isIconButton: true);

        onOpen += OnOpen;
        theme.onUpdate += OnThemeUpdate;
        theme.onUpdate += value =>
        {
            ImpThemeManager.Style(
                value,
                transform,
                // Window background color
                new StyleOverride("", Variant.BACKGROUND),
                // Titlebox border color
                new StyleOverride("TitleBox", Variant.DARKER),
                // Window border color
                new StyleOverride("Border", Variant.DARKER),
                new StyleOverride("Content", Variant.DARKER),
                new StyleOverride("Content/Border", Variant.DARKER)
            );

            if (titleBox)
            {
                // Window title
                ImpThemeManager.StyleText(
                    value,
                    transform,
                    new StyleOverride("TitleBox/Title", Variant.FOREGROUND)
                );
            }
        };

        InitWindow();

        // Style UI with the current theme
        OnThemeUpdate(theme.Value);

        transform.gameObject.SetActive(false);
    }

    internal void PlaceWindow(System.Numerics.Vector2 position, float scale, bool isOpen)
    {
        transform.position = new Vector2(position.X, position.Y);
        transform.localScale = new Vector3(scale * 1, scale * 1, 1);
        scaleFactor = scale;

        FocusWindow();

        // We don't want to call Open() here, as we don't want to call all the callbacka and play the noise
        if (isOpen)
        {
            onOpen?.Invoke();
            transform.gameObject.SetActive(true);
        }
    }


    protected void RegisterWidget<T>(Transform container, string path) where T : ImpWidget
    {
        container.Find(path).gameObject.AddComponent<T>().Init(theme, tooltip, ref onOpen, ref onClose);
    }

    protected abstract void InitWindow();

    /// <summary>
    ///     Hides the window.
    /// </summary>
    public void Close()
    {
        transform.gameObject.SetActive(false);

        windowDefinition.IsOpen = false;

        onClose?.Invoke();
    }

    /// <summary>
    ///     Show the window.
    /// </summary>
    public void Open()
    {
        transform.gameObject.SetActive(true);

        FocusWindow();

        // Set position if is opened the first time
        windowDefinition.Position = new System.Numerics.Vector2(transform.position.x, transform.position.y);
        windowDefinition.IsOpen = true;

        onOpen?.Invoke();
        GameUtils.PlayClip(ImpAssets.ButtonClick);
    }

    protected virtual void OnThemeUpdate(ImpTheme themeUpdated)
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void OnOpen()
    {
    }

    private void FocusWindow()
    {
        transform.SetAsLastSibling();
        onFocus?.Invoke();
    }

    public virtual bool CanOpen()
    {
        return true;
    }

    private float scaleFactor = 1f;
    private Vector2 dragOrigin;

    public void OnDrag(PointerEventData eventData)
    {
        if (Imperium.InputBindings.BaseMap["Alt"].IsPressed())
        {
            var delta = eventData.delta.magnitude * 0.002f;
            var windowOrigin = new Vector2(transform.position.x, transform.position.y);
            if ((windowOrigin - eventData.position).magnitude <
                (windowOrigin - eventData.position + eventData.delta).magnitude) delta *= -1;
            scaleFactor = Math.Clamp(scaleFactor + delta, 0.5f, 1f);

            transform.localScale = Vector3.one * scaleFactor + new Vector3(0, 0, 1);
            dragOrigin = eventData.position;

            windowDefinition.ScaleFactor = scaleFactor;
        }
        else
        {
            transform.position = (Vector2)transform.position + eventData.delta;
            windowDefinition.Position = new System.Numerics.Vector2(transform.position.x, transform.position.y);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        FocusWindow();
        Cursor.lockState = CursorLockMode.Confined;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Cursor.lockState = CursorLockMode.None;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        FocusWindow();
        dragOrigin = eventData.position;
    }
}
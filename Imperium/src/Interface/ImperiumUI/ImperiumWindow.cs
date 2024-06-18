#region

using System;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI;

internal abstract class ImperiumWindow : MonoBehaviour, ICloseable, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler
{
    protected ImpBinding<ImpTheme> theme;

    internal event Action onOpen;
    internal event Action onClose;

    protected Transform titleBox;

    private WindowDefinition windowDefinition;

    public void InitWindow(ImpBinding<ImpTheme> themeBinding, WindowDefinition definition)
    {
        theme = themeBinding;
        windowDefinition = definition;

        titleBox = transform.Find("TitleBox");
        if (titleBox) ImpButton.Bind("Close", titleBox, Close, theme: this.theme, isIconButton: true);

        onOpen += OnOpen;
        onOpen += FocusWindow;
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
        OnThemeUpdate(this.theme.Value);

        transform.gameObject.SetActive(false);
    }


    protected void RegisterWidget<T>(Transform container, string path) where T : ImpWidget
    {
        container.Find(path).gameObject.AddComponent<T>().Init(this, theme, ref onOpen, ref onClose);
    }

    protected abstract void InitWindow();

    private void CloseEvent(InputAction.CallbackContext _) => Close();

    /// <summary>
    ///     Hides the window.
    /// </summary>
    public void Close()
    {
        transform.gameObject.SetActive(false);

        onClose?.Invoke();
    }

    /// <summary>
    ///     Show the window.
    /// </summary>
    public void Open()
    {
        transform.gameObject.SetActive(true);

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

    private void FocusWindow() => transform.SetAsLastSibling();

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
            if ((windowOrigin - eventData.position).magnitude < (windowOrigin - eventData.position + eventData.delta).magnitude) delta *= -1;
            scaleFactor = Math.Clamp(scaleFactor + delta, 0.5f, 1f);

            transform.localScale = Vector3.one * scaleFactor + new Vector3(0, 0, 1);
            dragOrigin = eventData.position;

            windowDefinition.ScaleFactor = scaleFactor;
        }
        else
        {
            transform.position = (Vector2)transform.position + eventData.delta;
            windowDefinition.Position = transform.position;
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
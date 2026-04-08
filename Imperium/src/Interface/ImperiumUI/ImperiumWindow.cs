#region

using System;
using System.Collections;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Vector2 = System.Numerics.Vector2;

#endregion

namespace Imperium.Interface.ImperiumUI;

internal abstract class ImperiumWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerDownHandler
{
    protected ImpBinding<ImpTheme> theme;

    protected ImpTooltip tooltip;

    private CanvasGroup windowGroup;
    private Coroutine fadeAnimation;

    internal event Action onOpen;
    internal event Action onClose;
    internal event Action onFocus;

    protected Transform titleBox;
    protected RectTransform rect;

    private WindowDefinition windowDefinition;

    private ImperiumUI parent;

    internal InputAction openKeybind;

    public void InitWindow(
        ImpBinding<ImpTheme> themeBinding, WindowDefinition definition, ImpTooltip impTootip, ImperiumUI parentUI
    )
    {
        parent = parentUI;
        theme = themeBinding;
        windowDefinition = definition;
        tooltip = impTootip;

        titleBox = transform.Find("TitleBox");
        windowGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        if (titleBox) ImpButton.Bind("Close", titleBox, Close, theme: theme, isIconButton: true);

        onOpen += OnOpen;
        onOpen += SetWindowAnchors;

        onOpen += () =>
        {
            if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            fadeAnimation = StartCoroutine(animateOpacityTo(0.05f, 1));
        };

        onClose += () =>
        {
            if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            fadeAnimation = StartCoroutine(animateOpacityTo(0.05f, 0, false));
        };

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

        windowGroup.alpha = 0;
        windowGroup.interactable = false;
        windowGroup.blocksRaycasts = false;
    }

    private IEnumerator animateOpacityTo(float duration, float targetAlpha, bool setInteractable = true)
    {
        var startAlpha = windowGroup.alpha;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            windowGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        windowGroup.alpha = targetAlpha;
        windowGroup.interactable = setInteractable;
        windowGroup.blocksRaycasts = setInteractable;
    }

    private void OnDestroy()
    {
        if (openKeybind != null) openKeybind.performed -= OnKeybindOpen;
    }

    internal void OnKeybindOpen(InputAction.CallbackContext _)
    {
        if (!parent.IsOpen) parent.Open();

        if (windowDefinition.IsOpen)
        {
            FocusWindow();
            if (Imperium.Settings.Preferences.PlaySounds.Value) GameUtils.PlayClip(ImpAssets.OpenClick);
        }
        else
        {
            Open();
        }
    }

    internal void PlaceWindow(Vector2 position, float scale, bool isOpen)
    {
        transform.position = new UnityEngine.Vector2(position.X, position.Y);
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
        container.Find(path).gameObject.AddComponent<T>().Init(theme, tooltip, ref onOpen, ref onClose, parent);
    }

    /// <summary>
    ///     Hides the window.
    /// </summary>
    public void Close()
    {
        // transform.gameObject.SetActive(false);
        // if (fadeAnimation != null) StopCoroutine(fadeAnimation);
        // fadeAnimation = StartCoroutine(animateOpacityTo(0.05f, 0, false));

        windowDefinition.IsOpen = false;

        onClose?.Invoke();
    }

    /// <summary>
    ///     Show the window.
    /// </summary>
    public void Open()
    {
        FocusWindow();

        // Set position if is opened the first time
        windowDefinition.Position = new Vector2(transform.position.x, transform.position.y);
        windowDefinition.IsOpen = true;

        onOpen?.Invoke();
        if (Imperium.Settings.Preferences.PlaySounds.Value) GameUtils.PlayClip(ImpAssets.OpenClick);
    }

    /// <summary>
    ///     Called by <see cref="ImperiumUI" /> whever it opens.
    /// </summary>
    public void InvokeOnOpen() => onOpen?.Invoke();

    private void FocusWindow()
    {
        transform.SetAsLastSibling();
        onFocus?.Invoke();
    }

    private float scaleFactor = 1f;

    /// <summary>
    ///     Sets the window's anchors based on the screen side the window is currently placed without moving the window.
    /// </summary>
    private void SetWindowAnchors()
    {
        var windowWorldPosition = transform.position;
        var posX = transform.position.x;

        if (posX < Screen.width / 2f)
        {
            rect.anchorMin = new UnityEngine.Vector2(0, 0.5f);
            rect.anchorMax = new UnityEngine.Vector2(0, 0.5f);
        }
        else
        {
            rect.anchorMin = new UnityEngine.Vector2(1, 0.5f);
            rect.anchorMax = new UnityEngine.Vector2(1, 0.5f);
        }

        // Restore original window position
        transform.position = windowWorldPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Imperium.InputBindings.StaticMap["Alt"].IsPressed())
        {
            var delta = eventData.delta.magnitude * 0.002f;
            var windowOrigin = new UnityEngine.Vector2(transform.position.x, transform.position.y);
            if ((windowOrigin - eventData.position).magnitude <
                (windowOrigin - eventData.position + eventData.delta).magnitude) delta *= -1;
            scaleFactor = Math.Clamp(scaleFactor + delta, 0.5f, 1f);

            transform.localScale = Vector3.one * scaleFactor + new Vector3(0, 0, 1);

            windowDefinition.ScaleFactor = scaleFactor;
        }
        else
        {
            transform.position = (UnityEngine.Vector2)transform.position + eventData.delta;
            windowDefinition.Position = new Vector2(transform.position.x, transform.position.y);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        FocusWindow();
        Cursor.lockState = CursorLockMode.Confined;

        if (windowGroup)
        {
            if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            fadeAnimation = StartCoroutine(animateOpacityTo(0.15f, 0.9f));
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Cursor.lockState = CursorLockMode.None;

        if (windowGroup)
        {
            if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            fadeAnimation = StartCoroutine(animateOpacityTo(0.15f, 1f));
        }

        SetWindowAnchors();
    }

    public void OnPointerDown(PointerEventData eventData) => FocusWindow();

    protected abstract void InitWindow();

    protected void CloseParent() => parent.Close();

    protected virtual void OnThemeUpdate(ImpTheme themeUpdated)
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void OnOpen()
    {
    }

    public virtual bool CanOpen()
    {
        return true;
    }
}
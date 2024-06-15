#region

using System;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI;

internal abstract class BaseWindow : MonoBehaviour
{
    // Reference to the content UI component
    protected Transform content;
    protected Transform titleBox;
    public event Action onOpen;

    // Binding holding the current theme
    protected ImpBinding<ImpTheme> themeBinding;

    protected BaseUI parentUI;

    /// <summary>
    ///     Registers a window and implements close / collapse functionality for the window.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="theme">The theme to use for the window.</param>
    public void RegisterWindow(BaseUI parent, ImpBinding<ImpTheme> theme)
    {
        parentUI = parent;
        themeBinding = theme;

        content = transform.Find("Content");
        if (!content) content = transform.Find("Main/Content");

        // Use scrollbar content if content is scroll bar
        if (content && content.gameObject.GetComponent<ScrollRect>())
        {
            content = content.Find("Viewport/Content");
        }

        titleBox = transform.Find("TitleBox");
        if (titleBox)
        {
            var draggable = titleBox.gameObject.AddComponent<ImpDraggable>();
            draggable.Init(transform);
            ImpButton.Bind("Close", titleBox, CloseUI, theme: themeBinding, isIconButton: true);
            if (titleBox.Find("Arrow"))
            {
                ImpButton.CreateCollapse(
                    "Arrow",
                    titleBox,
                    transform.Find("Container/Window/Main"),
                    theme: themeBinding
                );
            }
        }

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

        RegisterWindow();

        // Style UI with the current theme
        OnThemeUpdate(themeBinding.Value);
    }

    protected virtual void OnThemeUpdate(ImpTheme themeUpdated)
    {
    }

    public void TriggerOnOpen() => onOpen?.Invoke();

    protected void CloseUI() => parentUI.CloseUI();

    protected virtual void OnOpen()
    {
    }

    protected abstract void RegisterWindow();
}
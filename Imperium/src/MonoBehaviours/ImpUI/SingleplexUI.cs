#region

using System;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI;

/// <summary>
///     Simple UI with a single window.
///     Uses the component called "Window" in the UI container as the window component.
/// </summary>
internal abstract class SingleplexUI : BaseUI
{
    // Main content of the UI, can be null
    protected Transform content;

    // Titlebox of the UI, can be null
    private Transform titleBox;

    public override void InitializeUI(
        ImpBinding<ImpTheme> themeBinding,
        bool closeOnMovement = true,
        bool ignoreTabInput = false
    )
    {
        InitSingleplex(themeBinding);
        base.InitializeUI(themeBinding, closeOnMovement, ignoreTabInput);
    }

    private void InitSingleplex(ImpBinding<ImpTheme> themeBinding, BaseUI parent = null)
    {
        content = transform.Find("Container/Window/Content");
        if (!content) content = transform.Find("Container/Window/Main/Content");

        // Use scrollbar content if content is scroll bar
        if (content && content.gameObject.GetComponent<ScrollRect>())
        {
            content = content.Find("Viewport/Content");
        }

        titleBox = transform.Find("Container/Window/TitleBox");
        if (titleBox)
        {
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

        themeBinding.onUpdate += value =>
        {
            ImpThemeManager.Style(value, container.Find("Window"), [
                // Window background color
                new StyleOverride("", Variant.BACKGROUND),
                // Titlebox border color
                new StyleOverride("TitleBox", Variant.DARKER),
                // Window border color
                new StyleOverride("Border", Variant.DARKER),
                new StyleOverride("Content", Variant.DARKER),
                new StyleOverride("Content/Border", Variant.DARKER)
            ]);

            if (titleBox)
            {
                // Window title
                ImpThemeManager.StyleText(
                    value,
                    container.Find("Window"),
                    new StyleOverride("TitleBox/Title", Variant.FOREGROUND)
                );
            }
        };
    }
}
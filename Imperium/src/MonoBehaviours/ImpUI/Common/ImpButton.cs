#region

using System.Linq;
using Imperium.Core;
using Imperium.Core.Lifecycle;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

public abstract class ImpButton
{
    /// <summary>
    ///     Binds a unity button with an onclick listener and interactiveBindings
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="onClick"></param>
    /// <param name="theme">The theme the button will use</param>
    /// <param name="isIconButton">Whether the button represents an icon button (For theming)</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="playClickSound">Whether the click sound playes when the button is clicked.</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static Button Bind(
        string path,
        Transform container,
        UnityAction onClick,
        IBinding<ImpTheme> theme = null,
        bool isIconButton = false,
        bool interactableInvert = false,
        bool playClickSound = true,
        params IBinding<bool>[] interactableBindings
    )
    {
        var buttonObject = container.Find(path);
        if (!buttonObject)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind button '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        if (playClickSound) button.onClick.AddListener(() => GameUtils.PlayClip(ImpAssets.GrassClick));

        var icon = buttonObject.Find("Icon")?.GetComponent<Image>();

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(
                button, icon,
                interactableBindings.All(entry => entry.Value),
                interactableInvert
            );
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onUpdate += value => ToggleInteractable(
                    button, icon,
                    interactableBindings.All(entry => entry.Value),
                    interactableInvert
                );
            }
        }

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, buttonObject, isIconButton);
            OnThemeUpdate(theme.Value, buttonObject, isIconButton);
        }

        return button;
    }

    /// <summary>
    ///     Binds a unity button with an onclick listener and interactiveBindings
    ///     This version is meant for arrow buttons that collapse an area
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="collapseArea"></param>
    /// <param name="theme">The theme the button will use</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static void CreateCollapse(
        string path,
        Transform container,
        Transform collapseArea,
        IBinding<ImpTheme> theme = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var buttonObject = container.Find(path);
        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            collapseArea.gameObject.SetActive(!collapseArea.gameObject.activeSelf);
            button.transform.Rotate(0, 0, 180);
        });
        button.onClick.AddListener(() => GameUtils.PlayClip(ImpAssets.GrassClick));

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(button, null, interactableBindings.All(entry => entry.Value), interactableInvert);
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onUpdate += value => ToggleInteractable(button, null, value, interactableInvert);
            }
        }

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, buttonObject, true);
            OnThemeUpdate(theme.Value, buttonObject, true);
        }
    }

    private static void ToggleInteractable(Selectable button, [CanBeNull] Image icon, bool isOn, bool inverted)
    {
        button.interactable = inverted ? !isOn : isOn;
        if (icon) ImpUtils.Interface.ToggleImageActive(icon, inverted ? !isOn : isOn);
    }

    private static void OnThemeUpdate(ImpTheme theme, Transform container, bool isIconButton)
    {
        if (isIconButton)
        {
            ImpThemeManager.Style(
                theme,
                container,
                new StyleOverride("", Variant.LIGHTER),
                new StyleOverride("Icon", Variant.LIGHTER)
            );
        }
        else
        {
            ImpThemeManager.Style(
                theme,
                container,
                new StyleOverride("", Variant.DARKER),
                new StyleOverride("Icon", Variant.DARKER)
            );
        }
    }
}
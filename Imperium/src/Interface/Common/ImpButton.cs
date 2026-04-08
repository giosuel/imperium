#region

using System;
using System.Linq;
using Imperium.Core;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.Common;

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
    /// <param name="tooltipDefinition">The tooltip definition of the button tooltip.</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static Button Bind(
        string path,
        Transform container,
        UnityAction onClick,
        IBinding<ImpTheme> theme = null,
        bool isIconButton = false,
        bool interactableInvert = false,
        bool playClickSound = true,
        TooltipDefinition tooltipDefinition = null,
        params IBinding<bool>[] interactableBindings
    )
    {
        var buttonObject = container.Find(path);
        if (!buttonObject || !buttonObject.TryGetComponent<Button>(out var button))
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind button '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        button.onClick.AddListener(() =>
        {
            onClick();

            if (Imperium.Settings.Preferences.PlaySounds.Value && playClickSound) GameUtils.PlayClip(ImpAssets.ButtonClick);
        });

        var icon = buttonObject.Find("Icon")?.GetComponent<Image>();
        var text = buttonObject.Find("Text")?.GetComponent<TMP_Text>() ??
                   buttonObject.Find("Text (TMP)")?.GetComponent<TMP_Text>();

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(
                button, icon, text,
                interactableBindings.All(entry => entry.Value),
                interactableInvert
            );
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onTrigger += () => ToggleInteractable(
                    button, icon, text,
                    interactableBindings.All(entry => entry.Value),
                    interactableInvert
                );
            }
        }

        if (tooltipDefinition != null)
        {
            var interactable = buttonObject.gameObject.AddComponent<ImpInteractable>();
            interactable.onOver += position => tooltipDefinition.Tooltip.SetPosition(
                tooltipDefinition.Title,
                tooltipDefinition.Description,
                position,
                tooltipDefinition.HasAccess
            );
            interactable.onExit += () => tooltipDefinition.Tooltip.Deactivate();
        }

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, buttonObject, isIconButton);
            OnThemeUpdate(theme.Value, buttonObject, isIconButton);
        }

        return button;
    }

    internal static Button Bind(
        string path,
        Transform container,
        IBinding<bool> stateBinding,
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
        button.onClick.AddListener(() =>
        {
            stateBinding.Set(!stateBinding.Value);

            if (Imperium.Settings.Preferences.PlaySounds.Value && playClickSound) GameUtils.PlayClip(ImpAssets.ButtonClick);
        });

        var icon = buttonObject.Find("Icon")?.GetComponent<Image>();
        var text = buttonObject.Find("Text")?.GetComponent<TMP_Text>() ??
                   buttonObject.Find("Text (TMP)")?.GetComponent<TMP_Text>();

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(
                button, icon, text,
                interactableBindings.All(entry => entry.Value),
                interactableInvert
            );
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onTrigger += () => ToggleInteractable(
                    button, icon, text,
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
    /// <param name="stateBinding"></param>
    /// <param name="theme">The theme the button will use</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="updateFunction">Optional update function that is executed when the button is pressed</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static void CreateCollapse(
        string path,
        Transform container,
        Transform collapseArea = null,
        IBinding<bool> stateBinding = null,
        IBinding<ImpTheme> theme = null,
        bool interactableInvert = false,
        Action updateFunction = null,
        params IBinding<bool>[] interactableBindings
    )
    {
        var buttonObject = container.Find(path);
        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            stateBinding?.Set(!stateBinding.Value);
            if (collapseArea) collapseArea.gameObject.SetActive(!collapseArea.gameObject.activeSelf);
            button.transform.Rotate(0, 0, 180);
            updateFunction?.Invoke();

            if (Imperium.Settings.Preferences.PlaySounds.Value) GameUtils.PlayClip(ImpAssets.ButtonClick);
        });

        if (stateBinding != null && collapseArea)
        {
            stateBinding.onUpdate += isOn =>
            {
                collapseArea.gameObject.SetActive(isOn);
                button.transform.rotation = Quaternion.Euler(
                    button.transform.rotation.x,
                    button.transform.rotation.y,
                    isOn ? 180 : 0
                );
            };
        }

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(button, null, null, interactableBindings.All(entry => entry.Value), interactableInvert);
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onUpdate += value => ToggleInteractable(button, null, null, value, interactableInvert);
            }
        }

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, buttonObject, true);
            OnThemeUpdate(theme.Value, buttonObject, true);
        }
    }

    internal static void ToggleButton(Button button, bool isOn, bool inverted = false)
    {
        var icon = button.transform.Find("Icon")?.GetComponent<Image>();
        var text = button.transform.Find("Text")?.GetComponent<TMP_Text>() ??
                   button.transform.Find("Text (TMP)")?.GetComponent<TMP_Text>();

        ToggleInteractable(button, icon, text, isOn, inverted);
    }

    private static void ToggleInteractable(
        Selectable button,
        [CanBeNull] Image icon,
        [CanBeNull] TMP_Text text,
        bool isOn,
        bool inverted
    )
    {
        button.interactable = inverted ? !isOn : isOn;
        if (icon) ImpUtils.Interface.ToggleImageActive(icon, inverted ? !isOn : isOn);
        if (text) ImpUtils.Interface.ToggleTextActive(text, inverted ? !isOn : isOn);
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
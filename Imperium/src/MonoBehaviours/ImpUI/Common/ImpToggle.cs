#region

using System.Linq;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

/// <summary>
///     Represents a toggle in the Imperium UI, supports two types of structures
///     Toggle (Toggle)
///     - Background (Image)
///     - Checkmark (Image)
///     - Text (TMP_Text)
///     Toggle (Toggle, Image)
///     - Checkmark (Image)
/// </summary>
public abstract class ImpToggle
{
    /// <summary>
    ///     Binds a unity toggle with an ImpBinding and interactiveBindings
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding">Binding that decides on the state of the toggle</param>
    /// <param name="interactableBindings">List of bindings that decide if the button is interactable</param>
    internal static Toggle Bind(
        string path,
        Transform container,
        ImpBinding<bool> valueBinding,
        params ImpBinding<bool>[] interactableBindings
    )
    {
        var toggleObject = container.Find(path);
        var toggle = toggleObject.GetComponent<Toggle>();
        var checkmark = toggleObject.Find("Background/Checkmark")?.GetComponent<Image>()
                        ?? toggleObject.Find("Checkmark").GetComponent<Image>();
        var text = toggleObject.Find("Text")?.GetComponent<TMP_Text>();

        toggle.isOn = valueBinding.Value;
        toggle.onValueChanged.AddListener(valueBinding.Set);
        toggle.onValueChanged.AddListener(_ => GameManager.PlayClip(ImpAssets.GrassClick));
        valueBinding.onUpdate += value => toggle.isOn = value;

        if (interactableBindings.Length > 0)
        {
            var isOn = interactableBindings.All(entry => entry.Value);
            ToggleInteractable(checkmark, text, toggle, isOn);

            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onUpdate += value => ToggleInteractable(checkmark, text, toggle, value);
            }
        }

        return toggle;
    }

    private static void ToggleInteractable(
        Image checkmark,
        [CanBeNull] TMP_Text text,
        Selectable toggle,
        bool isOn
    )
    {
        toggle.interactable = isOn;
        ImpUtils.Interface.ToggleImageActive(checkmark, isOn);
        if (text) ImpUtils.Interface.ToggleTextActive(text, isOn);
    }
}
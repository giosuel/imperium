#region

using System.Collections.Generic;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

internal abstract class ImpMultiSelectEntry
{
    /// <summary>
    ///     MultiSelectEntry ImpUI Component - Represents an entry in a <see cref="ImpMultiSelect" />.
    ///     Note: This can also be used without ImpMultiSelect.
    ///     Required UI Layout:
    ///     Template
    ///     "Selected" (Image?) - Overlay that's shown when the entry is selected.
    ///     "Hover" (Image?) - Overlay that's shown when the mouse hovers over the entry.
    ///     "Text" (TMP_Text?) - Text that contains the label string.
    /// </summary>
    /// <param name="value">The value the current entry represents</param>
    /// <param name="entryObj">The UI component.</param>
    /// <param name="selectionBinding">The binding that controls the group's currently selected item.</param>
    /// <param name="hoverBinding">The binding that controls which item in the group is currently being hovered.</param>
    /// <param name="theme">The theme the multi-select entry will use</param>
    /// <param name="label">The label of the current entry.</param>
    internal static void Bind<T>(
        T value,
        GameObject entryObj,
        ImpBinding<T> selectionBinding,
        ImpBinding<T> hoverBinding,
        ImpBinding<ImpTheme> theme = null,
        string label = null
    )
    {
        var entryInteractable = entryObj.AddComponent<ImpInteractable>();

        if (label != null) entryObj.transform.Find("Text").GetComponent<TMP_Text>().text = label;

        var selectedOverlay = entryObj.transform.Find("Selected").gameObject;
        selectedOverlay.SetActive(EqualityComparer<T>.Default.Equals(value, selectionBinding.Value));

        var hoverOverlay = entryObj.transform.Find("Hover").gameObject;
        hoverOverlay.SetActive(EqualityComparer<T>.Default.Equals(value, selectionBinding.Value));

        entryInteractable.onEnter += () => hoverBinding.Set(value);
        entryInteractable.onExit += () => hoverBinding.Set(default);
        entryInteractable.onClick += () => selectionBinding.Set(value);

        selectionBinding.onUpdate += selectedValue =>
        {
            if (selectedOverlay) selectedOverlay.SetActive(EqualityComparer<T>.Default.Equals(value, selectedValue));
        };

        hoverBinding.onUpdate += hoveredValue =>
        {
            if (hoverOverlay) hoverOverlay.SetActive(EqualityComparer<T>.Default.Equals(value, hoveredValue));
        };

        if (theme != null)
        {
            theme.onUpdate += updatedTheme => OnThemeUpdate(updatedTheme, entryObj.transform);
            OnThemeUpdate(theme.Value, entryObj.transform);
        }
    }

    private static void OnThemeUpdate(ImpTheme theme, Transform container)
    {
        ImpThemeManager.Style(
            theme,
            container,
            new StyleOverride("Hover", Variant.FADED),
            new StyleOverride("Hover", Variant.LIGHTER),
            new StyleOverride("Box", Variant.FOREGROUND)
        );
    }
}
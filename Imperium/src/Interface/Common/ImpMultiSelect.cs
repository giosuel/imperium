#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

public abstract class ImpMultiSelect
{
    /// <summary>
    ///     MultiSelect ImpUI Component - Represents a multi-select UI system.
    ///     Required UI Layout:
    ///     Root
    ///     "Content" (ScrollRect)
    ///     "Viewport" (Mask)
    ///     "Content"
    ///     "Template" (<see cref="ImpMultiSelectEntry" />)
    /// </summary>
    /// <param name="path">The path from the container to the UI component</param>
    /// <param name="container">The container of the UI component</param>
    /// <param name="selectionBinding">The binding that controls the group's currently selected item.</param>
    /// <param name="items">Selectable entries in the multi-select list</param>
    /// <param name="labelGenerator">A function that transforms the labels (e.g. for counting)</param>
    /// <param name="theme">The theme the multi-select will use</param>
    /// <param name="emptyPlaceholder">A placeholder that is displayed when there are no items in the list</param>
    public static void Bind<T>(
        string path,
        Transform container,
        IBinding<T> selectionBinding,
        IBinding<IReadOnlyCollection<T>> items,
        Func<T, string> labelGenerator,
        ImpBinding<ImpTheme> theme,
        string emptyPlaceholder = null
    )
    {
        var viewContainer = container.Find(path);
        var scrollView = viewContainer.Find("Content");
        var placeholder = viewContainer.Find("Placeholder")?.GetComponent<TMP_Text>();
        var multiSelectContainer = scrollView ? scrollView.Find("Viewport/Content") : viewContainer;

        var entryTemplate = multiSelectContainer.transform.Find("Template").gameObject;
        entryTemplate.SetActive(false);

        var labelMaker = labelGenerator;

        HashSet<GameObject> listEntries = [];

        if (placeholder && !string.IsNullOrEmpty(emptyPlaceholder)) placeholder.text = emptyPlaceholder;

        items.onUpdate += updatedItems =>
        {
            foreach (var item in listEntries) Object.Destroy(item);
            listEntries.Clear();

            if (updatedItems.Count <= 0)
            {
                if (placeholder) placeholder.gameObject.SetActive(true);
                if (scrollView) scrollView.gameObject.SetActive(false);
                return;
            }

            if (placeholder) placeholder.gameObject.SetActive(false);
            if (scrollView) scrollView.gameObject.SetActive(true);

            var hoveredEntry = new ImpBinding<T>();

            foreach (var item in updatedItems.Where(obj => obj != null))
            {
                var listEntryObj = Object.Instantiate(entryTemplate, multiSelectContainer.transform);
                listEntryObj.SetActive(true);
                ImpMultiSelectEntry.Bind(
                    item,
                    listEntryObj,
                    selectionBinding,
                    hoveredEntry,
                    theme,
                    labelMaker(item)
                );
                listEntries.Add(listEntryObj);
            }
        };
        items.Set(items.Value);

        if (theme != null)
        {
            theme.onUpdate += updatedTheme => OnThemeUpdate(updatedTheme, viewContainer.transform);
            OnThemeUpdate(theme.Value, viewContainer.transform);
        }
    }

    private static void OnThemeUpdate(ImpTheme theme, Transform container)
    {
        ImpThemeManager.Style(
            theme,
            container,
            new StyleOverride("", Variant.FOREGROUND),
            new StyleOverride("Content/Scrollbar", Variant.DARKEST),
            new StyleOverride("Content/Scrollbar/SlidingArea/Handle", Variant.FOREGROUND)
        );
    }
}
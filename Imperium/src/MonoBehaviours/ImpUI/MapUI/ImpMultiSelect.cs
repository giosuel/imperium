using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Imperium.MonoBehaviours.ImpUI.MapUI;

internal abstract class ImpMultiSelect
{
    internal static void Bind<T>(
        string path,
        Transform container,
        ImpBinding<T> selectedObject,
        ImpBinding<HashSet<T>> items,
        Func<T, string> labelGenerator,
        string emptyPlaceholder = null
    )
    {
        var viewContainer = container.Find(path);
        var scrollView = viewContainer.Find("Content");
        var placeholder = viewContainer.Find("Placeholder").GetComponent<TMP_Text>();
        var multiSelectContainer = scrollView.Find("Viewport/Content");

        var entryTemplate = multiSelectContainer.transform.Find("Template").gameObject;
        entryTemplate.SetActive(false);

        var labelMaker = labelGenerator;

        HashSet<GameObject> listEntries = [];

        if (!string.IsNullOrEmpty(emptyPlaceholder)) placeholder.text = emptyPlaceholder;

        items.onUpdate += updatedItems =>
        {
            foreach (var item in listEntries) Object.Destroy(item);
            listEntries.Clear();

            if (updatedItems.Count <= 0)
            {
                placeholder.gameObject.SetActive(true);
                scrollView.gameObject.SetActive(false);
                return;
            }

            placeholder.gameObject.SetActive(false);
            scrollView.gameObject.SetActive(true);

            foreach (var item in updatedItems.Where(obj => obj != null))
            {
                var listEntryObj = Object.Instantiate(entryTemplate, multiSelectContainer.transform);
                listEntryObj.SetActive(true);
                ImpMultiSelectEntry<T>.Bind(listEntryObj, item, labelMaker(item), selectedObject);
                listEntries.Add(listEntryObj);
            }
        };
        items.Set(items.Value);
    }
}
using System.Collections.Generic;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

namespace Imperium.MonoBehaviours.ImpUI.MapUI;

internal abstract class ImpMultiSelectEntry<T>
{
    internal static void Bind(
        GameObject entryObj,
        T entry,
        string label,
        ImpBinding<T> updateBinding
    )
    {
        var entryInteractable = entryObj.AddComponent<ImpInteractable>();

        entryObj.transform.Find("Text").GetComponent<TMP_Text>().text = label;
        var checkmark = entryObj.transform.Find("Check").gameObject;
        checkmark.SetActive(false);

        var cover = entryObj.transform.Find("Cover").gameObject;
        cover.SetActive(false);

        entryInteractable.onEnter += () => cover.SetActive(true);
        entryInteractable.onExit += () => cover.SetActive(false);
        entryInteractable.onClick += () => updateBinding.Set(entry);


        updateBinding.onUpdate += value =>
        {
            if (!checkmark) return;
            checkmark.SetActive(EqualityComparer<T>.Default.Equals(entry, value));
        };
    }
}
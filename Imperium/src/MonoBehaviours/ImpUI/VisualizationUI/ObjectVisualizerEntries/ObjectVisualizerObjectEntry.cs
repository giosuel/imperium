#region

using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.VisualizationUI.ObjectVisualizerEntries;

public class ObjectVisualizerObjectEntry : MonoBehaviour
{
    internal void Init(string entryName, ImpBinding<bool> entryBinding, ImpBinding<ImpTheme> themeBinding)
    {
        transform.Find("Name").GetComponent<TMP_Text>().text = entryName;

        ImpToggle.Bind("Checkboxes/Infos", transform, entryBinding, theme: themeBinding);
    }
}
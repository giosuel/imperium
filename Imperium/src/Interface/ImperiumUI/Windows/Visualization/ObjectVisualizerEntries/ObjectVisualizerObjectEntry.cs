#region

using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Visualization.ObjectVisualizerEntries;

public class ObjectVisualizerInsightEntry : MonoBehaviour
{
    internal void Init(string entryName, ImpBinding<bool> entryBinding, IBinding<ImpTheme> themeBinding)
    {
        transform.Find("Name").GetComponent<TMP_Text>().text = entryName;

        ImpToggle.Bind("Checkboxes/Insights", transform, entryBinding, theme: themeBinding);
    }
}
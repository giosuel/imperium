#region

using Imperium.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.VisualizationUI.ObjectVisualizerEntries;

public class ObjectVisualizerPlayerEntry : MonoBehaviour
{
    internal void Init(PlayerInfoConfig config, ImpBinding<ImpTheme> themeBinding)
    {
        transform.Find("Name").GetComponent<TMP_Text>().text = config.playerName;

        // ImpToggle.Bind("Checkboxes/Infos", transform, config.Info, theme: themeBinding);
    }
}
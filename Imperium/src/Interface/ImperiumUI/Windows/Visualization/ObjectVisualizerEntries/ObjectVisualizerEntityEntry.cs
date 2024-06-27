#region

using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.VisualizationUI.ObjectVisualizerEntries;

public class ObjectVisualizerEntityEntry : MonoBehaviour
{
    internal void Init(EntityGizmoConfig config, IBinding<ImpTheme> themeBinding)
    {
        transform.Find("Name").GetComponent<TMP_Text>().text = config.entityName;

        ImpToggle.Bind("Checkboxes/Pathfinding", transform, config.Pathfinding, theme: themeBinding);
        ImpToggle.Bind("Checkboxes/Targeting", transform, config.Targeting, theme: themeBinding);
        ImpToggle.Bind("Checkboxes/LineOfSight", transform, config.LineOfSight, theme: themeBinding);
        ImpToggle.Bind("Checkboxes/Hearing", transform, config.Hearing, theme: themeBinding);
        ImpToggle.Bind("Checkboxes/Custom", transform, config.Custom, theme: themeBinding);
    }
}
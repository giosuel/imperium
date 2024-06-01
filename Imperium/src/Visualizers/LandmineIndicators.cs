#region

using System.Collections.Generic;
using System.Linq;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class LandmineIndicators(
    ImpBinding<HashSet<Landmine>> objectsBinding,
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<HashSet<Landmine>>(objectsBinding, visibleBinding)
{
    protected override void Refresh(HashSet<Landmine> objects)
    {
        ClearObjects();

        foreach (var landmine in objects.Where(obj => obj))
        {
            if (!indicatorObjects.ContainsKey(landmine.GetInstanceID()))
            {
                var indicatorObject = new GameObject();
                indicatorObject.transform.SetParent(landmine.transform);
                var indicator = indicatorObject.AddComponent<LandmineIndicator>();
                indicator.Init(landmine);

                indicatorObjects[landmine.GetInstanceID()] = indicatorObject;
            }
        }
    }

    internal void SnapshotPlayerHitbox(int landmineId)
    {
        if (!indicatorObjects.TryGetValue(landmineId, out var landmine)) return;

        landmine.GetComponent<LandmineIndicator>().SnapshotHitboxes();
    }
}
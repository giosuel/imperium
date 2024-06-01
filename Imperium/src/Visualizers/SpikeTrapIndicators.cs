#region

using System.Collections.Generic;
using System.Linq;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class SpikeTrapIndicators(
    ImpBinding<HashSet<SpikeRoofTrap>> objectsBinding,
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<HashSet<SpikeRoofTrap>>(objectsBinding, visibleBinding)
{
    protected override void Refresh(HashSet<SpikeRoofTrap> objects)
    {
        ClearObjects();

        foreach (var spikeTrap in objects.Where(obj => obj))
        {
            if (!indicatorObjects.ContainsKey(spikeTrap.GetInstanceID()))
            {
                var indicatorObject = new GameObject();
                indicatorObject.transform.SetParent(spikeTrap.transform);
                var indicator = indicatorObject.AddComponent<SpikeTrapIndicator>();
                indicator.Init(spikeTrap);

                indicatorObjects[spikeTrap.GetInstanceID()] = indicatorObject;
            }
        }
    }
}
#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class SpikeTrapIndicators(ImpBinding<HashSet<SpikeRoofTrap>> objectsBinding) :
    BaseVisualizer<HashSet<SpikeRoofTrap>>("Spike Trap Indicators", objectsBinding)
{
    protected override void Refresh(HashSet<SpikeRoofTrap> objects)
    {
        ClearObjects();

        foreach (var landmine in objects.Where(obj => obj))
        {
            if (!indicatorObjects.ContainsKey(landmine.GetInstanceID()))
            {
                var indicatorObject = new GameObject();
                indicatorObject.transform.SetParent(landmine.transform);
                var indicator = indicatorObject.AddComponent<SpikeTrapIndicator>();
                indicator.Init(landmine);
                indicatorObject.SetActive(ImpSettings.Visualizations.SpikeTrapIndicators.Value);

                indicatorObjects[landmine.GetInstanceID()] = indicatorObject;
            }
        }
    }
}
#region

using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class SpikeTrapGizmos(
    IBinding<IReadOnlyCollection<SpikeRoofTrap>> objectsBinding,
    IBinding<bool> visibilityBinding
) : BaseVisualizer<IReadOnlyCollection<SpikeRoofTrap>, SpikeTrapGizmo>(objectsBinding, visibilityBinding)
{
    protected override void OnRefresh(IReadOnlyCollection<SpikeRoofTrap> objects)
    {
        ClearObjects();

        foreach (var spikeTrap in objects.Where(obj => obj))
        {
            if (!visualizerObjects.ContainsKey(spikeTrap.GetInstanceID()))
            {
                var indicatorObject = new GameObject($"Imp_SpikeTrapGizmo_{spikeTrap.GetInstanceID()}");
                indicatorObject.transform.SetParent(spikeTrap.transform.parent);

                var indicator = indicatorObject.AddComponent<SpikeTrapGizmo>();
                indicator.Init(spikeTrap);

                visualizerObjects[spikeTrap.GetInstanceID()] = indicator;
            }
        }
    }
}
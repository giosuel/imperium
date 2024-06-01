#region

using Imperium.Core;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class ShovelIndicators(ImpBinding<bool> visibleBinding) : BaseVisualizer<Shovel>(visibleBinding: visibleBinding)
{
    internal void Refresh(Shovel shovel, bool isActivelyHolding)
    {
        if (!indicatorObjects.TryGetValue(shovel.GetInstanceID(), out var indicatorObject))
        {
            indicatorObject = new GameObject();
            indicatorObject.transform.SetParent(shovel.transform);
            indicatorObject.AddComponent<ShovelIndicator>();

            indicatorObjects[shovel.GetInstanceID()] = indicatorObject;
        }

        indicatorObject.GetComponent<ShovelIndicator>().Init(shovel, isActivelyHolding);
        indicatorObject.gameObject.SetActive(ImpSettings.Visualizations.ShovelIndicators.Value);
    }
}
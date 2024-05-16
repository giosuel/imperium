#region

using Imperium.Core;
using Imperium.MonoBehaviours.VisualizerObjects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class ShotgunIndicators() : BaseVisualizer<ShotgunItem>("Shotgun Indicators")
{
    internal void Refresh(ShotgunItem shotgun, bool isActivelyHolding)
    {
        if (!indicatorObjects.TryGetValue(shotgun.GetInstanceID(), out var indicatorObject))
        {
            indicatorObject = new GameObject();
            indicatorObject.transform.SetParent(shotgun.transform);
            indicatorObject.AddComponent<ShotgunIndicator>();

            indicatorObjects[shotgun.GetInstanceID()] = indicatorObject;
        }

        indicatorObject.GetComponent<ShotgunIndicator>().Init(shotgun, isActivelyHolding);
        indicatorObject.gameObject.SetActive(ImpSettings.Visualizations.ShotgunIndicators.Value);
    }
}
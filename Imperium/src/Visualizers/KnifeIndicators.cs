#region

using Imperium.Core;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class KnifeIndicators(ImpBinding<bool> visibleBinding)
    : BaseVisualizer<Shovel>("Knife Indicators", visibleBinding)
{
    internal void Refresh(KnifeItem knife, bool isActivelyHolding)
    {
        if (!indicatorObjects.TryGetValue(knife.GetInstanceID(), out var indicatorObject))
        {
            indicatorObject = new GameObject();
            indicatorObject.transform.SetParent(knife.transform);
            indicatorObject.AddComponent<KnifeIndicator>();

            indicatorObjects[knife.GetInstanceID()] = indicatorObject;
        }

        indicatorObject.GetComponent<KnifeIndicator>().Init(knife, isActivelyHolding);
        indicatorObject.gameObject.SetActive(ImpSettings.Visualizations.KnifeIndicators.Value);
    }

    internal override void Toggle(bool isOn)
    {
        base.Toggle(isOn);
        foreach (var knife in Object.FindObjectsOfType<KnifeItem>()) Refresh(knife, knife.isHeld);
    }
}
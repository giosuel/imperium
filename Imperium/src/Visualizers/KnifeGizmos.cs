#region

using Imperium.Core;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class KnifeGizmos(
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<Shovel, KnifeGizmo>(visibleBinding: visibleBinding)
{
    internal void Refresh(KnifeItem knife, bool isActivelyHolding)
    {
        if (!visualizerObjects.TryGetValue(knife.GetInstanceID(), out var knifeIndicator))
        {
            var knifeIndicatorObject = new GameObject($"Imp_KnifeGizmo_{knife.GetInstanceID()}");
            knifeIndicatorObject.transform.SetParent(knife.transform);
            knifeIndicator = knifeIndicatorObject.AddComponent<KnifeGizmo>();

            visualizerObjects[knife.GetInstanceID()] = knifeIndicator;
        }

        knifeIndicator.Init(knife, isActivelyHolding);
        knifeIndicator.gameObject.SetActive(ImpSettings.Visualizations.KnifeIndicators.Value);
    }
}
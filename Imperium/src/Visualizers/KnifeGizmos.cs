#region

using Imperium.API.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class KnifeGizmos(
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<Shovel, KnifeGizmo>(visibilityBinding: visibilityBinding)
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
        knifeIndicator.gameObject.SetActive(Imperium.Settings.Visualization.KnifeIndicators.Value);
    }
}
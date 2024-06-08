#region

using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class SpawnIndicators(
    ImpBinding<OracleState> oracleStateBinding,
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<OracleState, SpawnIndicator>(oracleStateBinding, visibleBinding)
{
    protected override void OnRefresh(OracleState state)
    {
        ClearObjects();

        for (var i = state.currentCycle; i < state.outdoorCycles.Length; i++)
        {
            foreach (var spawnReport in state.outdoorCycles[i])
            {
                var indicatorObject = Object.Instantiate(ImpAssets.SpawnIndicator);
                var indicator = indicatorObject.AddComponent<SpawnIndicator>();
                indicator.transform.position = spawnReport.position;
                indicator.Init(
                    Imperium.ObjectManager.GetDisplayName(spawnReport.entity.enemyName),
                    spawnReport.spawnTime
                );

                visualizerObjects[indicatorObject.GetInstanceID()] = indicator;
            }
        }
    }
}
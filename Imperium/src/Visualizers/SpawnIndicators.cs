#region

using Imperium.API.Types;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class SpawnIndicators(
    ImpBinding<OracleState> oracleStateBinding,
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<OracleState, SpawnIndicator>(oracleStateBinding, visibilityBinding)
{
    protected override void OnRefresh(OracleState state)
    {
        ClearObjects();

        for (var i = state.CurrentCycle; i < state.OutdoorCycles.Length; i++)
        {
            foreach (var spawnReport in state.OutdoorCycles[i])
            {
                var indicatorObject = Object.Instantiate(ImpAssets.SpawnIndicator);
                var indicator = indicatorObject.AddComponent<SpawnIndicator>();
                indicator.transform.position = spawnReport.Position;
                indicator.Init(
                    Imperium.ObjectManager.GetDisplayName(spawnReport.Entity.enemyName),
                    spawnReport.SpawnTime
                );

                visualizerObjects[indicatorObject.GetInstanceID()] = indicator;
            }
        }
    }
}
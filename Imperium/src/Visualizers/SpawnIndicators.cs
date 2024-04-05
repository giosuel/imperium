#region

using Imperium.Core;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Oracle;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class SpawnIndicators(ImpBinding<OracleState> oracleStateBinding)
    : BaseVisualizer<OracleState>("Spawn Indicators", oracleStateBinding)
{
    protected override void Refresh(OracleState state)
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

                indicatorObject.SetActive(ImpSettings.Visualizations.SpawnIndicators.Value);
                indicatorObjects[indicatorObject.GetInstanceID()] = indicatorObject;
            }
        }
    }
}
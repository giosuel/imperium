#region

using System.Collections.Generic;
using Imperium.API;
using Imperium.API.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using Visualization = Imperium.Core.Visualization;

#endregion

namespace Imperium.Visualizers;

internal class MapHazardIndicators(
    ImpBinding<HashSet<HazardIndicator>> objectsBinding,
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<HashSet<HazardIndicator>, Transform>(objectsBinding, visibilityBinding)
{
    protected override void OnRefresh(HashSet<HazardIndicator> objects)
    {
        ClearObjects();

        foreach (var spawn in objects)
        {
            if (!visualizerObjects.ContainsKey(spawn.GetHashCode()))
            {
                visualizerObjects[spawn.GetHashCode()] = Visualization.VisualizePoint(
                    null,
                    spawn.spawnRange,
                    material: ImpAssets.WireframeRed,
                    name: "Imp_HazardSpawnIndicator"
                ).transform;
                visualizerObjects[spawn.GetHashCode()].position = spawn.position;
            }
        }
    }
}

internal class HazardIndicator(Vector3 position, float spawnRange)
{
    internal Vector3 position = position;
    internal readonly float spawnRange = spawnRange;
}
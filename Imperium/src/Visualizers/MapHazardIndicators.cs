#region

using System.Collections.Generic;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class MapHazardIndicators(
    ImpBinding<HashSet<HazardIndicator>> objectsBinding,
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<HashSet<HazardIndicator>, Transform>(objectsBinding, visibleBinding)
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
                    material: ImpAssets.WireframeRedMaterial
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
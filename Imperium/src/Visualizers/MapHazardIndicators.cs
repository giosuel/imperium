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
) : BaseVisualizer<HashSet<HazardIndicator>>(objectsBinding, visibleBinding)
{
    protected override void Refresh(HashSet<HazardIndicator> objects)
    {
        ClearObjects();

        foreach (var spawn in objects)
        {
            if (!indicatorObjects.ContainsKey(spawn.GetHashCode()))
            {
                indicatorObjects[spawn.GetHashCode()] = Visualization.VisualizePoint(
                    null,
                    spawn.spawnRange,
                    material: ImpAssets.WireframeRedMaterial
                );
                indicatorObjects[spawn.GetHashCode()].transform.position = spawn.position;
            }
        }
    }
}

internal class HazardIndicator(Vector3 position, float spawnRange)
{
    internal Vector3 position = position;
    internal float spawnRange = spawnRange;
}
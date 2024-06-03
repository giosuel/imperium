#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class ScrapSpawnIndicators(
    ImpBinding<HashSet<RandomScrapSpawn>> objectsBinding,
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<HashSet<RandomScrapSpawn>, Transform>(objectsBinding, visibleBinding)
{
    protected override void OnRefresh(HashSet<RandomScrapSpawn> objects)
    {
        ClearObjects();

        foreach (var spawn in objects.Where(obj => obj))
        {
            if (!visualizerObjects.ContainsKey(spawn.GetInstanceID()))
            {
                var size = spawn.spawnedItemsCopyPosition
                    ? 1f
                    : spawn.itemSpawnRange;

                var material = spawn.spawnedItemsCopyPosition
                    ? ImpAssets.WireframeCyanMaterial
                    : ImpAssets.WireframeAmaranthMaterial;

                visualizerObjects[spawn.GetInstanceID()] = Visualization.VisualizePoint(
                    spawn.gameObject,
                    size,
                    material: material
                ).transform;
            }
        }
    }
}
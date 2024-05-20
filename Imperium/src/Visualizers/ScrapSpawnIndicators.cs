#region

using System.Collections.Generic;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;

#endregion

namespace Imperium.Visualizers;

internal class ScrapSpawnIndicators(
    ImpBinding<HashSet<RandomScrapSpawn>> objectsBinding,
    ImpBinding<bool> visibleBinding)
    : BaseVisualizer<HashSet<RandomScrapSpawn>>("Scrap Spawns", objectsBinding, visibleBinding)
{
    protected override void Refresh(HashSet<RandomScrapSpawn> objects)
    {
        ClearObjects();

        foreach (var spawn in objects)
        {
            if (!indicatorObjects.ContainsKey(spawn.GetInstanceID()))
            {
                var size = spawn.spawnedItemsCopyPosition
                    ? 1f
                    : spawn.itemSpawnRange;

                var material = spawn.spawnedItemsCopyPosition
                    ? ImpAssets.WireframeCyanMaterial
                    : ImpAssets.WireframeAmaranthMaterial;

                indicatorObjects[spawn.GetInstanceID()] = Visualization.VisualizePoint(
                    spawn.gameObject,
                    size,
                    material: material
                );
            }
        }
    }
}
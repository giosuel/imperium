#region

using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using Visualization = Imperium.Core.Visualization;

#endregion

namespace Imperium.Visualizers;

internal class ScrapSpawnIndicators(
    ImpBinding<IReadOnlyCollection<RandomScrapSpawn>> objectsBinding,
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<IReadOnlyCollection<RandomScrapSpawn>, Transform>(objectsBinding, visibilityBinding)
{
    protected override void OnRefresh(IReadOnlyCollection<RandomScrapSpawn> objects)
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
                    ? ImpAssets.WireframeCyan
                    : ImpAssets.WireframeAmaranth;

                visualizerObjects[spawn.GetInstanceID()] = Visualization.VisualizePoint(
                    spawn.gameObject,
                    size,
                    material: material,
                    name: $"Imp_ScrapSpawnIndicator_{spawn.GetInstanceID()}"
                ).transform;
            }
        }
    }
}
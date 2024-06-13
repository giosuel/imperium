#region

using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class ScrapSpawnIndicators(
    ImpBinding<HashSet<RandomScrapSpawn>> objectsBinding,
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<HashSet<RandomScrapSpawn>, Transform>(objectsBinding, visibilityBinding)
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
                    ? API.Materials.WireframeCyan
                    : API.Materials.WireframeAmaranth;

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
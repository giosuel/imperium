#region

using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class NavMeshVisualizer(
    ImpBinding<bool> isLoadedBinding,
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<bool, Component>(isLoadedBinding, visibleBinding: visibleBinding)
{
    protected override void OnRefresh(bool isSceneLoaded)
    {
        ClearObjects();

        var index = 0;
        foreach (var navmeshSurface in Visualization.GetNavmeshSurfaces())
        {
            var navmeshVisualizer = new GameObject($"ImpVis_NavMeshSurface_{index}");
            var navmeshRenderer = navmeshVisualizer.AddComponent<MeshRenderer>();
            navmeshRenderer.material = ImpAssets.WireframeNavMeshMaterial;
            var navmeshFilter = navmeshVisualizer.AddComponent<MeshFilter>();
            navmeshFilter.mesh = navmeshSurface;

            visualizerObjects[navmeshVisualizer.GetInstanceID()] = navmeshRenderer;

            index++;
        }
    }
}
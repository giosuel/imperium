#region

using Imperium.API.Types;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class NavMeshVisualizer(
    ImpBinding<bool> isLoadedBinding,
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<bool, Component>(isLoadedBinding, visibilityBinding: visibilityBinding)
{
    protected override void OnRefresh(bool isSceneLoaded)
    {
        ClearObjects();

        var index = 0;
        foreach (var navmeshSurface in Visualization.GetNavmeshSurfaces())
        {
            var navmeshVisualizer = new GameObject($"ImpVis_NavMeshSurface_{index}");
            var navmeshRenderer = navmeshVisualizer.AddComponent<MeshRenderer>();
            navmeshRenderer.material = API.Materials.WireframeNavMesh;
            var navmeshFilter = navmeshVisualizer.AddComponent<MeshFilter>();
            navmeshFilter.mesh = navmeshSurface;

            visualizerObjects[navmeshVisualizer.GetInstanceID()] = navmeshRenderer;

            index++;
        }
    }
}
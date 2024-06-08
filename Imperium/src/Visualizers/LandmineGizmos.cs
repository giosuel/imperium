#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class LandmineGizmos(
    ImpBinding<HashSet<Landmine>> objectsBinding,
    ImpBinding<bool> visibleBinding
) : BaseVisualizer<HashSet<Landmine>, LandmineGizmo>(objectsBinding, visibleBinding)
{
    protected override void OnRefresh(HashSet<Landmine> objects)
    {
        ClearObjects();

        foreach (var landmine in objects.Where(obj => obj))
        {
            if (!visualizerObjects.ContainsKey(landmine.GetInstanceID()))
            {
                var landmineGizmoObject = new GameObject();
                landmineGizmoObject.transform.SetParent(landmine.transform);

                var landmineGizmo = landmineGizmoObject.AddComponent<LandmineGizmo>();
                landmineGizmo.Init(landmine);

                visualizerObjects[landmine.GetInstanceID()] = landmineGizmo;
            }
        }
    }

    internal void SnapshotPlayerHitbox(int landmineId)
    {
        if (!visualizerObjects.TryGetValue(landmineId, out var landmine)) return;

        landmine.GetComponent<LandmineGizmo>().SnapshotHitboxes();
    }
}
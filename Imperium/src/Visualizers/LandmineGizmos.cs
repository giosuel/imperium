#region

using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class LandmineGizmos(
    IBinding<IReadOnlyCollection<Landmine>> objectsBinding,
    IBinding<bool> visibilityBinding
) : BaseVisualizer<IReadOnlyCollection<Landmine>, LandmineGizmo>(objectsBinding, visibilityBinding)
{
    protected override void OnRefresh(IReadOnlyCollection<Landmine> objects)
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
#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal abstract class BaseVisualizer<T>
{
    protected readonly Dictionary<int, GameObject> indicatorObjects = [];

    protected BaseVisualizer(ImpBinding<T> objectsBinding = null, ImpBinding<bool> visibleBinding = null)
    {
        if (objectsBinding != null)
        {
            objectsBinding.onUpdate += objects =>
            {
                Refresh(objects);
                if (visibleBinding != null) OnVisibilityUpdate(visibleBinding.Value);
            };
        }

        if (visibleBinding != null) visibleBinding.onUpdate += OnVisibilityUpdate;
    }

    private void OnVisibilityUpdate(bool isVisible)
    {
        foreach (var obj in indicatorObjects.Values.Where(obj => obj)) obj.SetActive(isVisible);
    }

    internal virtual void Toggle(bool isOn) => ImpUtils.ToggleGameObjects(indicatorObjects.Values, isOn);

    protected void ClearObjects()
    {
        foreach (var indicatorObject in indicatorObjects.Values.Where(indicator => indicator))
        {
            Object.Destroy(indicatorObject);
        }

        indicatorObjects.Clear();
    }

    protected virtual void Refresh(T objects)
    {
    }
}
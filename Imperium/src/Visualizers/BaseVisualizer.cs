#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal abstract class BaseVisualizer<T, R> where R : Component
{
    protected readonly Dictionary<int, R> visualizerObjects = [];

    protected BaseVisualizer(ImpBinding<T> objectsBinding = null, ImpBinding<bool> visibleBinding = null)
    {
        if (objectsBinding != null)
        {
            objectsBinding.onUpdate += objects =>
            {
                OnRefresh(objects);
                if (visibleBinding != null) OnVisibilityUpdate(visibleBinding.Value);
            };
        }

        if (visibleBinding != null) visibleBinding.onUpdate += OnVisibilityUpdate;
    }

    private void OnVisibilityUpdate(bool isVisible)
    {
        foreach (var obj in visualizerObjects.Values.Where(obj => obj)) obj.gameObject.SetActive(isVisible);
    }

    protected void ClearObjects()
    {
        foreach (var indicatorObject in visualizerObjects.Values.Where(indicator => indicator))
        {
            Object.Destroy(indicatorObject.gameObject);
        }

        visualizerObjects.Clear();
    }

    protected virtual void OnRefresh(T objects)
    {
    }
}
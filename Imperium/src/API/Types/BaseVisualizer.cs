#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.API.Types;

/// <summary>
/// Defines a custom visualization manager for a gizmo or an indicator in Imperium.
/// </summary>
/// <typeparam name="T">The type of objects this visualizer will handle</typeparam>
/// <typeparam name="R">The visualizer object this manager works with</typeparam>
public abstract class BaseVisualizer<T, R> where R : Component
{
    protected readonly Dictionary<int, R> visualizerObjects = [];

    protected BaseVisualizer(IBinding<T> objectsBinding = null, IBinding<bool> visibilityBinding = null)
    {
        if (objectsBinding != null)
        {
            objectsBinding.onUpdate += objects =>
            {
                OnRefresh(objects);
                if (visibilityBinding != null) OnVisibilityUpdate(visibilityBinding.Value);
            };
        }

        if (visibilityBinding != null) visibilityBinding.onUpdate += OnVisibilityUpdate;
    }

    /// <summary>
    ///     Called when the visibility binding is updated.
    /// </summary>
    private void OnVisibilityUpdate(bool isVisible)
    {
        foreach (var obj in visualizerObjects.Values.Where(obj => obj)) obj.gameObject.SetActive(isVisible);
    }

    /// <summary>
    ///     Called when the objects binding is updated.
    /// </summary>
    /// <param name="objects"></param>
    protected virtual void OnRefresh(T objects)
    {
    }

    /// <summary>
    ///     Internal function to clear all the current visualizer objects.
    /// </summary>
    protected void ClearObjects()
    {
        foreach (var indicatorObject in visualizerObjects.Values.Where(indicator => indicator))
        {
            Object.Destroy(indicatorObject.gameObject);
        }

        visualizerObjects.Clear();
    }
}
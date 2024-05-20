#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal abstract class BaseVisualizer<T>
{
    private readonly string displayName;
    protected readonly Dictionary<int, GameObject> indicatorObjects = [];

    protected BaseVisualizer(string displayName, ImpBinding<bool> visibleBinding)
    {
        this.displayName = displayName;
        visibleBinding.onUpdate += OnVisibilityUpdate;
    }

    protected BaseVisualizer(string displayName, ImpBinding<T> objectsBinding, ImpBinding<bool> visibleBinding)
    {
        this.displayName = displayName;
        objectsBinding.onUpdate += objects =>
        {
            Refresh(objects);
            OnVisibilityUpdate(visibleBinding.Value);
        };

        visibleBinding.onUpdate += OnVisibilityUpdate;
    }

    private void OnVisibilityUpdate(bool isVisible)
    {
        foreach (var obj in indicatorObjects.Values)
        {
            obj.SetActive(isVisible);
        }
    }

    internal virtual void Toggle(bool isOn)
    {
        ImpUtils.ToggleGameObjects(indicatorObjects.Values, isOn);

        ImpOutput.Send(
            isOn ? $"Successfully enabled {displayName}!" : $"Successfully disabled {displayName}!",
            type: NotificationType.Confirmation
        );
    }

    protected void ClearObjects()
    {
        foreach (var indicatorObject in indicatorObjects.Values.Where(indicator => indicator != null))
        {
            Object.Destroy(indicatorObject);
        }

        indicatorObjects.Clear();
    }

    protected virtual void Refresh(T objects)
    {
    }
}
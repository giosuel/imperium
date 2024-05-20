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

    protected BaseVisualizer(string displayName)
    {
        this.displayName = displayName;
    }

    protected BaseVisualizer(string displayName, ImpBinding<T> objectsBinding)
    {
        this.displayName = displayName;
        objectsBinding.onUpdate += Refresh;
    }

    internal virtual void Toggle(bool isOn)
    {
        ImpUtils.ToggleGameObjects(indicatorObjects.Values, isOn);

        ImpOutput.Send(
            isOn ? $"Successfully enabled {displayName}!" : $"Successfully disabled {displayName}!",
            notificationType: NotificationType.Confirmation
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
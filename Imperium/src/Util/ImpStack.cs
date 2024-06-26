#region

using System.Collections.Generic;

#endregion

namespace Imperium.Util;

/// <summary>
///     Small wrapper around the List class.
///     This is used by the ImperiumUI to manage the z-index of interfaces. When an existing window is focused, that window
///     is moved to the back of the stack. Newly opened windows are added to the list.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ImpStack<T> : List<T>
{
    public void MoveToBackOrAdd(T item)
    {
        var index = IndexOf(item);
        if (index > -1)
        {
            Remove(item);
        }

        Add(item);
    }
}
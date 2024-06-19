#region

using System.Collections.Generic;

#endregion

namespace Imperium.Extensions;

public static class HashSetExtensions
{
    public static HashSet<T> Toggle<T>(this HashSet<T> set, T obj)
    {
        if (!set.Add(obj)) set.Remove(obj);
        return set;
    }
}
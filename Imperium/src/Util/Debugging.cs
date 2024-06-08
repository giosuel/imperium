#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

namespace Imperium.Util;

public static class Debugging
{
    /// <summary>
    ///     Returns an ordered list of a type's parent types in ascending order.
    ///     e.g. SpringManAI -> EnemyAI -> Component -> Object
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<Type> GetParentTypes(Type type)
    {
        var types = new List<Type>();
        var typePointer = type;
        while (typePointer != null)
        {
            types.Add(typePointer);
            typePointer = typePointer.BaseType;
        }

        return types;
    }

    /// <summary>
    ///     <see cref="GetParentTypes" />
    /// </summary>
    public static List<Type> GetParentTypes<T>() => GetParentTypes(typeof(T));

    /// <summary>
    ///     Formats the parents of a Unity transform into a string.
    ///     e.g. "ImpInterface/imperium_ui/Container/Window/Content"
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    public static string GetTransformPath(Transform root)
    {
        if (!root) return "";

        List<string> path = [];
        while (root)
        {
            path.Add(root.name);
            root = root.parent;
        }

        return path.AsEnumerable().Reverse().Aggregate((a, b) => a + "/" + b);
    }
}
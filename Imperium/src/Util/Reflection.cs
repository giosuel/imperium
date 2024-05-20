#region

using System;
using System.Collections;
using System.Reflection;

#endregion

namespace Imperium.Util;

internal static class Reflection
{
    internal static void Invoke<T>(
        T instance,
        string methodName,
        params object[] parameters
    )
    {
        typeof(T).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(instance, parameters);
    }

    internal static R Invoke<T, R>(
        T instance,
        string methodName,
        params object[] parameters
    )
    {
        var value = typeof(T)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(instance, parameters);
        if (value is not R valueCasted) throw new ArgumentOutOfRangeException();

        return valueCasted;
    }

    internal static IEnumerator GetCoroutine<T>(
        T instance,
        string methodName
    )
    {
        return (IEnumerator)typeof(T)
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(instance, []);
    }

    internal static R Get<T, R>(
        T instance,
        string fieldName,
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance
    )
    {
        var value = typeof(T)
            .GetField(fieldName, bindingFlags)!
            .GetValue(instance);

        return (R)value;
    }

    internal static void Set<T, V>(
        T instance,
        string fieldName,
        V value,
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance
    )
    {
        typeof(T).GetField(fieldName, bindingFlags)!.SetValue(instance, value);
    }

    internal static void SetProperty<T, V>(
        T instance,
        string propertyName,
        V value,
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance
    )
    {
        typeof(T).InvokeMember(propertyName, bindingFlags | BindingFlags.SetProperty, null, instance, [value]);
    }

    internal static void CopyField<T>(
        T source,
        T target,
        string fieldName,
        BindingFlags bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance
    )
    {
        var field = typeof(T).GetField(fieldName, bindingFlags)!;
        field.SetValue(target, field.GetValue(source));
    }
}
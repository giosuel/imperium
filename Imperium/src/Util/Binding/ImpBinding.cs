#region

using System;
using System.Collections.Generic;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
///     Binds a value and allows subscribers to register on change listeners.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ImpBinding<T> : IBinding<T>
{
    public event Action<T> onUpdate;
    public event Action<T> onUpdateSecondary;
    public event Action onTrigger;
    public event Action onTriggerSecondary;

    /// <summary>
    ///     If this is set to true, calls to <see cref="Refresh" /> won't invoke any events.
    /// </summary>
    private readonly bool ignoreRefresh;

    public T DefaultValue { get; }
    public T Value { get; protected set; }

    public ImpBinding()
    {
    }

    public ImpBinding(
        T currentValue = default,
        T defaultValue = default,
        Action<T> primaryUpdate = null,
        Action<T> onUpdateSecondary = null,
        bool ignoreRefresh = false
    )
    {
        Value = currentValue;
        DefaultValue = !EqualityComparer<T>.Default.Equals(defaultValue, default)
            ? defaultValue
            : currentValue;

        this.ignoreRefresh = ignoreRefresh;

        onUpdate += primaryUpdate;
        this.onUpdateSecondary += onUpdateSecondary;
    }

    public virtual void Set(T updatedValue, bool invokePrimary = true, bool invokeSecondary = true)
    {
        var isSame = EqualityComparer<T>.Default.Equals(updatedValue, Value);
        Value = updatedValue;

        if (invokePrimary)
        {
            onUpdate?.Invoke(Value);
            onTrigger?.Invoke();
        }

        if (invokeSecondary && !isSame)
        {
            onUpdateSecondary?.Invoke(updatedValue);
            onTriggerSecondary?.Invoke();
        }
    }

    public virtual void Refresh()
    {
        if (!ignoreRefresh) Set(Value);
    }

    public void Reset(bool invokePrimary = true, bool invokeSecondary = true)
    {
        Set(DefaultValue, invokePrimary, invokeSecondary);
    }
}
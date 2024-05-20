#region

using System;
using System.Collections.Generic;
using Imperium.Core;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
///     Binds a value and allows subscribers to register on change listeners.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ImpBinding<T> : IRefreshable, IResettable
{
    // Synchronize update that can be used when a binding broadcasts an update to other clients or objects
    //
    // e.g. Time update callbacks broadcast the update to all clients and each client calls their callback again,
    // meaning we need to make sure that this doesn't cause a runaway chain update on all clients
    // Therefore we put the network update into the sync update which is only called on actual user inputs and not on
    // external updates from other clients or the server.
    public event Action<T> onUpdateSync;

    // Additional update listeners registered by dependent components and objects
    public event Action<T> onUpdate;

    // Same as onUpdate, but this callback doesn't receive the new value 
    public event Action onTrigger;

    /// <summary>
    ///     Whether manual refresh (<see cref="Refresh" />) should skip callback invocation. This is useful if different
    ///     bindings have the same expensive update callback and you intend to manually call once it at the end of the
    ///     refresh instead of having every binding calling it separately.
    ///     e.g. Some render setting bindings use <see cref="PlayerManager.UpdateCameras()" /> as their callback to update
    ///     the cameras. This is an expensive function and we want to avoid calling it all the time during config loading.
    /// </summary>
    private readonly bool ignoreRefresh;

    public T DefaultValue { get; }

    public T Value { get; protected set; }

    /// <summary>
    ///     Initializes default value and value with default value
    /// </summary>
    internal ImpBinding()
    {
    }

    /// <param name="currentValue"></param>
    /// <param name="update">
    ///     <see cref="onUpdate" />
    /// </param>
    /// <param name="syncUpdate">
    ///     <see cref="onUpdateSync" />
    /// </param>
    /// <param name="ignoreRefresh">
    ///     <see cref="ignoreRefresh" />
    /// </param>
    internal ImpBinding(
        T currentValue,
        Action<T> update = null,
        Action<T> syncUpdate = null,
        bool ignoreRefresh = false
    )
    {
        Value = currentValue;
        DefaultValue = currentValue;
        this.ignoreRefresh = ignoreRefresh;
        onUpdateSync = syncUpdate;

        onUpdate += update;
    }

    /// <summary>
    ///     This constructor takes a default value that is different from the current value.
    /// </summary>
    /// <param name="currentValue"></param>
    /// <param name="defaultValue"></param>
    /// <param name="update">
    ///     <see cref="onUpdate" />
    /// </param>
    /// <param name="syncUpdate">
    ///     <see cref="onUpdateSync" />
    /// </param>
    /// <param name="ignoreRefresh">
    ///     <see cref="ignoreRefresh" />
    /// </param>
    internal ImpBinding(
        T currentValue,
        T defaultValue,
        Action<T> update = null,
        Action<T> syncUpdate = null,
        bool ignoreRefresh = false
    )
    {
        Value = currentValue;
        DefaultValue = defaultValue;
        this.ignoreRefresh = ignoreRefresh;
        onUpdateSync = syncUpdate;

        onUpdate += update;
    }

    /// <summary>
    ///     Resets the value of the bound variable to the default value
    /// </summary>
    /// <param name="skipSync">Whether the synchronize callback should not be executed</param>
    internal void Reset(bool skipSync) => Set(DefaultValue, skipSync);

    public void Refresh()
    {
        if (!ignoreRefresh) Set(Value, true);
    }

    /// <summary>
    ///     Set new value disguised as refresh so ignoreRefresh applies.
    /// </summary>
    /// <param name="newValue"></param>
    public void Refresh(T newValue)
    {
        Value = newValue;
        if (!ignoreRefresh) Set(newValue, true);
    }

    public void Reset() => Reset(false);

    internal void Set(T value) => Set(value, false);

    /// <summary>
    ///     Sets the value of the bound variable and calls all callbacks if the new value isn't equal to the current value
    /// </summary>
    /// <param name="value"></param>
    /// <param name="skipSync">Whether the synchronize callback should not be executed</param>
    internal virtual void Set(T value, bool skipSync)
    {
        var isSame = EqualityComparer<T>.Default.Equals(Value, value);
        Value = value;

        if (!skipSync && !isSame) onUpdateSync?.Invoke(value);
        onUpdate?.Invoke(value);
        onTrigger?.Invoke();
    }
}

public interface IRefreshable
{
    public void Refresh();
}

public interface IResettable
{
    public void Reset();
}
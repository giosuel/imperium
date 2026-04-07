#region

using System;

#endregion

namespace Imperium.Util.Binding;

public interface IBinding<T> : IResettable, IRefreshable
{
    /// <summary>
    ///     Primary action that is invoked every time the binding state is updated. Provides the updated state.
    ///     The binding's internal
    /// </summary>
    public event Action<T> onUpdate;

    /// <summary>
    ///     Secondary action that is invoked everytime the binding state is updated, except if explicitly skipped.
    ///     Provides the updated state.
    ///     When invoking a binding update via <see cref="Set" />, the caller can specify to skip the secondary update.
    ///     This can be useful to avoid circular updates or skip certain callbacks when invoking an update.
    /// </summary>
    public event Action<T> onUpdateSecondary;

    /// <summary>
    ///     Primary action that is invoked every time the binding state is updated. Does not provide the updated state.
    /// </summary>
    public event Action onTrigger;

    /// <summary>
    ///     Secondary action that is invoked everytime the binding state is updated, except if explicitly skipped.
    ///     Does not provide the updated state.
    ///     When invoking a binding update via <see cref="Set" />, the caller can specify to skip the secondary update.
    ///     This can be useful to avoid circular updates or skip certain callbacks when invoking an update.
    /// </summary>
    public event Action onTriggerSecondary;

    public T Value { get; }
    public T DefaultValue { get; }

    /// <summary>
    ///     Sets a new state and optionally invokes the primary and secondary events.
    /// </summary>
    /// <param name="updatedValue">The updated state</param>
    /// <param name="invokePrimary">Whether the primary update events should be invoked</param>
    /// <param name="invokeSecondary">Whether the secondary update events should be invoked</param>
    public void Set(T updatedValue, bool invokePrimary = true, bool invokeSecondary = true);

    /// <summary>
    ///     Same as calling <see cref="Set" /> with the current state and default arguments.
    /// </summary>
    public new void Refresh();

    /// <summary>
    ///     Resets the state to the default value and invokes the callbacks.
    /// </summary>
    /// <param name="invokePrimary">Whether the primary update events should be invoked</param>
    /// <param name="invokeSecondary">Whether the secondary update events should be invoked</param>
    public new void Reset(bool invokePrimary = true, bool invokeSecondary = true);
}

public interface IRefreshable
{
    public void Refresh();
}

public interface IResettable
{
    public void Reset(bool invokePrimary = true, bool invokeSecondary = true);
}
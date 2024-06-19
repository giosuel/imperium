#region

using System;

#endregion

namespace Imperium.Util.Binding;

public interface IBinding<T> : IResettable, IRefreshable
{
    /// <summary>
    ///     Action that is invoked every time the binding state is updated. Provides the updated state.
    /// </summary>
    public event Action<T> onUpdate;

    /// <summary>
    ///     Action that is invoked every time the binding state is updated. Does not provide the updated state.
    /// </summary>
    public event Action onTrigger;

    /// <summary>
    ///     Action that is invoked everytime the binding state is updated.
    ///     This action is only executed when the update function was invoked locally. This can be used to avoid
    ///     circular-updates. It can also be used to only play an SFX if the user changed something manually.
    /// </summary>
    public event Action<T> onUpdateFromLocal;

    /// <summary>
    ///     Same thing as <see cref="onUpdateFromLocal" /> but without arguments. Does not provide the updated state.
    /// </summary>
    public event Action onTriggerFromLocal;

    public T DefaultValue { get; }
    public T Value { get; }

    /// <summary>
    ///     Sets a new state and invokes the callbacks.
    /// </summary>
    /// <param name="updatedValue">The new state</param>
    /// <param name="invokeUpdate">Whether the update callbacks should be called</param>
    public void Set(T updatedValue, bool invokeUpdate = true);

    /// <summary>
    ///     Invokes the callbacks with the current state.
    /// </summary>
    public new void Refresh();

    /// <summary>
    ///     Resets the state to the default value and invokes the callbacks.
    /// </summary>
    /// <param name="invokeUpdate">Whether the update callbacks should be called</param>
    public new void Reset(bool invokeUpdate = true);
}

public interface IRefreshable
{
    public void Refresh();
}

public interface IResettable
{
    public void Reset(bool invokeUpdate = true);
}
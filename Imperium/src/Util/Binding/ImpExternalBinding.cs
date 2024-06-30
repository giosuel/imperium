#region

using System;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
///     Intermediate binding for when the value can't be easily bound due to external changes
///     Whenever the refresher binding is updated, the child refreshes it's value based on the provided getter function.
///     E.g This can be used when the binding depends on a game objects availability that depends on if the ship has landed
///     or is in space. The getter in this case is GameObject.Find and the parent is <see cref="Imperium.IsSceneLoaded" />
/// </summary>
/// <typeparam name="T">Type of the binding and getter return value</typeparam>
/// <typeparam name="R">Type of the parent binding</typeparam>
public class ImpExternalBinding<T, R> : ImpBinding<T>
{
    private readonly Func<T> valueGetter;

    /// <param name="valueGetter">Getter function that returns the value</param>
    /// <param name="refresher">ImpBinding that the binder is listening to</param>
    /// <param name="onUpdate">
    ///     <see cref="ImpBinding{T}.onUpdate" />
    /// </param>
    /// <param name="fromLocalUpdate">
    ///     <see cref="ImpBinding{T}.onUpdateFromLocal" />
    /// </param>
    public ImpExternalBinding(
        Func<T> valueGetter,
        IBinding<R> refresher = null,
        Action<T> onUpdate = null,
        Action<T> fromLocalUpdate = null
    ) : base(ImpUtils.InvokeDefaultOnNull(valueGetter), onUpdate: onUpdate, onUpdateFromLocal: fromLocalUpdate)
    {
        this.valueGetter = valueGetter;
        if (refresher != null) refresher.onUpdate += _ => Set(ImpUtils.InvokeDefaultOnNull(valueGetter));
    }

    public override void Set(T updatedValue, bool invokeUpdate = true)
    {
        base.Set(ImpUtils.InvokeDefaultOnNull(valueGetter), invokeUpdate);
    }
}
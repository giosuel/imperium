#region

using System;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
///     Intermediate binding for when the a value can't be easily bound due to external changes
///     Whenever the refresher binding is updated, the child refreshes it's value based on the provided getter function.
///     E.g This can be used when the binding depends on a game objects availability that depends on if the ship has landed
///     or is in space. The getter in this case is GameObject.Find and the parent is <see cref="Imperium.IsSceneLoaded" />
/// </summary>
/// <typeparam name="T">Type of the binding and getter return value</typeparam>
/// <typeparam name="R">Type of the parent binding</typeparam>
public class ImpExternalBinding<T, R> : ImpBinding<T>
{
    /// <param name="valueGetter">Getter function that returns the value</param>
    /// <param name="refresher">ImpBinding that the binder is listening to</param>
    /// <param name="onUpdate">
    ///     <see cref="ImpBinding{T}.onUpdate" />
    /// </param>
    /// <param name="syncUpdate">
    ///     <see cref="ImpBinding{T}.onUpdateSync" />
    /// </param>
    internal ImpExternalBinding(
        Func<T> valueGetter,
        ImpBinding<R> refresher,
        Action<T> onUpdate = null,
        Action<T> syncUpdate = null
    ) : base(ImpUtils.InvokeDefaultOnNull(valueGetter), onUpdate, syncUpdate)
    {
        refresher.onUpdate += _ => Set(ImpUtils.InvokeDefaultOnNull(valueGetter));
    }
}
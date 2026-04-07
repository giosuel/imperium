#region

using System;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
///     A ImpBinding that can eb subscribed to, but cannot be updated.
///     Imperium uses this in the API, to make sure that clients can't update internal values.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ImpImmutableBinding<T> : IBinding<T>
{
    private readonly IBinding<T> parentWritableBinding;

    public T Value => parentWritableBinding.Value;
    public T DefaultValue => parentWritableBinding.DefaultValue;

    public event Action<T> onUpdate;
    public event Action onTrigger;

    public event Action<T> onUpdateSecondary;
    public event Action onTriggerSecondary;

    public static ImpImmutableBinding<T> Wrap(IBinding<T> parent) => new(parent);

    private ImpImmutableBinding(IBinding<T> parentWritableBinding)
    {
        this.parentWritableBinding = parentWritableBinding;
        parentWritableBinding.onUpdate += value => onUpdate?.Invoke(value);
        parentWritableBinding.onTrigger += () => onTrigger?.Invoke();

        parentWritableBinding.onUpdateSecondary += value => onUpdateSecondary?.Invoke(value);
        parentWritableBinding.onTriggerSecondary += () => onTriggerSecondary?.Invoke();
    }

    public void Set(T updatedValue, bool invokePrimary = true, bool invokeSecondary = true)
    {
    }

    public void Refresh()
    {
    }

    public void Reset(bool invokePrimary, bool invokeSecondary)
    {
    }
}
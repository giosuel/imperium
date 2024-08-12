#region

using System;

#endregion

namespace Imperium.Util.Binding;

/// <summary>
/// A binding that canot be updated. Used by the API to make sure clients can't update the internal values.
/// </summary>
/// <typeparam name="T"></typeparam>
public class ReadOnlyBinding<T>
{
    private readonly IBinding<T> parentWritableBinding;

    public T DefaultValue => parentWritableBinding.Value;
    public T Value => parentWritableBinding.Value;

    public event Action<T> onUpdate;
    public event Action onTrigger;

    public event Action<T> onUpdateFromLocal;
    public event Action onTriggerFromLocal;

    private ReadOnlyBinding(IBinding<T> parentWritableBinding)
    {
        this.parentWritableBinding = parentWritableBinding;
        parentWritableBinding.onTrigger += () => onTrigger?.Invoke();
        parentWritableBinding.onUpdate += value => onUpdate?.Invoke(value);

        parentWritableBinding.onTriggerFromLocal += () => onTriggerFromLocal?.Invoke();
        parentWritableBinding.onUpdateFromLocal += value => onUpdateFromLocal?.Invoke(value);
    }

    public static ReadOnlyBinding<T> Wrap(IBinding<T> parent) => new(parent);
}
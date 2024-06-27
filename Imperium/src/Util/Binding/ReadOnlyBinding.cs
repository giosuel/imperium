#region

using System;

#endregion

namespace Imperium.Util.Binding;

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

// public class ReadOnlyCollectionBinding<R>
// {
//     private readonly IBinding<ICollection<R>> parentWritableBinding;
//
//     public IReadOnlyCollection<R> Value => parentWritableBinding.Value.ToList().AsReadOnly();
//
//     public event Action<IReadOnlyCollection<R>> onUpdate;
//     public event Action onTrigger;
//
//     public event Action<IReadOnlyCollection<R>> onUpdateFromLocal;
//     public event Action onTriggerFromLocal;
//
//     private ReadOnlyCollectionBinding(IBinding<ICollection<R>> parentWritableBinding)
//     {
//         this.parentWritableBinding = parentWritableBinding;
//         parentWritableBinding.onTrigger += () => onTrigger?.Invoke();
//         parentWritableBinding.onUpdate += value => onUpdate?.Invoke(value.ToList().AsReadOnly());
//
//         parentWritableBinding.onTriggerFromLocal += () => onTriggerFromLocal?.Invoke();
//         parentWritableBinding.onUpdateFromLocal += value => onUpdateFromLocal?.Invoke(value.ToList().AsReadOnly());
//     }
//
//     public static ReadOnlyCollectionBinding<R> Wrap(IBinding<HashSet<R>> parent) => new(parent);
// }
#region

using System;
using System.Linq;

#endregion

namespace Imperium.Util.Binding;

public class ImpBinaryBinding : ImpBinding<bool>
{
    public event Action onTrue;
    public event Action onFalse;

    public ImpBinaryBinding(
        bool currentValue,
        Action onTrue = null,
        Action onFalse = null
    ) : base(currentValue)
    {
        this.onTrue += onTrue;
        this.onFalse += onFalse;

        onUpdate += OnUpdate;
    }

    public void Toggle() => Set(!Value);
    public void SetTrue() => Set(true);
    public void SetFalse() => Set(false);

    private void OnUpdate(bool updatedValue)
    {
        if (updatedValue)
        {
            onTrue?.Invoke();
        }
        else
        {
            onFalse?.Invoke();
        }
    }

    /// <summary>
    ///     Creates a combined binary binding from two or more provided bindings.
    ///     The new binding's value is true if any of the source bindings' values is true.
    /// </summary>
    /// <param name="bindingPairs">A list of bindings to combine</param>
    public static ImpBinaryBinding CreateAnd((IBinding<bool>, bool)[] bindingPairs)
    {
        var combinedBinding = new ImpBinaryBinding(GetCombinedAndValue(bindingPairs));
        foreach (var bindingPair in bindingPairs)
        {
            bindingPair.Item1.onTrigger += () => combinedBinding.Set(GetCombinedAndValue(bindingPairs));
        }

        return combinedBinding;

        bool GetCombinedAndValue((IBinding<bool>, bool)[] pairs) => pairs.Aggregate(
            true, (combined, current) => combined && current.Item2 ^ current.Item1.Value
        );
    }

    /// <summary>
    ///     Creates a combined binary binding from two or more provided bindings.
    ///     The new binding's value is true if all the source bindings' values are true.
    /// </summary>
    /// <param name="bindingPairs">A list of bindings to combine</param>
    public static ImpBinaryBinding CreateOr((IBinding<bool>, bool)[] bindingPairs)
    {
        var combinedBinding = new ImpBinaryBinding(GetCombinedOrValue(bindingPairs));
        foreach (var bindingPair in bindingPairs)
        {
            bindingPair.Item1.onTrigger += () => combinedBinding.Set(GetCombinedOrValue(bindingPairs));
        }

        return combinedBinding;

        bool GetCombinedOrValue((IBinding<bool>, bool)[] pairs) => pairs.Aggregate(
            false, (combined, current) => combined || current.Item2 ^ current.Item1.Value
        );
    }
}
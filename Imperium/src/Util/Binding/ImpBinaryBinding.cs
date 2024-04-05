#region

using System;

#endregion

namespace Imperium.Util.Binding;

public class ImpBinaryBinding : ImpBinding<bool>
{
    internal event Action onTrue;
    internal event Action onFalse;

    internal ImpBinaryBinding(
        bool currentValue,
        Action onTrue = null,
        Action onFalse = null
    ) : base(currentValue)
    {
        this.onTrue += onTrue;
        this.onFalse += onFalse;

        onUpdate += OnUpdate;
    }

    internal void Toggle() => Set(!Value);
    internal void SetTrue() => Set(true);
    internal void SetFalse() => Set(false);

    private void OnUpdate(bool value)
    {
        if (value)
        {
            onTrue?.Invoke();
        }
        else
        {
            onFalse?.Invoke();
        }
    }
}
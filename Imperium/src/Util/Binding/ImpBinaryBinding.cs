#region

using System;

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
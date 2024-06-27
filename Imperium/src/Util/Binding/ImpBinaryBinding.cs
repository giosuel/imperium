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
    public void SetTrue()
    {
        Imperium.IO.LogInfo("BINARY BINDING UPDATE ONTRUE");
        Set(true);
    }

    public void SetFalse()
    {
        Imperium.IO.LogInfo("BINARY BINDING UPDATE ONFALSE");
        Set(false);
    }

    private void OnUpdate(bool updatedValue)
    {
        Imperium.IO.LogInfo("BINARY BINDING UPDATE");
        if (updatedValue)
        {
            onTrue?.Invoke();
        }
        else
        {
            onFalse?.Invoke();
        }
    }
}
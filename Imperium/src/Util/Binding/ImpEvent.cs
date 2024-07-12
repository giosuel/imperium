using System;

namespace Imperium.Util.Binding;

public class ImpEvent
{
    public event Action onTrigger;

    public void Trigger() => onTrigger?.Invoke();
}
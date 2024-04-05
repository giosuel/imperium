#region

using System;

#endregion

namespace Imperium.Util.Binding;

internal class ImpEvent
{
    internal event Action onTrigger;

    internal ImpEvent(Action onTrigger = null)
    {
        this.onTrigger += onTrigger;
    }

    internal void Trigger() => onTrigger?.Invoke();
}
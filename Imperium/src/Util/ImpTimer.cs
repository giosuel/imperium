#region

using UnityEngine;

#endregion

namespace Imperium.Util;

internal class ImpTimer
{
    private float initialTime;
    private float countdown;

    private ImpTimer()
    {
    }

    internal static ImpTimer ForInterval(float seconds)
    {
        var timer = new ImpTimer
        {
            initialTime = seconds,
            countdown = seconds
        };
        return timer;
    }

    internal void Set(float newTime)
    {
        initialTime = newTime;
    }

    internal bool Tick()
    {
        countdown -= Time.deltaTime;
        if (countdown <= 0)
        {
            countdown = initialTime;
            return true;
        }

        return false;
    }

    internal void Reset() => countdown = initialTime;
}
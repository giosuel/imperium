using System;
using UnityEngine;

namespace Imperium.API;

public static class Utils
{
    /// <summary>
    /// Activates the position indicator.
    /// </summary>
    /// <param name="callback">The callback that is executed when a position has been picked.</param>
    /// <param name="parent">The transform the indicator origin will be parented to. Default: Player Camera</param>
    /// <param name="castToGround">Whether the raycast should only hit ground surfaces.</param>
    public static void ActivateIndicator(Action<Vector3> callback, Transform parent = null, bool castToGround = true)
    {
        APIHelpers.AssertImperiumReady();

        Imperium.ImpPositionIndicator.Activate(callback, parent, castToGround);
    }

    /// <summary>
    /// Deactivates the position indicator.
    /// </summary>
    public static void DeactivateIndicator()
    {
        APIHelpers.AssertImperiumReady();

        Imperium.ImpPositionIndicator.Deactivate();
    }
}
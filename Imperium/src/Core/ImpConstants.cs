#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Imperium.Core;

internal abstract class ImpConstants
{
    internal abstract class Opacity
    {
        internal const float Enabled = 1;
        internal const float TextDisabled = 0.1f;
        internal const float ImageDisabled = 0.3f;
    }

    internal const int DefaultFOV = 66;
    internal const float DefaultMovementSpeed = 4.6f;
    internal const float DefaultJumpForce = 13f;
    internal const float DefaultTimeSpeed = 1.4f;

    internal const int ShotgunCollisionCount = 10;
    internal const float ShotgunDefaultCooldown = 0.7f;

    internal const float DefaultMapCameraScale = 19.7f;

    internal const int DefaultMapCameraFarClip = 50;
    internal const int DefaultMapCameraNearClip = 9;

    internal const int DefaultMapCameraFarClipFreeLook = 200;
    internal const int DefaultMapCameraNearClipFreeLook = -20;

    internal const string GeneralSaveFile = "LCGeneralSaveData";

    internal static readonly LayerMask IndicatorMask = LayerMask.GetMask("Room", "Terrain", "Railing");

    internal static readonly string[] MoonWeathers =
    [
        "None",
        "Dust Clouds",
        "Rainy",
        "Stormy",
        "Foggy",
        "Flooded",
        "Eclipsed"
    ];

    // Items that have no spawn prefab
    public static readonly HashSet<string> ItemBlacklist = ["box"];
}
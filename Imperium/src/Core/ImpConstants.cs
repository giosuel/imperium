#region

using System.Collections.Generic;
using GameNetcodeStuff;
using UnityEngine;

#endregion

namespace Imperium.Core;

public abstract class ImpConstants
{
    internal abstract class Opacity
    {
        internal const float Enabled = 1;
        internal const float TextDisabled = 0.1f;
        internal const float ImageDisabled = 0.3f;
    }

    public const int DefaultFOV = 66;
    public const float DefaultMovementSpeed = 4.6f;
    public const float DefaultJumpForce = 13f;
    public const float DefaultTimeSpeed = 1.4f;

    internal const int ShotgunCollisionCount = 10;
    internal const float ShotgunDefaultCooldown = 0.7f;

    internal const float DefaultMapCameraScale = 19.7f;

    internal const int DefaultMapCameraFarClip = 50;
    internal const int DefaultMapCameraNearClip = 9;

    internal const int DefaultMapCameraFarClipFreeLook = 200;
    internal const int DefaultMapCameraNearClipFreeLook = -20;

    internal const float DefaultCarPushForceMultiplier = 27;

    internal const string GeneralSaveFile = "LCGeneralSaveData";

    internal static readonly LayerMask IndicatorMask = LayerMask.GetMask("Room", "Terrain", "Railing");

    /*
     * Maps the class names of default insights to more recognizable names.
     */
    internal static readonly Dictionary<string, string> ClassNameMap = new()
    {
        { nameof(PlayerControllerB), "Players" },
        { nameof(EnemyAI), "Enemies" },
        { nameof(GrabbableObject), "Items" },
        { nameof(Turret), "Turrets" },
        { nameof(Landmine), "Landmines" },
        { nameof(SteamValveHazard), "Steam Valves" },
        { nameof(BridgeTrigger), "Bridges" },
        { nameof(VehicleController), "Company Cruiser" }
    };

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
#region

using UnityEngine;

#endregion

namespace Imperium.Util;

/// <summary>
///     Layers of animators, and hashes of animations names.
/// </summary>
internal static class ImpAnimatorHash
{
    internal static class Metarig
    {
        internal const string Layer_HoldingItemsBothHands = "HoldingItemsBothHands";
        internal static readonly int ShovelReelUp = Animator.StringToHash("ShovelReelUp");
        internal static readonly int HitShovel = Animator.StringToHash("HitShovel");
    }

    /// <summary>
    ///     Internally named 'Models1.controller'.
    ///     Assigned to SampleSceneRelay/Environment/HangarShip animator.
    ///     Contains a single Base Layer.
    /// </summary>
    internal static class ShipAnimator
    {
        /// <summary>
        ///     The default state.
        ///     The <see cref="Terminal" /> and <see cref="ShipBuildModeManager" />
        ///     require ship animator to be in this state in order to use them.
        /// </summary>
        internal static readonly int ShipIdle = Animator.StringToHash("ShipIdle");

        /// <summary>
        ///     Has exit time into ShipFlyToPlanet.
        /// </summary>
        internal static readonly int ShipFlyAwayFromPlanet = Animator.StringToHash("ShipFlyAwayFromPlanet");

        /// <summary>
        ///     Has exit time into ShipIdle.
        /// </summary>
        internal static readonly int ShipFlyToPlanet = Animator.StringToHash("ShipFlyToPlanet");

        /// <summary>
        ///     Has exit time into ShipIdleLanded.
        /// </summary>
        internal static readonly int ShipOpen = Animator.StringToHash("ShipOpen");

        /// <summary>
        ///     Has exit time into ShipIdle.
        /// </summary>
        internal static readonly int ShipLeave = Animator.StringToHash("ShipLeave");
    }
}

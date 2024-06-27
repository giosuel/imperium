#region

using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.API.Types.Networking;
using Imperium.Netcode;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerPatch
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PreloadPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        private static void ConnectClientToPlayerObjectPatch(PlayerControllerB __instance)
        {
            if (GameNetworkManager.Instance.localPlayerController != __instance) return;
            Imperium.Player = GameNetworkManager.Instance.localPlayerController;

            Imperium.Networking = new ImpNetworking();
            Imperium.Networking.RequestImperiumAccess();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("DamagePlayer")]
    private static void DamagePlayerPatch(PlayerControllerB __instance, int damageNumber, CauseOfDeath causeOfDeath)
    {
        if (Imperium.Settings.Player.GodMode.Value)
        {
            // Fixes that every following jump will cause the player to take fall damage
            __instance.takingFallDamage = false;
            __instance.criticallyInjured = false;
            __instance.health = 100;

            Imperium.IO.Send(
                $"God mode negated {damageNumber} damage from '{(causeOfDeath).ToString()}'",
                type: NotificationType.GodMode
            );
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("KillPlayer")]
    private static void KillPlayerPatch(PlayerControllerB __instance, CauseOfDeath causeOfDeath)
    {
        if (Imperium.Settings.Player.GodMode.Value)
        {
            Imperium.IO.Send($"God mode saved you from death by '{(causeOfDeath).ToString()}'",
                type: NotificationType.GodMode);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("AllowPlayerDeath")]
    private static bool AllowPlayerDeathPatch(PlayerControllerB __instance, ref bool __result)
    {
        // Internal override for Object Explorer kill functionality that ignores god mode
        if (Imperium.PlayerManager.AllowPlayerDeathOverride)
        {
            __result = true;
            return false;
        }

        if (Imperium.Settings.Player.GodMode.Value)
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("KillPlayerClientRpc")]
    private static void KillPlayerClientRpc(PlayerControllerB __instance, int playerId)
    {
        Imperium.IO.Send($"Employee {__instance.playerUsername} has died!", type: NotificationType.Other);
    }

    [HarmonyPrefix]
    [HarmonyPatch("PlayFootstepLocal")]
    private static bool PlayFootstepLocalPatch(PlayerControllerB __instance)
    {
        return !Imperium.Settings.Player.Muted.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SpawnPlayerAnimation")]
    private static bool SpawnPlayerAnimationPatch(PlayerControllerB __instance)
    {
        return !Imperium.Settings.AnimationSkipping.PlayerSpawn.Value;
    }

    #region Interact Triggers

    private static readonly Dictionary<int, bool> OriginalTriggerHold = [];

    [HarmonyPrefix]
    [HarmonyPatch("ClickHoldInteraction")]
    private static void ClickHoldInteractionPrefixPatch(PlayerControllerB __instance)
    {
        if (!__instance.hoveringOverTrigger) return;

        if (Imperium.Settings.AnimationSkipping.InteractHold.Value)
        {
            // Backup original hold
            if (!OriginalTriggerHold.ContainsKey(__instance.hoveringOverTrigger.GetInstanceID()))
            {
                OriginalTriggerHold[__instance.hoveringOverTrigger.GetInstanceID()] =
                    __instance.hoveringOverTrigger.holdInteraction;
            }

            __instance.hoveringOverTrigger.holdInteraction = false;
        }
        else
        {
            // Restore original hold if it has been changed before
            if (OriginalTriggerHold.TryGetValue(__instance.hoveringOverTrigger.GetInstanceID(), out var originalHold))
            {
                __instance.hoveringOverTrigger.holdInteraction = originalHold;
            }
        }
    }

    #endregion

    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int Sprinting = Animator.StringToHash("Sprinting");
    private static readonly int Sideways = Animator.StringToHash("Sideways");
    private static readonly int Crouching = Animator.StringToHash("crouching");
    private static readonly int Jumping = Animator.StringToHash("Jumping");

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    private static void UpdatePrefixPatch(PlayerControllerB __instance)
    {
        if (Imperium.PlayerManager.IsFlying.Value)
        {
            if (!Imperium.Player.quickMenuManager.isMenuOpen
                && !Imperium.Player.inTerminalMenu
                && !Imperium.Player.isTypingChat
                && !Imperium.Player.isClimbingLadder
                && !Imperium.Player.inSpecialInteractAnimation
                && !Imperium.Player.jetpackControls)
            {
                var moveVector = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move").ReadValue<Vector2>();
                var upInput = Imperium.PlayerManager.FlyIsAscending
                    ? IngamePlayerSettings.Instance.playerInput.actions.FindAction("Jump").ReadValue<float>()
                    : 0;
                var downInput = Imperium.PlayerManager.FlyIsDescending
                    ? IngamePlayerSettings.Instance.playerInput.actions.FindAction("Crouch").ReadValue<float>()
                    : 0;

                upInput *= 15f;
                downInput *= 15f;

                var forceVector = new Vector3(moveVector.x * 10f, upInput - downInput, moveVector.y * 10f);
                forceVector = Quaternion.AngleAxis(Imperium.Player.transform.eulerAngles.y, Vector3.up) * forceVector;
                __instance.externalForces += forceVector;
            }

            __instance.fallValue = 0;
            __instance.fallValueUncapped = 0;

            __instance.playerBodyAnimator.SetBool(Walking, value: false);
            __instance.playerBodyAnimator.SetBool(Sprinting, value: false);
            __instance.playerBodyAnimator.SetBool(Sideways, value: false);
            __instance.playerBodyAnimator.SetBool(Crouching, value: false);
            __instance.playerBodyAnimator.SetBool(Jumping, value: true);
            __instance.isCrouching = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePostfixPatch(PlayerControllerB __instance)
    {
        if (Imperium.Settings.Player.Permadrunk.Value) __instance.drunkness = 3;
        if (Imperium.Settings.Player.InfiniteSprint.Value) __instance.sprintMeter = 1;

        // Make player invincible to animation locking
        if (Imperium.Settings.Player.DisableLocking.Value)
        {
            __instance.snapToServerPosition = false;
            __instance.inSpecialInteractAnimation = false;
        }

        // Apply custom FOV
        if (Imperium.Settings.Player.CustomFieldOfView.Value > -1)
        {
            var targetFOV = Imperium.Settings.Player.CustomFieldOfView.Value;
            if (__instance.inTerminalMenu) targetFOV -= 6;
            if (__instance.IsInspectingItem) targetFOV -= 14;
            if (__instance.isSprinting) targetFOV += 2f;

            __instance.gameplayCamera.fieldOfView = Mathf.Lerp(
                __instance.gameplayCamera.fieldOfView,
                targetFOV,
                6f * Time.deltaTime
            );
        }
    }

    // Temporarily stores gameHasStarted if patch overwrites it for pickup check
    private static bool gameHasStartedBridge;

    [HarmonyPrefix]
    [HarmonyPatch("BeginGrabObject")]
    private static void BeginGrabObjectPrefixPatch(PlayerControllerB __instance)
    {
        gameHasStartedBridge = GameNetworkManager.Instance.gameHasStarted;
        if (Imperium.Settings.Player.PickupOverwrite.Value) GameNetworkManager.Instance.gameHasStarted = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("BeginGrabObject")]
    private static void BeginGrabObjectPostfixPatch(PlayerControllerB __instance)
    {
        GameNetworkManager.Instance.gameHasStarted = gameHasStartedBridge;
    }

    ////////////////////////////////////////////////////////////////////////////////////
    /// The following patches are blocking native input when an Imperium UI or the freecam is open
    /// I tried to make use the existing variable isMenuOpen and isFreeCamera as much as possible
    /// but some actions are still performed (e.g. Crouch, Drop Item, etc.).
    ////////////////////////////////////////////////////////////////////////////////////

    #region NativeInputHandlerPatches

    [HarmonyPrefix]
    [HarmonyPatch("Discard_performed")]
    private static bool Discard_performedPatch(PlayerControllerB __instance)
    {
        return !__instance.quickMenuManager.isMenuOpen && !__instance.isFreeCamera;
    }

    [HarmonyPrefix]
    [HarmonyPatch("ScrollMouse_performed")]
    private static bool ScrollMouse_performedPatch(PlayerControllerB __instance)
    {
        return !__instance.quickMenuManager.isMenuOpen && !__instance.isFreeCamera;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Jump_performed")]
    private static bool Jump_performedPatch(PlayerControllerB __instance)
    {
        return !__instance.quickMenuManager.isMenuOpen
               && !__instance.isFreeCamera
               && !Imperium.PlayerManager.IsFlying.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Crouch_performed")]
    private static bool Crouch_performedPatch(PlayerControllerB __instance)
    {
        return !__instance.quickMenuManager.isMenuOpen
               && !__instance.isFreeCamera
               && !Imperium.PlayerManager.IsFlying.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("ActivateItem_performed")]
    private static bool ActivateItem_performedPatch(PlayerControllerB __instance)
    {
        return !__instance.quickMenuManager.isMenuOpen && !__instance.isFreeCamera;
    }

    [HarmonyPrefix]
    [HarmonyPatch("InspectItem_performed")]
    private static bool InspectItem_performedPatch(PlayerControllerB __instance)
    {
        return !__instance.quickMenuManager.isMenuOpen && !__instance.isFreeCamera;
    }

    #endregion
}
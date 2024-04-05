#region

using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Core;
using Imperium.Netcode;
using Imperium.Util;
using Unity.Netcode;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(PlayerControllerB))]
internal static class PlayerControllerPatch
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal static class PreloadPatches
    {
        /// <summary>
        /// This is used as the entry function for Imperium
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch("ConnectClientToPlayerObject")]
        private static void ConnectClientToPlayerObjectPatch(PlayerControllerB __instance)
        {
            if (!Imperium.IsImperiumReady || GameNetworkManager.Instance.localPlayerController != __instance) return;
            Imperium.Player = __instance;
            ImpNetCommunication.Instance.RequestImperiumAccessServerRpc(NetworkManager.Singleton.LocalClientId);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePatch(PlayerControllerB __instance)
    {
        if (ImpSettings.Player.InfiniteSprint.Value) __instance.sprintMeter = 1;
    }

    [HarmonyPostfix]
    [HarmonyPatch("DamagePlayer")]
    private static void DamagePlayerPatch(PlayerControllerB __instance, int damageNumber, CauseOfDeath causeOfDeath)
    {
        if (ImpSettings.Player.GodMode.Value)
        {
            // Fixes that every following jump will cause the player to take fall damage
            __instance.takingFallDamage = false;
            __instance.criticallyInjured = false;
            __instance.health = 100;

            ImpOutput.Send(
                $"God mode negated {damageNumber} damage from '{(causeOfDeath).ToString()}'",
                notificationType: NotificationType.GodMode
            );
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("KillPlayer")]
    private static void KillPlayerPatch(PlayerControllerB __instance, CauseOfDeath causeOfDeath)
    {
        if (ImpSettings.Player.GodMode.Value)
        {
            ImpOutput.Send($"God mode saved you from death by '{(causeOfDeath).ToString()}'",
                notificationType: NotificationType.GodMode);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("AllowPlayerDeath")]
    private static bool AllowPlayerDeathPatch(PlayerControllerB __instance, ref bool __result)
    {
        if (Imperium.PlayerManager.AllowPlayerDeathOverride)
        {
            __result = true;
            return false;
        }

        if (ImpSettings.Player.GodMode.Value)
        {
            __result = false;
            return false;
        }

        __result = true;
        return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("KillPlayerClientRpc")]
    private static void KillPlayerClientRpc(PlayerControllerB __instance, int playerId)
    {
        ImpOutput.Send($"Employee {__instance.playerUsername} has died!", notificationType: NotificationType.Other);
    }

    [HarmonyPrefix]
    [HarmonyPatch("PlayFootstepLocal")]
    private static bool PlayFootstepLocalPatch(PlayerControllerB __instance)
    {
        return !ImpSettings.Player.Muted.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePostfixPatch(PlayerControllerB __instance)
    {
        // Make player invincible to animation locking
        if (ImpSettings.Player.DisableLocking.Value)
        {
            __instance.snapToServerPosition = false;
            __instance.inSpecialInteractAnimation = false;
        }

        if (ImpSettings.Player.CustomFieldOfView.Value < 0) return;

        var targetFOV = ImpSettings.Player.CustomFieldOfView.Value;
        if (__instance.isSprinting) targetFOV += 2;
        __instance.gameplayCamera.fieldOfView = targetFOV;
    }

    // Temporarily stores gameHasStarted if patch overwrites it for pickup check
    private static bool gameHasStartedBridge;

    [HarmonyPrefix]
    [HarmonyPatch("BeginGrabObject")]
    private static void BeginGrabObjectPrefixPatch(PlayerControllerB __instance)
    {
        gameHasStartedBridge = GameNetworkManager.Instance.gameHasStarted;
        if (ImpSettings.Player.PickupOverwrite.Value) GameNetworkManager.Instance.gameHasStarted = true;
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
        return !__instance.quickMenuManager.isMenuOpen && !__instance.isFreeCamera;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Crouch_performed")]
    private static bool Crouch_performedPatch(PlayerControllerB __instance)
    {
        return !__instance.quickMenuManager.isMenuOpen && !__instance.isFreeCamera;
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
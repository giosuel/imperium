#region

using System.Collections.Generic;
using HarmonyLib;
using Imperium.API.Types.Networking;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatch
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class PreloadPatches
    {
        // /// <summary>
        // ///     This is used as the entry function for Imperium
        // /// </summary>
        // [HarmonyPrefix]
        // [HarmonyPatch(typeof(StartOfRound), "Awake")]
        // private static void ConnectClientToPlayerObjectPatch(StartOfRound __instance)
        // {
        //     if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
        //     {
        //         Object.Instantiate(ImpNetworkManager.NetworkPrefab).GetComponent<NetworkObject>().Spawn();
        //     }
        // }
    }

    [HarmonyPrefix]
    [HarmonyPatch("StartGame")]
    private static void StartGamePrefixPatch(StartOfRound __instance)
    {
        if (Imperium.Settings.Ship.InstantLanding.Value) __instance.shipAnimator.enabled = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("ShipLeave")]
    private static void ShipLeavePrefixPatch(StartOfRound __instance)
    {
        if (Imperium.Settings.Ship.InstantTakeoff.Value) __instance.shipAnimator.enabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ShipLeave")]
    private static void ShipLeavePostfixPatch(StartOfRound __instance)
    {
        if (Imperium.Settings.Ship.InstantTakeoff.Value)
        {
            Object.FindObjectOfType<ElevatorAnimationEvents>().ElevatorFullyRunning();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("TeleportPlayerInShipIfOutOfRoomBounds")]
    private static bool TeleportPlayerInShipIfOutOfRoomBoundsPatch()
    {
        return !Imperium.Settings.Player.DisableOOB.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch("EndOfGame")]
    private static void EndOfGamePostfixPatch(StartOfRound __instance)
    {
        Imperium.IsSceneLoaded.SetFalse();
    }

    [HarmonyPrefix]
    [HarmonyPatch("ShipLeaveAutomatically")]
    private static bool ShipLeaveAutomaticallyPatch(StartOfRound __instance)
    {
        if (Imperium.Settings.Ship.PreventLeave.Value)
        {
            // We have to revert this
            __instance.allPlayersDead = false;

            Imperium.IO.Send("Prevented the ship from leaving.", type: NotificationType.Other);
            Imperium.IO.LogInfo("[MON] Prevented the ship from leaving.");
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ChooseNewRandomMapSeed")]
    private static void ChooseNewRandomMapSeedPatch(StartOfRound __instance)
    {
        if (Imperium.GameManager.CustomSeed.Value != -1)
        {
            __instance.randomMapSeed = Imperium.GameManager.CustomSeed.Value;
        }
    }

    internal static readonly Harmony InstantLandingHarmony = new(Imperium.PLUGIN_GUID + ".InstantLanding");
    internal static readonly Harmony InstantTakeoffHarmony = new(Imperium.PLUGIN_GUID + ".InstantTakeoff");

    internal static class InstantLandingPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence", MethodType.Enumerator)]
        private static IEnumerable<CodeInstruction> openingDoorsSequenceTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            return ImpUtils.Transpiling.SkipWaitingForSeconds(instructions);
        }
    }

    internal static class InstantTakeoffPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(StartOfRound), "EndOfGame", MethodType.Enumerator)]
        private static IEnumerable<CodeInstruction> EndOfGameTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            return ImpUtils.Transpiling.SkipWaitingForSeconds(instructions);
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(RoundManager), "DetectElevatorRunning", MethodType.Enumerator)]
        private static IEnumerable<CodeInstruction> DetectElevatorRunningTranspiler(
            IEnumerable<CodeInstruction> instructions
        )
        {
            return ImpUtils.Transpiling.SkipWaitingForSeconds(instructions);
        }
    }
}
#region

using System.Collections.Generic;
using HarmonyLib;
using Imperium.API.Types.Networking;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("StartGame")]
    private static void StartGamePrefixPatch(StartOfRound __instance)
    {
        __instance.shipAnimator.gameObject.GetComponent<PlayAudioAnimationEvent>().audioToPlay.mute = true;
        __instance.shipAnimator.gameObject.GetComponent<PlayAudioAnimationEvent>().audioToPlayB.mute = true;
        __instance.shipAnimator.speed = Imperium.ShipManager.InstantLanding.Value ? 1000f : 1;
    }

    [HarmonyPrefix]
    [HarmonyPatch("ShipLeave")]
    private static void ShipLeavePrefixPatch(StartOfRound __instance)
    {
        __instance.shipAnimator.gameObject.GetComponent<PlayAudioAnimationEvent>().audioToPlay.mute = true;
        __instance.shipAnimator.gameObject.GetComponent<PlayAudioAnimationEvent>().audioToPlayB.mute = true;
        __instance.shipAnimator.speed = Imperium.ShipManager.InstantTakeoff.Value ? 1000f : 1;
    }

    [HarmonyPostfix]
    [HarmonyPatch("openingDoorsSequence")]
    private static void openingDoorsSequencePostfixPatch(StartOfRound __instance)
    {
        // Reset ship animator
        __instance.shipAnimator.gameObject.GetComponent<PlayAudioAnimationEvent>().audioToPlay.mute = false;
        __instance.shipAnimator.gameObject.GetComponent<PlayAudioAnimationEvent>().audioToPlayB.mute = false;
        __instance.shipAnimator.speed = 1;
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
        if (Imperium.ShipManager.PreventShipLeave.Value)
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
            IEnumerable<CodeInstruction> instructions
        )
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
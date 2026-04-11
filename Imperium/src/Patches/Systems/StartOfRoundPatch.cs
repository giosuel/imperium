#region

using System.Collections;
using HarmonyLib;
using Imperium.API.Types.Networking;
using Imperium.Netcode;
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

    internal static IEnumerator SkipShipAnimatorIf(IEnumerator result, ImpNetworkBinding<bool> condition)
    {
        if (condition.Value)
        {
            Imperium.StartOfRound.shipAnimator.speed = 1000f;
            return ImpUtils.SkipWaitingForSeconds(result);
        }
        else
        {
            Imperium.StartOfRound.shipAnimator.speed = 1;
            return result; // pure pass-through
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("openingDoorsSequence")]
    private static IEnumerator openingDoorsSequencePatch(IEnumerator __result)
    {
        return SkipShipAnimatorIf(__result, Imperium.ShipManager.InstantLanding);
    }

    /// see also <see cref="RoundManagerPatch.DetectElevatorRunningPostfixPatch"/>
    [HarmonyPostfix]
    [HarmonyPatch("EndOfGame")]
    private static IEnumerator EndOfGamePatch(IEnumerator __result)
    {
        return SkipShipAnimatorIf(__result, Imperium.ShipManager.InstantTakeoff);
    }

    // TODO: Move to RoundManagerPatch and merge with DetectElevatorRunningPostfixPatch?
    [HarmonyPostfix]
    [HarmonyPatch(typeof(RoundManager), "DetectElevatorRunning")]
    private static IEnumerator DetectElevatorRunningPatch(IEnumerator __result)
    {
        return SkipShipAnimatorIf(__result, Imperium.ShipManager.InstantTakeoff);
    }
}
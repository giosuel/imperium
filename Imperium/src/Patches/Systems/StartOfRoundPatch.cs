#region

using System.Collections;
using HarmonyLib;
using Imperium.API.Types.Networking;
using Imperium.Core.Scripts;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("TeleportPlayerInShipIfOutOfRoomBounds")]
    private static bool TeleportPlayerInShipIfOutOfRoomBoundsPatch()
    {
        return !Imperium.Settings.Player.DisableOOB.Value;
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

    [HarmonyPostfix]
    [HarmonyPatch("openingDoorsSequence")]
    private static IEnumerator openingDoorsSequencePostfixPatch(IEnumerator __result)
    {
        return ImpShipAnimatorManager.SkipAnimationIf(__result, Imperium.ShipManager.InstantLanding.Value);
    }

    [HarmonyPostfix]
    [HarmonyPatch("gameOverAnimation")]
    private static IEnumerator gameOverAnimationPostfixPatch(IEnumerator __result)
    {
        return ImpUtils.SkipWaitingForSecondsIf(__result, Imperium.ShipManager.InstantTakeoff.Value);
    }

    [HarmonyPostfix]
    [HarmonyPatch("ShipLeave")]
    private static void ShipLeavePostfixPatch()
    {
        ImpShipAnimatorManager.SkipAnimationIf(Imperium.ShipManager.InstantTakeoff.Value);
    }

    [HarmonyPostfix]
    [HarmonyPatch("EndOfGame")]
    private static IEnumerator EndOfGamePostfixPatch(IEnumerator __result)
    {
        Imperium.IsSceneLoaded.SetFalse();
        return ImpUtils.SkipWaitingForSecondsIf(__result, Imperium.ShipManager.InstantTakeoff.Value);
    }

    [HarmonyPostfix]
    [HarmonyPatch("TravelToLevelEffects")]
    private static IEnumerator TravelToLevelEffectsPostfixPatch(IEnumerator __result)
    {
        return ImpShipAnimatorManager.SkipAnimationIf(__result, Imperium.ShipManager.InstantRoute.Value);
    }
}
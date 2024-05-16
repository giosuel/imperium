#region

using HarmonyLib;
using Imperium.Core;
using Imperium.Netcode;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatch
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal static class PreloadPatches
    {
        /// <summary>
        ///     This is used as the entry function for Imperium
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(StartOfRound), "Awake")]
        private static void ConnectClientToPlayerObjectPatch(StartOfRound __instance)
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                Object.Instantiate(ImpNetworkManager.NetworkPrefab).GetComponent<NetworkObject>().Spawn();
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("EndOfGameClientRpc")]
    private static void EndOfGameClientRpcPatch(StartOfRound __instance)
    {
        Imperium.IsSceneLoaded.SetFalse();
    }

    [HarmonyPrefix]
    [HarmonyPatch("ShipLeaveAutomatically")]
    private static bool ShipLeaveAutomaticallyPatch(StartOfRound __instance)
    {
        if (ImpSettings.Game.PreventShipLeave.Value)
        {
            // We have to revert this
            __instance.allPlayersDead = false;

            ImpOutput.Send("Prevented the ship from leaving.", notificationType: NotificationType.Other);
            Imperium.Log.LogInfo("[MON] Prevented the ship from leaving.");
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
}
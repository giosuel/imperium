#region

using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Core;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyAI))]
internal static class EnemyAIPatch
{
    private static PlayerControllerB[] playerBackup = [];

    [HarmonyPrefix]
    [HarmonyPatch("PlayerIsTargetable")]
    private static bool PlayerIsTargetablePatch(EnemyAI __instance, PlayerControllerB playerScript,
        ref bool __result)
    {
        if (!Imperium.IsImperiumReady) return true;

        if (playerScript == Imperium.Player && ImpSettings.Player.Invisibility.Value)
        {
            __result = false;
            return false;
        }

        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("KillEnemy")]
    private static void KillEnemyPatch(EnemyAI __instance)
    {
        Imperium.ObjectManager.CurrentLevelEntities.Refresh();
        Imperium.Oracle.Simulate(
            $"Entity {Imperium.ObjectManager.GetDisplayName(__instance.enemyType.enemyName)} was killed."
        );
    }

    /// <summary>
    ///     Temporarily removes invisible player from allPlayerScripts
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPrefixPatch(EnemyAI __instance)
    {
        if (!Imperium.IsImperiumReady) return;

        if (ImpSettings.Player.Invisibility.Value)
        {
            playerBackup = Imperium.StartOfRound.allPlayerScripts;
            Imperium.StartOfRound.allPlayerScripts = Imperium.StartOfRound.allPlayerScripts
                .Where(player => player != Imperium.Player).ToArray();
        }
    }

    /// <summary>
    ///     Restores allPlayerScripts modified by prefix patch
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPostfixPatch(EnemyAI __instance)
    {
        if (!Imperium.IsImperiumReady) return;

        if (ImpSettings.Player.Invisibility.Value)
        {
            Imperium.StartOfRound.allPlayerScripts = playerBackup;
        }
    }

    /// <summary>
    ///     Temporarily removes invisible player from allPlayerScripts
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPrefixPatch(EnemyAI __instance)
    {
        if (!Imperium.IsImperiumReady) return;

        if (ImpSettings.Player.Invisibility.Value)
        {
            playerBackup = Imperium.StartOfRound.allPlayerScripts;
            Imperium.StartOfRound.allPlayerScripts = Imperium.StartOfRound.allPlayerScripts
                .Where(player => player != Imperium.Player).ToArray();
        }
    }

    /// <summary>
    ///     Restores allPlayerScripts modified by prefix patch
    /// </summary>
    /// <param name="__instance"></param>
    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPostfixPatch(EnemyAI __instance)
    {
        if (!Imperium.IsImperiumReady) return;

        if (ImpSettings.Player.Invisibility.Value)
        {
            Imperium.StartOfRound.allPlayerScripts = playerBackup;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("MeetsStandardPlayerCollisionConditions")]
    private static void MeetsStandardPlayerCollisionConditionsPrefix()
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("MeetsStandardPlayerCollisionConditions")]
    private static void MeetsStandardPlayerCollisionConditionsPostfix()
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }
}
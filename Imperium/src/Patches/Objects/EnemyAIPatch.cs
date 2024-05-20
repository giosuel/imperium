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
    private static bool PlayerIsTargetablePatch(
        EnemyAI __instance, PlayerControllerB playerScript, ref bool __result
    )
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

    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForPosition")]
    private static void CheckLineOfSightForPositionPrefixPatch(EnemyAI __instance, float width, int range)
    {
        Imperium.Visualization.EntityInfos.LineOfSightUpdate(__instance, null, width, range);
    }

    /// <summary>
    ///     Temporarily removes invisible player from allPlayerScripts
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPrefixPatch(EnemyAI __instance, float width, int range)
    {
        if (ImpSettings.Player.Invisibility.Value)
        {
            playerBackup = Imperium.StartOfRound.allPlayerScripts;
            Imperium.StartOfRound.allPlayerScripts = Imperium.StartOfRound.allPlayerScripts
                .Where(player => player != Imperium.Player).ToArray();
        }

        Imperium.Visualization.EntityInfos.LineOfSightUpdate(__instance, null, width, range);
    }

    /// <summary>
    ///     Restores allPlayerScripts modified by prefix patch
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPostfixPatch(EnemyAI __instance)
    {
        if (ImpSettings.Player.Invisibility.Value)
        {
            Imperium.StartOfRound.allPlayerScripts = playerBackup;
        }
    }

    /// <summary>
    ///     Temporarily removes invisible player from allPlayerScripts
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPrefixPatch(EnemyAI __instance, float width, int range)
    {
        if (ImpSettings.Player.Invisibility.Value)
        {
            playerBackup = Imperium.StartOfRound.allPlayerScripts;
            Imperium.StartOfRound.allPlayerScripts = Imperium.StartOfRound.allPlayerScripts
                .Where(player => player != Imperium.Player).ToArray();
        }

        Imperium.Visualization.EntityInfos.LineOfSightUpdate(__instance, null, width, range);
    }

    /// <summary>
    ///     Restores allPlayerScripts modified by prefix patch
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPostfixPatch(EnemyAI __instance)
    {
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
#region

using System.Collections.Generic;
using HarmonyLib;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(RoundManager))]
internal static class RoundManagerPatch
{
    internal static readonly Dictionary<string, List<Vector3>> spawnedEntitiesInCycle = new();

    [HarmonyPrefix]
    [HarmonyPatch("SpawnScrapInLevel")]
    private static void SpawnScrapInLevelPrefixPatch(RoundManager __instance)
    {
        var random = ImpUtils.CloneRandom(__instance.AnomalyRandom);
        MoonManager.Current.ScrapAmount = (int)(random.Next(
            __instance.currentLevel.minScrap, __instance.currentLevel.maxScrap) * __instance.scrapAmountMultiplier);
        MoonManager.Current.ChallengeScrapAmount = MoonManager.Current.ScrapAmount + random.Next(10, 30);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnScrapInLevel")]
    private static void SpawnScrapInLevelPostfixPatch()
    {
        Imperium.ObjectManager.RefreshLevelItems();
    }

    [HarmonyPostfix]
    [HarmonyPatch("YRotationThatFacesTheNearestFromPosition")]
    private static void YRotationThatFacesTheNearestFromPositionPatch(RoundManager __instance)
    {
        if (!Imperium.IsImperiumReady) return;

        // Re-simulate spawn cycle this function uses AnomalyRandom
        Imperium.Log.LogInfo("[ORACLE] Oracle had to re-simulate due to YRotNear");
        Imperium.Oracle.Simulate();
    }

    [HarmonyPostfix]
    [HarmonyPatch("YRotationThatFacesTheFarthestFromPosition")]
    private static void YRotationThatFacesTheFarthestFromPosition(RoundManager __instance)
    {
        if (!Imperium.IsImperiumReady) return;

        // Re-simulate spawn cycle this function uses AnomalyRandom
        Imperium.Log.LogInfo("[ORACLE] Oracle had to re-simulate due to YRotFar");
        Imperium.Oracle.Simulate();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnEnemyFromVent")]
    private static void SpawnEnemyFromVentPatch(RoundManager __instance)
    {
        Imperium.ObjectManager.RefreshLevelEntities();
    }
    
    [HarmonyPrefix]
    [HarmonyPatch("BeginEnemySpawning")]
    private static void BeginEnemySpawningPrefixPatch(RoundManager __instance)
    {
        Imperium.Oracle.Simulate(initial: true, null);
        
        ImpSpawnTracker.StartCycle(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("BeginEnemySpawning")]
    private static void BeginEnemySpawningPostfixPatch(RoundManager __instance)
    {
        ImpSpawnTracker.EndCycle(__instance);
    }
    
    [HarmonyPrefix]
    [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
    private static void AdvanceHourAndSpawnNewBatchOfEnemiesPrefixPatch(RoundManager __instance)
    {
        ImpSpawnTracker.StartCycle(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
    private static void AdvanceHourAndSpawnNewBatchOfEnemiesPostfixPatch(RoundManager __instance)
    {
        Imperium.ObjectManager.RefreshLevelEntities();
        
        ImpSpawnTracker.EndCycle(__instance);
    }
    
    
    [HarmonyPrefix]
    [HarmonyPatch("PlotOutEnemiesForNextHour")]
    private static bool PlotOutEnemiesForNextHourPatch()
    {
        return !Imperium.GameManager.IndoorSpawningPaused.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SpawnEnemiesOutside")]
    private static bool SpawnEnemiesOutsidePatch()
    {
        return !Imperium.GameManager.OutdoorSpawningPaused.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SpawnDaytimeEnemiesOutside")]
    private static bool SpawnDaytimeEnemiesOutsidePatch(RoundManager __instance)
    {
        return !Imperium.GameManager.DaytimeSpawningPaused.Value;
    }

    /// <summary>
    /// Level is finished generating, all scrap and map obstacles have been placed, no entities yet
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("FinishGeneratingNewLevelClientRpc")]
    private static void FinishGeneratingNewLevelClientRpcPostfixPatch()
    {
        Imperium.IsSceneLoaded.SetTrue();
    }
}
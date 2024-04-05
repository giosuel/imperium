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
        ImpOutput.Log("[ORACLE] Oracle had to re-simulate due to YRotNear");

        Imperium.Log.LogInfo("YRotationThatFacesTheNearestFromPosition DIE ORACLE TRIGGER");

        Imperium.Oracle.Simulate();
    }

    [HarmonyPostfix]
    [HarmonyPatch("YRotationThatFacesTheFarthestFromPosition")]
    private static void YRotationThatFacesTheFarthestFromPosition(RoundManager __instance)
    {
        if (!Imperium.IsImperiumReady) return;

        // Re-simulate spawn cycle this function uses AnomalyRandom
        ImpOutput.Log("[ORACLE] Oracle had to re-simulate due to YRotFar");

        Imperium.Log.LogInfo("YRotationThatFacesTheFarthestFromPosition DIE ORACLE TRIGGER");

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
        Imperium.Log.LogInfo("BEGIN ORSCLE SPAWN TRIGGR");
        Imperium.Log.LogInfo(
            $"BEFORE BEGIN SPAWN, Real anomaly: {ImpUtils.CloneRandom(__instance.AnomalyRandom).Next()}");
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

        Imperium.Oracle.Simulate();
    }

    [HarmonyPostfix]
    [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
    private static void AdvanceHourAndSpawnNewBatchOfEnemiesPostfixPatch(RoundManager __instance)
    {
        Imperium.ObjectManager.RefreshLevelEntities();

        ImpSpawnTracker.EndCycle(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnRandomDaytimeEnemy")]
    private static void SpawnRandomDaytimeEnemy(RoundManager __instance, bool __result)
    {
        Imperium.Log.LogInfo(
            $"After spawn single daytime entity, Real anomaly: {ImpUtils.CloneRandom(__instance.AnomalyRandom).Next()}, result: {__result}");
    }

    [HarmonyPostfix]
    [HarmonyPatch("PositionWithDenialPointsChecked")]
    private static void PositionWithDenialPointsCheckedPatch(RoundManager __instance, Vector3 spawnPosition)
    {
        Imperium.Log.LogInfo($"Spawn position for denial: {ImpUtils.FormatVector(spawnPosition)}");
        Imperium.Log.LogInfo(
            $"After SPAWN DENIAL FUNCTION, Real anomaly: {ImpUtils.CloneRandom(__instance.AnomalyRandom).Next()}");
        // Imperium.Log.LogInfo($"After Daytime, Real outside: {ImpUtils.CloneRandom(__instance.OutsideEnemySpawnRandom).Next()}");
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnDaytimeEnemiesOutside")]
    private static void SpawnDaytimeEnemiesOutsidePostfixPatch(RoundManager __instance)
    {
        Imperium.Log.LogInfo($"After Daytime, Real anomaly: {ImpUtils.CloneRandom(__instance.AnomalyRandom).Next()}");
        // Imperium.Log.LogInfo($"After Daytime, Real outside: {ImpUtils.CloneRandom(__instance.OutsideEnemySpawnRandom).Next()}");
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnEnemiesOutside")]
    private static void SpawnEnemiesOutsidePostfixPatch(RoundManager __instance)
    {
        Imperium.Log.LogInfo($"After Outside, Real anomaly: {ImpUtils.CloneRandom(__instance.AnomalyRandom).Next()}");
        // Imperium.Log.LogInfo($"After Outside, Real outside: {ImpUtils.CloneRandom(__instance.OutsideEnemySpawnRandom).Next()}");
    }

    [HarmonyPostfix]
    [HarmonyPatch("PlotOutEnemiesForNextHour")]
    private static void PlotOutEnemiesForNextHourPostfixPatch(RoundManager __instance)
    {
        Imperium.Log.LogInfo($"After Indoor, Real anomaly: {ImpUtils.CloneRandom(__instance.AnomalyRandom).Next()}");
        // Imperium.Log.LogInfo($"After Indoor, Real outside: {ImpUtils.CloneRandom(__instance.OutsideEnemySpawnRandom).Next()}");
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
        Imperium.Log.LogInfo($"BEFORE Daytime, Real anomaly: {ImpUtils.CloneRandom(__instance.AnomalyRandom).Next()}");
        var currentHour = Reflection.Get<RoundManager, int>(__instance, "currentHour");
        var num = __instance.timeScript.lengthOfHours * currentHour;
        var num2 =
            __instance.currentLevel.daytimeEnemySpawnChanceThroughDay.Evaluate(num / __instance.timeScript.totalTime);
        Imperium.Log.LogInfo("==================== Daytime Entities Spawned ====================");
        Imperium.Log.LogInfo($"Daytime entities: {num2}");
        var anomalySimulator = ImpUtils.CloneRandom(Imperium.RoundManager.AnomalyRandom);
        var random = anomalySimulator.Next((int)(num2 - __instance.currentLevel.daytimeEnemiesProbabilityRange),
            (int)(num2 + __instance.currentLevel.daytimeEnemiesProbabilityRange));
        var entityAmount = Mathf.Clamp(random, 0, 20);
        Imperium.Log.LogInfo($"Actual daytime amount: {entityAmount}");
        Imperium.Log.LogInfo($"Actual daytime amount rmg: {random}");
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
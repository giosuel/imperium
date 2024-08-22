#region

using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Imperium.Core;
using Imperium.Util;
using Imperium.Util.Binding;
using Imperium.Visualizers;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(RoundManager))]
internal static class RoundManagerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("SpawnScrapInLevel")]
    private static void SpawnScrapInLevelPrefixPatch(RoundManager __instance)
    {
        var random = ImpUtils.CloneRandom(__instance.AnomalyRandom);
        Imperium.MoonManager.ScrapAmount = (int)(random.Next(
            __instance.currentLevel.minScrap, __instance.currentLevel.maxScrap) * __instance.scrapAmountMultiplier);
        Imperium.MoonManager.ChallengeScrapAmount = Imperium.MoonManager.ScrapAmount + random.Next(10, 30);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnEnemyGameObject")]
    private static void SpawnEnemyGameObjectPostfixPatch(RoundManager __instance)
    {
        // Refresh entities when the game spawns entities via this function
        Imperium.ObjectManager.RefreshLevelEntities();
    }

    [HarmonyPostfix]
    [HarmonyPatch("DetectElevatorRunning")]
    private static void DetectElevatorRunningPostfixPatch(RoundManager __instance)
    {
        // Reset ship animator
        Imperium.StartOfRound.shipAnimator.gameObject.GetComponent<PlayAudioAnimationEvent>().audioToPlay.mute = false;
        Imperium.StartOfRound.shipAnimator.gameObject.GetComponent<PlayAudioAnimationEvent>().audioToPlayB.mute = false;
        Imperium.StartOfRound.shipAnimator.speed = 1;
    }

    [HarmonyPrefix]
    [HarmonyPatch("PlayAudibleNoise")]
    private static void PlayAudibleNoisePatch(
        Vector3 noisePosition,
        float noiseRange,
        float noiseLoudness,
        int timesPlayedInSameSpot,
        bool noiseIsInsideClosedShip,
        int noiseID
    )
    {
        var range = noiseRange;
        if (noiseIsInsideClosedShip) range /= 2f;

        var colliders = new Collider[20];
        Physics.OverlapSphereNonAlloc(
            noisePosition,
            range,
            colliders,
            8912896,
            QueryTriggerInteraction.Collide
        );
        colliders.FirstOrDefault(collider => collider && collider.gameObject.name == "Imp_NoiseListener")
            ?.GetComponent<INoiseListener>()
            ?.DetectNoise(noisePosition, noiseLoudness, timesPlayedInSameSpot, noiseID);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnScrapInLevel")]
    private static void SpawnScrapInLevelPostfixPatch()
    {
        Imperium.ObjectManager.RefreshLevelObjects();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnOutsideHazards")]
    private static void SpawnOutsideHazardsPostfixPatch()
    {
        Imperium.ObjectManager.RefreshLevelObjects();
    }

    internal static readonly ImpBinding<HashSet<HazardIndicator>> MapHazardPositions = new([]);

    [HarmonyPrefix]
    [HarmonyPatch("SpawnMapObjects")]
    private static void SpawnMapObjectsPatch()
    {
        MapHazardPositions.Set(
            Object.FindObjectsOfType<RandomMapObject>()
                .Select(node => new HazardIndicator(node.transform.position, node.spawnRange))
                .ToHashSet()
        );
    }

    [HarmonyPostfix]
    [HarmonyPatch("YRotationThatFacesTheNearestFromPosition")]
    private static void YRotationThatFacesTheNearestFromPositionPatch(RoundManager __instance)
    {
        if (!Imperium.IsImperiumLoaded) return;

        // Re-simulate spawn cycle this function uses AnomalyRandom
        Imperium.IO.LogInfo("[ORACLE] Oracle had to re-simulate due to YRotNear");
        Imperium.Oracle.Resimulate(null);
    }

    [HarmonyPostfix]
    [HarmonyPatch("YRotationThatFacesTheFarthestFromPosition")]
    private static void YRotationThatFacesTheFarthestFromPosition(RoundManager __instance)
    {
        if (!Imperium.IsImperiumLoaded) return;

        // Re-simulate spawn cycle this function uses AnomalyRandom
        Imperium.IO.LogInfo("[ORACLE] Oracle had to re-simulate due to YRotFar");
        Imperium.Oracle.Resimulate(null);
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnEnemyFromVent")]
    private static void SpawnEnemyFromVentPatch(RoundManager __instance, EnemyVent vent)
    {
        Imperium.ObjectManager.RefreshLevelEntities();
        Imperium.EventLog.GameEvents.SpawnEnemyFromVent(vent);
    }

    [HarmonyPrefix]
    [HarmonyPatch("GenerateNewFloor")]
    private static void GenerateNewFloorPrefixPatch(RoundManager __instance)
    {
// #if DEBUG
//         __instance.mapSizeMultiplier = 0.1f;
        foreach (var flow in __instance.currentLevel.dungeonFlowTypes)
        {
            flow.rarity = flow.id == 4 ? 10 : 0;
        }
// #endif
    }

    [HarmonyPostfix]
    [HarmonyPatch("SwitchPower")]
    private static void SwitchPowerPatch(RoundManager __instance, bool on)
    {
        Imperium.EventLog.GameEvents.SwitchPower(on);
    }

    [HarmonyPrefix]
    [HarmonyPatch("BeginEnemySpawning")]
    private static void BeginEnemySpawningPrefixPatch(RoundManager __instance)
    {
        Imperium.Oracle.Simulate(initial: true, null);

        Imperium.EventLog.GameEvents.AdvanceHourAndSpawnNewBatchOfEnemiesPrefix(true);

        ImpSpawnTracker.StartCycle(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("BeginEnemySpawning")]
    private static void BeginEnemySpawningPostfixPatch(RoundManager __instance)
    {
        Imperium.EventLog.GameEvents.AdvanceHourAndSpawnNewBatchOfEnemiesPostfix(true);

        ImpSpawnTracker.EndCycle(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
    private static void AdvanceHourAndSpawnNewBatchOfEnemiesPrefixPatch(RoundManager __instance)
    {
        ImpSpawnTracker.StartCycle(__instance);

        Imperium.EventLog.GameEvents.AdvanceHourAndSpawnNewBatchOfEnemiesPrefix(false);

        Imperium.Oracle.Simulate();
    }

    [HarmonyPostfix]
    [HarmonyPatch("AdvanceHourAndSpawnNewBatchOfEnemies")]
    private static void AdvanceHourAndSpawnNewBatchOfEnemiesPostfixPatch(RoundManager __instance)
    {
        Imperium.ObjectManager.RefreshLevelObjects();

        ImpSpawnTracker.EndCycle(__instance);

        Imperium.EventLog.GameEvents.AdvanceHourAndSpawnNewBatchOfEnemiesPostfix(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch("PlotOutEnemiesForNextHour")]
    private static bool PlotOutEnemiesForNextHourPatch()
    {
        return !Imperium.MoonManager.IndoorSpawningPaused.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SpawnEnemiesOutside")]
    private static bool SpawnEnemiesOutsidePatch()
    {
        return !Imperium.MoonManager.OutdoorSpawningPaused.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SpawnDaytimeEnemiesOutside")]
    private static bool SpawnDaytimeEnemiesOutsidePatch(RoundManager __instance)
    {
        return !Imperium.MoonManager.DaytimeSpawningPaused.Value;
    }

    /// <summary>
    ///     Level is finished generating, all scrap and map obstacles have been placed, no entities yet
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("RefreshEnemiesList")]
    private static void RefreshEnemiesListPostfixPatch() => Imperium.IsSceneLoaded.SetTrue();
}
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
        Imperium.ObjectManager.TriggerRefresh();
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnOutsideHazards")]
    private static void SpawnOutsideHazardsPostfixPatch()
    {
        Imperium.ObjectManager.TriggerRefresh();
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
    [HarmonyPatch("SpawnEnemyFromVent")]
    private static void SpawnEnemyFromVentPatch(RoundManager __instance, EnemyVent vent)
    {
        Imperium.ObjectManager.RefreshLevelEntities();
        Imperium.EventLog.GameEvents.SpawnEnemyFromVent(vent);
    }

    private static float mapSizeMultiplierBackup;
    private static IntWithRarity[] dungeonFlowTypesBackup = [];

    [HarmonyPrefix]
    [HarmonyPatch("GenerateNewFloor")]
    private static void GenerateNewFloorPrefixPatch(RoundManager __instance)
    {
        if (Imperium.GameManager.CustomMapSize.Value > -1)
        {
            mapSizeMultiplierBackup = __instance.currentLevel.factorySizeMultiplier;
            __instance.currentLevel.factorySizeMultiplier = Imperium.GameManager.CustomMapSize.Value;
        }

        if (Imperium.GameManager.CustomDungeonFlow.Value > -1)
        {
            dungeonFlowTypesBackup = Imperium.StartOfRound.currentLevel.dungeonFlowTypes;
            __instance.currentLevel.dungeonFlowTypes =
            [
                new IntWithRarity(Imperium.GameManager.CustomDungeonFlow.Value, 1, null)
            ];
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("GenerateNewFloor")]
    private static void GenerateNewFloorPostfixPatch(RoundManager __instance)
    {
        // Restore original dungeon flow rarity list and map size multiplier, if previously overridden
        if (Imperium.GameManager.CustomMapSize.Value > -1)
        {
            __instance.currentLevel.factorySizeMultiplier = mapSizeMultiplierBackup;
        }

        if (Imperium.GameManager.CustomDungeonFlow.Value > -1)
        {
            __instance.currentLevel.dungeonFlowTypes = dungeonFlowTypesBackup;
        }
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

        Imperium.Oracle.Simulate();
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
        Imperium.ObjectManager.TriggerRefresh();

        ImpSpawnTracker.EndCycle(__instance);

        Imperium.EventLog.GameEvents.AdvanceHourAndSpawnNewBatchOfEnemiesPostfix(false);
    }

    [HarmonyPrefix]
    [HarmonyPatch("PlotOutEnemiesForNextHour")]
    private static bool PlotOutEnemiesForNextHourPatch(RoundManager __instance)
    {
        return !Imperium.MoonManager.IndoorSpawningPaused.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SpawnEnemiesOutside")]
    private static bool SpawnEnemiesOutsidePatch(RoundManager __instance)
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
    ///     Level is finished generating, all scrap and map obstacles have been placed, no entities yet.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("RefreshEnemiesList")]
    private static void RefreshEnemiesListPostfixPatch(RoundManager __instance)
    {
        Imperium.IsSceneLoaded.SetTrue();

        // We need to do this here because the occlusion culler always needs to be active at the start.
        if (Imperium.Settings.Rendering.DisableCulling.Value)
        {
            Imperium.StartOfRound.occlusionCuller.enabled = false;
        }

        Imperium.MoonManager.FogEnabledThisRound = __instance.indoorFog.gameObject.activeSelf;
    }
}
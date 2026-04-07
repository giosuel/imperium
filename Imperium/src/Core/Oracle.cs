#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types;
using Imperium.API.Types.Networking;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using Random = System.Random;

#endregion

namespace Imperium.Core;

internal class Oracle : ImpLifecycleObject
{
    internal readonly ImpBinding<OracleState> State = new(new OracleState());

    internal void Simulate()
    {
        Imperium.IO.LogInfo("[ORACLE] Oracle is simulating...");
        Simulate(false, null);
    }

    internal void Resimulate(string reason) => Simulate(false, reason);

    internal void Simulate(bool initial, string reason)
    {
        ImpUtils.RunSafe(() => SimulateUnsafe(initial, reason), "Oracle simulation failed");
    }

    private void SimulateUnsafe(bool initial, string reason)
    {
        if (!Imperium.IsSceneLoaded.Value) return;

        var currentHour = Imperium.RoundManager.currentHour;
        if (!initial) currentHour += Imperium.RoundManager.hourTimeBetweenEnemySpawnBatches;

        Imperium.IO.LogDebug($"[ORACLE] Start simulating at {currentHour}. Initial: {initial}");

        var AnomalySimulator = ImpUtils.CloneRandom(Imperium.RoundManager.AnomalyRandom);
        var EntitySimulator = ImpUtils.CloneRandom(Imperium.RoundManager.EnemySpawnRandom);

        var OutsideEnemySpawnSimulator = ImpUtils.CloneRandom(Imperium.RoundManager.OutsideEnemySpawnRandom);
        var OutsideEnemySpawnPlacementSimulator = ImpUtils.CloneRandom(
            Imperium.RoundManager.OutsideEnemySpawnPlacementRandom
        );

        var WeedEnemySpawnSimulator = ImpUtils.CloneRandom(Imperium.RoundManager.WeedEnemySpawnRandom);
        var WeedEnemySpawnPlacementSimulator = ImpUtils.CloneRandom(
            Imperium.RoundManager.WeedEnemySpawnPlacementRandom
        );

        var DaytimeEnemySpawnSimulator = ImpUtils.CloneRandom(Imperium.RoundManager.DaytimeEnemySpawnRandom);
        var DaytimeEnemySpawnPlacementSimulator = ImpUtils.CloneRandom(
            Imperium.RoundManager.DaytimeEnemySpawnPlacementRandom
        );

        var roundManager = Imperium.RoundManager;
        var currentLevel = roundManager.currentLevel;

        var moldSpreadManager = FindObjectOfType<MoldSpreadManager>();

        // Variables that change in cycles
        var currentTime = Imperium.TimeOfDay.currentDayTime;
        var normalizedTimeOfDay = Imperium.TimeOfDay.normalizedTimeOfDay;

        var indoorPower = roundManager.currentEnemyPower;
        var indoorPowerNoDeaths = roundManager.currentEnemyPowerNoDeaths;
        var indoorDiversityLevel = roundManager.currentInsideEnemyDiversityLevel;

        var outdoorPower = roundManager.currentOutsideEnemyPower;
        var outdoorPowerNoDeaths = roundManager.currentOutsideEnemyPowerNoDeaths;
        var outdoorDiversityLevel = roundManager.currentOutsideEnemyDiversityLevel;

        var daytimePower = roundManager.currentDaytimeEnemyPower;
        var daytimePowerNoDeaths = roundManager.currentDaytimeEnemyPowerNoDeaths;

        var weedPower = roundManager.currentWeedEnemyPower;

        var cannotSpawnMoreInsideEnemies = roundManager.cannotSpawnMoreInsideEnemies;

        var indoorEntityCounts = currentLevel.Enemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.numberSpawned);
        var indoorEntitySpawnedAtLeastOnce = currentLevel.Enemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.hasSpawnedAtLeastOne);

        var outdoorEntityCounts = currentLevel.OutsideEnemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.numberSpawned);
        var outdoorEntitySpawnedAtLeastOnce = currentLevel.OutsideEnemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.hasSpawnedAtLeastOne);

        var daytimeEntityCounts = currentLevel.DaytimeEnemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.numberSpawned);
        var daytimeEntitySpawnedAtLeastOnce = currentLevel.DaytimeEnemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.hasSpawnedAtLeastOne);

        var weedEntityCounts = roundManager.WeedEnemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.numberSpawned);
        var weedEntitySpawnedAtLeastOnce = roundManager.WeedEnemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.hasSpawnedAtLeastOne);

        var firstTimeSpawningEnemies = roundManager.firstTimeSpawningEnemies;
        var firstTimeSpawningOutsideEnemies = roundManager.firstTimeSpawningOutsideEnemies;
        var firstTimeSpawningDaytimeEnemies = roundManager.firstTimeSpawningDaytimeEnemies;
        var firstTimeSpawningWeedEnemies = roundManager.firstTimeSpawningWeedEnemies;

        var minIndoorEntitiesToSpawn = roundManager.minEnemiesToSpawn;

        // Cycle Variables
        State.Value.CurrentCycle = Mathf.RoundToInt(
            currentHour * Imperium.TimeOfDay.lengthOfHours / Imperium.TimeOfDay.totalTime * 9
        );

        Imperium.IO.LogDebug($"[ORACLE] Simulating starting cycle: {State.Value.CurrentCycle}");

        for (var i = State.Value.CurrentCycle; i <= 9; i++)
        {
            Imperium.IO.LogDebug(
                $"[ORACLE] Simulating cycle {i} at currentTime: {currentTime}, currentHour: {currentHour}");

            State.Value.Cycles[i].CycleTime = currentTime;

            // The game only spawns outdoor, daytime and weed entities after the initial cycle
            if (i > 0)
            {
                if (!Imperium.MoonManager.DaytimeSpawningPaused.Value)
                {
                    State.Value.DaytimeCycles[i] = SimulateDaytimeSpawnCycle(
                        AnomalySimulator, DaytimeEnemySpawnSimulator,
                        DaytimeEnemySpawnPlacementSimulator,
                        ref daytimePower,
                        ref daytimePowerNoDeaths,
                        daytimeEntityCounts,
                        daytimeEntitySpawnedAtLeastOnce,
                        ref firstTimeSpawningDaytimeEnemies,
                        normalizedTimeOfDay,
                        currentHour, currentTime
                    );
                }

                if (!Imperium.MoonManager.OutdoorSpawningPaused.Value)
                {
                    State.Value.OutdoorCycles[i] = SimulateOutdoorSpawnCycle(
                        AnomalySimulator,
                        OutsideEnemySpawnSimulator,
                        OutsideEnemySpawnPlacementSimulator,
                        ref outdoorPower,
                        ref outdoorPowerNoDeaths,
                        ref outdoorDiversityLevel,
                        outdoorEntityCounts,
                        outdoorEntitySpawnedAtLeastOnce,
                        ref firstTimeSpawningOutsideEnemies,
                        moldSpreadManager,
                        normalizedTimeOfDay,
                        currentHour, currentTime
                    );
                }

                if (!Imperium.MoonManager.WeedSpawningPaused.Value)
                {
                    State.Value.OutdoorCycles[i].AddRange(SimulateWeedSpawnCycle(
                        AnomalySimulator,
                        WeedEnemySpawnSimulator,
                        WeedEnemySpawnPlacementSimulator,
                        ref weedPower,
                        weedEntityCounts,
                        weedEntitySpawnedAtLeastOnce,
                        moldSpreadManager,
                        ref firstTimeSpawningWeedEnemies,
                        currentHour, currentTime
                    ));
                }
            }

            var spawnTimes = new List<int>();
            if (!Imperium.MoonManager.IndoorSpawningPaused.Value)
            {
                State.Value.IndoorCycles[i] = SimulateIndoorSpawnCycle(
                    EntitySimulator,
                    ref indoorPower,
                    ref indoorPowerNoDeaths,
                    ref indoorDiversityLevel,
                    indoorEntityCounts,
                    indoorEntitySpawnedAtLeastOnce,
                    ref cannotSpawnMoreInsideEnemies,
                    ref firstTimeSpawningEnemies,
                    ref minIndoorEntitiesToSpawn,
                    normalizedTimeOfDay,
                    currentHour
                );
                spawnTimes = State.Value.IndoorCycles[i].Select(report => report.SpawnTime).ToList();
            }

            var timeUpToCurrentHour = Imperium.TimeOfDay.lengthOfHours * currentHour;
            State.Value.Cycles[i].MinSpawnTime = (int)(10f + timeUpToCurrentHour);
            State.Value.Cycles[i].MaxSpawnTime = (int)Imperium.TimeOfDay.lengthOfHours *
                roundManager.hourTimeBetweenEnemySpawnBatches + timeUpToCurrentHour - 1;

            // Add next regular spawn time to possible spawns as fallback when no vents are being used
            spawnTimes.Add((currentHour + 1) * (int)Imperium.TimeOfDay.lengthOfHours);

            var lastSpawn = spawnTimes.Max();
            State.Value.Cycles[i].NextCycleTime = lastSpawn;

            // Advance cycle times
            currentHour += Imperium.RoundManager.hourTimeBetweenEnemySpawnBatches;
            currentTime = lastSpawn;
        }

        State.Value.PrintLog();

        if (!string.IsNullOrEmpty(reason))
        {
            Imperium.IO.Send(
                $"Spawn predictions updated due to {reason}!",
                title: "Oracle",
                type: NotificationType.OracleUpdate
            );
        }

        State.Refresh();
    }

    private static List<SpawnReport> SimulateIndoorSpawnCycle(
        Random entitySimulator,
        ref float currentPower,
        ref float currentPowerNoDeaths,
        ref int currentDiversityLevel,
        IDictionary<EnemyType, int> entitySpawnCounts,
        IDictionary<EnemyType, bool> entitySpawnedAtLeastOne,
        ref bool cannotSpawnMoreInsideEnemies,
        ref bool firstTimeSpawning,
        ref int minEntitiesToSpawn,
        float normalizedTimeOfDay,
        int currentHour
    )
    {
        var roundManager = Imperium.RoundManager;
        var spawning = new List<SpawnReport>();

        var freeVents = Imperium.RoundManager.allEnemyVents.Where(t => !t.occupied).ToList();
        var timeUpToCurrentHour = Imperium.TimeOfDay.lengthOfHours * currentHour;

        if (!freeVents.Any() || cannotSpawnMoreInsideEnemies) return spawning;

        if (
            StartOfRound.Instance.connectedPlayersAmount + 1 > 0 &&
            TimeOfDay.Instance.daysUntilDeadline <= 2 && (
                roundManager.valueOfFoundScrapItems / TimeOfDay.Instance.profitQuota > 0.8f &&
                normalizedTimeOfDay > 0.3f ||
                roundManager.valueOfFoundScrapItems / roundManager.totalScrapValueInLevel > 0.65f ||
                StartOfRound.Instance.daysPlayersSurvivedInARow >= 5
            ) &&
            minEntitiesToSpawn == 0
        )
        {
            minEntitiesToSpawn = 1;
        }

        // Get time of next hour since AdvanceHourAndSpawnNewBatchOfEnemies increases currentHour before spawning
        var baseEntityAmount = roundManager.currentLevel.enemySpawnChanceThroughoutDay.Evaluate(
            Imperium.TimeOfDay.lengthOfHours * currentHour / roundManager.timeScript.totalTime);
        baseEntityAmount -= 1;

        if (StartOfRound.Instance.isChallengeFile) baseEntityAmount += 1f;

        var lowerBound = baseEntityAmount + Mathf.Abs(Imperium.TimeOfDay.daysUntilDeadline - 3) / 1.6f -
                         roundManager.currentLevel.spawnProbabilityRange;
        var upperBound = baseEntityAmount + roundManager.currentLevel.spawnProbabilityRange;

        var entityAmount = Mathf.RoundToInt(
            Mathf.Clamp(
                Mathf.Lerp(lowerBound, upperBound, (float)entitySimulator.NextDouble()),
                minEntitiesToSpawn, 20f
            )
        );

        if (roundManager.enemyRushIndex != -1) entityAmount += 2;
        entityAmount = Mathf.Clamp(entityAmount, 0, freeVents.Count);

        if (currentPower >= roundManager.currentMaxInsidePower)
        {
            cannotSpawnMoreInsideEnemies = true;
            return spawning;
        }

        var specialEntity = roundManager.currentLevel.specialEnemyRarity;
        if (
            specialEntity.overrideEnemy != null &&
            specialEntity.overrideEnemy.numberSpawned < specialEntity.overrideEnemy.MaxCount &&
            specialEntity.percentageChance >= 1f
        )
        {
            entityAmount = Mathf.Max(entityAmount, 1);
        }

        for (var i = 0; i < entityAmount; i++)
        {
            var spawnTime = entitySimulator.Next(
                (int)(10f + timeUpToCurrentHour),
                (int)(Imperium.TimeOfDay.lengthOfHours * roundManager.hourTimeBetweenEnemySpawnBatches +
                      timeUpToCurrentHour
                )
            );
            var spawnVentIndex = entitySimulator.Next(freeVents.Count);
            var spawnVent = freeVents[spawnVentIndex];

            var dynamicPower = currentPowerNoDeaths;
            var useNoDeathsPower = false;
            for (var j = 0; j < roundManager.currentLevel.OutsideEnemies.Count; j++)
            {
                var entity = roundManager.currentLevel.OutsideEnemies[j];
                if (firstTimeSpawning)
                {
                    entitySpawnCounts[entity.enemyType] = 0;
                    entitySpawnedAtLeastOne[entity.enemyType] = false;
                }

                if (!IndoorEnemyCannotBeSpawned(j, currentPower, currentDiversityLevel, entitySpawnCounts))
                {
                    useNoDeathsPower = true;
                    break;
                }
            }

            if (!useNoDeathsPower)
            {
                dynamicPower = currentPower;
            }

            var probabilities = new List<int>();
            for (var j = 0; j < roundManager.currentLevel.Enemies.Count; j++)
            {
                var enemyType = roundManager.currentLevel.Enemies[j].enemyType;

                if (firstTimeSpawning)
                {
                    entitySpawnCounts[enemyType] = 0;
                    entitySpawnedAtLeastOne[enemyType] = false;
                }

                var entityCannotBespawned =
                    IndoorEnemyCannotBeSpawned(j, dynamicPower, currentDiversityLevel, entitySpawnCounts);

                if (entityCannotBespawned)
                {
                    probabilities.Add(0);
                    continue;
                }

                var currentTime = Imperium.TimeOfDay.lengthOfHours * currentHour / Imperium.TimeOfDay.totalTime;

                int probability;

                if (
                    roundManager.enemyRushIndex != -1 && roundManager.enemyRushIndex == j ||
                    roundManager.increasedInsideEnemySpawnRateIndex == j
                )
                {
                    probability = 100;
                }
                else if (enemyType.useNumberSpawnedFalloff)
                {
                    probability = (int)(
                        roundManager.currentLevel.Enemies[j].rarity * (
                            enemyType.probabilityCurve.Evaluate(currentTime) *
                            enemyType.numberSpawnedFalloff.Evaluate(
                                Mathf.Clamp(entitySpawnCounts[enemyType] / 10f, 0, 1)
                            )
                        )
                    );
                }
                else
                {
                    probability = (int)(
                        roundManager.currentLevel.Enemies[j].rarity *
                        enemyType.probabilityCurve.Evaluate(currentTime)
                    );
                }

                if (
                    enemyType.increasedChanceInterior != -1 &&
                    roundManager.currentDungeonType == enemyType.increasedChanceInterior
                )
                {
                    probability = (int)Mathf.Min(probability * 1.5f, probability + 50);
                }

                if (roundManager.enemyRushIndex == -1 && roundManager.enemyRushIndex != j)
                {
                    probability = Mathf.RoundToInt(probability * 0.075f);
                }

                probabilities.Add(probability);
            }

            firstTimeSpawning = false;

            if (probabilities.Sum() <= 0)
            {
                if (dynamicPower >= Imperium.RoundManager.currentMaxInsidePower)
                {
                    cannotSpawnMoreInsideEnemies = true;
                    break;
                }
            }

            var entityIndex = 0;
            var hasOverrideEntity = false;
            if (roundManager.currentLevel.specialEnemyRarity.overrideEnemy != null)
            {
                var overrideEntity = roundManager.currentLevel.specialEnemyRarity;
                Imperium.IO.LogDebug($"[ORACLE] Level has override entity: {overrideEntity.overrideEnemy.enemyName}");

                if (overrideEntity.percentageChance >= 1f)
                {
                    for (var j = 0; j < roundManager.currentLevel.Enemies.Count; j++)
                    {
                        if (roundManager.currentLevel.Enemies[j].enemyType == overrideEntity.overrideEnemy)
                        {
                            if (probabilities[j] != 0)
                            {
                                entityIndex = j;
                                hasOverrideEntity = true;
                            }

                            break;
                        }
                    }
                }
                else
                {
                    var num6 = -1;
                    var num7 = 0f;
                    for (var j = 0; j < roundManager.currentLevel.Enemies.Count; j++)
                    {
                        if (roundManager.currentLevel.Enemies[j].enemyType == overrideEntity.overrideEnemy)
                        {
                            num6 = j;
                        }

                        if (num6 != j)
                        {
                            num7 += probabilities[j];
                        }
                    }

                    if (num6 != -1 && probabilities[num6] != 0 && specialEntity.percentageChance > 0f)
                    {
                        probabilities[num6] =
                            (int)(specialEntity.percentageChance * num7 / (1f - specialEntity.percentageChance));
                    }
                }
            }

            if (!hasOverrideEntity)
            {
                entityIndex = roundManager.GetRandomWeightedIndex(probabilities.ToArray(), entitySimulator);
                Imperium.IO.LogDebug($"[ORACLE] Randomly picking enemy from list of {probabilities.Count}");
            }

            var spawningEntity = roundManager.currentLevel.Enemies[entityIndex].enemyType;

            Imperium.IO.LogDebug($"[ORACLE] Picked indoor entity: {spawningEntity.enemyName}");

            currentPower += spawningEntity.PowerLevel;
            currentPowerNoDeaths += spawningEntity.PowerLevel;

            entitySpawnCounts[spawningEntity]++;
            entitySpawnedAtLeastOne[spawningEntity] = true;

            spawning.Add(new SpawnReport
            {
                Entity = spawningEntity,
                Position = spawnVent.floorNode?.position ?? Vector3.zero,
                SpawnTime = spawnTime
            });

            freeVents.RemoveAt(spawnVentIndex);
        }

        return spawning;
    }

    private static List<SpawnReport> SimulateWeedSpawnCycle(
        Random anomalySimulator,
        Random weedEntitySimulator,
        Random weedEntityPlacementSimulator,
        ref float currentPower,
        IDictionary<EnemyType, int> entitySpawnCounts,
        IDictionary<EnemyType, bool> entitySpawnedAtLeastOne,
        MoldSpreadManager moldSpreadManager,
        ref bool firstTimeSpawning,
        int currentHour,
        float currentDayTime
    )
    {
        var roundManager = Imperium.RoundManager;
        var spawning = new List<SpawnReport>();

        var moldCount = moldSpreadManager ? moldSpreadManager.generatedMold.Count : 0;

        // This is actually TimeOfDay.hour, maybe this will break
        if (moldCount <= 15 || currentHour < 3 || weedEntitySimulator.Next(0, 70) > moldCount)
        {
            return spawning;
        }

        var entityAmount = weedEntitySimulator.Next(1, 3);
        var timeUpToCurrentHour = Imperium.TimeOfDay.lengthOfHours * currentHour;

        for (var i = 0; i < entityAmount; i++)
        {
            var probabilities = new List<int>();
            for (var j = 0; j < roundManager.WeedEnemies.Count; j++)
            {
                var enemyType = roundManager.WeedEnemies[j].enemyType;

                if (firstTimeSpawning)
                {
                    entitySpawnCounts[enemyType] = 0;
                    entitySpawnedAtLeastOne[enemyType] = false;
                }

                if (enemyType.PowerLevel > 4f - currentPower ||
                    entitySpawnCounts[enemyType] >= enemyType.MaxCount || enemyType.spawningDisabled)
                {
                    probabilities.Add(0);
                    continue;
                }

                var probability = roundManager.increasedOutsideEnemySpawnRateIndex == j
                    ? 100
                    : !enemyType.useNumberSpawnedFalloff
                        ? (int)(roundManager.WeedEnemies[j].rarity *
                                enemyType.probabilityCurve.Evaluate(
                                    timeUpToCurrentHour / Imperium.TimeOfDay.totalTime)
                        )
                        : (int)(roundManager.WeedEnemies[j].rarity *
                                (enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour / Imperium.TimeOfDay.totalTime) *
                                 enemyType.numberSpawnedFalloff.Evaluate(entitySpawnCounts[enemyType] / 10f)));

                probabilities.Add(probability);
            }

            firstTimeSpawning = false;

            if (probabilities.Sum() <= 20) continue;

            var randomWeightedIndex = roundManager.GetRandomWeightedIndex(
                probabilities.ToArray(), weedEntitySimulator
            );
            var spawningEntity = roundManager.currentLevel.OutsideEnemies[randomWeightedIndex].enemyType;

            var spawnPoints = spawningEntity.WaterType switch
            {
                EnemyWaterType.WaterOnly => roundManager.outsideAIWaterNodes,
                EnemyWaterType.LandOnly => roundManager.outsideAIDryNodes,
                _ => roundManager.outsideAINodes
            };

            float groupSize = Mathf.Max(spawningEntity.spawnInGroupsOf, 1);
            for (var k = 0; k < groupSize; k++)
            {
                if (spawningEntity.PowerLevel > 4f - currentPower)
                {
                    break;
                }

                var position = spawnPoints[anomalySimulator.Next(0, spawnPoints.Length)].transform.position;
                position = roundManager.GetRandomNavMeshPositionInBoxPredictable(
                    position, 10f, default, weedEntityPlacementSimulator,
                    roundManager.GetLayermaskForEnemySizeLimit(spawningEntity)
                );
                position = PositionWithDenialPointsChecked(
                    position, spawnPoints, spawningEntity, -1, weedEntityPlacementSimulator
                );

                currentPower += spawningEntity.PowerLevel;

                entitySpawnCounts[spawningEntity]++;
                entitySpawnedAtLeastOne[spawningEntity] = true;

                spawning.Add(new SpawnReport
                {
                    Entity = spawningEntity,
                    Position = position,
                    SpawnTime = (int)currentDayTime
                });
            }
        }

        return spawning;
    }

    private static List<SpawnReport> SimulateOutdoorSpawnCycle(
        Random anomalySimulator,
        Random outsideEntitySimulator,
        Random outsideEntityPlacementSimulator,
        ref float currentPower,
        ref float currentPowerNoDeaths,
        ref int currentDiversityLevel,
        IDictionary<EnemyType, int> entitySpawnCounts,
        IDictionary<EnemyType, bool> entitySpawnedAtLeastOne,
        ref bool firstTimeSpawning,
        MoldSpreadManager moldSpreadManager,
        float normalizedTimeOfDay,
        int currentHour,
        float currentDayTime
    )
    {
        var roundManager = Imperium.RoundManager;
        var spawning = new List<SpawnReport>();

        if (currentPower > roundManager.currentMaxOutsidePower) return spawning;

        // Get time of next hour since AdvanceHourAndSpawnNewBatchOfEnemies increases currentHour before spawning
        var timeUpToCurrentHour = Imperium.TimeOfDay.lengthOfHours * currentHour;
        var baseEntityAmount = (int)(roundManager.currentLevel.outsideEnemySpawnChanceThroughDay.Evaluate(
            timeUpToCurrentHour / Imperium.TimeOfDay.totalTime
        ) * 100f) / 100f;
        if (Imperium.StartOfRound.isChallengeFile) baseEntityAmount += 1f;

        var lower = baseEntityAmount + Mathf.Abs(Imperium.TimeOfDay.daysUntilDeadline - 3) / 1.6f;
        var lowerBound = (int)(lower - 3f);
        var upperBound = (int)(baseEntityAmount + 3f);
        var entityAmount = Mathf.Clamp(
            outsideEntitySimulator.Next(Mathf.Min(lowerBound, upperBound), Mathf.Max(lowerBound, upperBound)),
            roundManager.minOutsideEnemiesToSpawn, 20
        );

        Imperium.IO.LogDebug($"[ORACLE] Spawn outdoor amount; base: {baseEntityAmount}, actual: {entityAmount}");

        for (var i = 0; i < entityAmount; i++)
        {
            var dynamicPower = currentPowerNoDeaths;
            var useNoDeathsPower = false;
            foreach (var entity in roundManager.currentLevel.OutsideEnemies)
            {
                if (firstTimeSpawning)
                {
                    entitySpawnCounts[entity.enemyType] = 0;
                    entitySpawnedAtLeastOne[entity.enemyType] = false;
                }

                var canSpawnInNoDeaths =
                    (
                        entitySpawnCounts[entity.enemyType] > 0 || entity.enemyType.DiversityPowerLevel <=
                        roundManager.currentMaxOutsideDiversityLevel - currentDiversityLevel
                    ) &&
                    entity.enemyType.PowerLevel <= roundManager.currentLevel.maxDaytimeEnemyPowerCount -
                    roundManager.currentDaytimeEnemyPowerNoDeaths &&
                    entitySpawnCounts[entity.enemyType] < entity.enemyType.MaxCount &&
                    entity.enemyType.normalizedTimeInDayToLeave >= normalizedTimeOfDay &&
                    !entity.enemyType.spawningDisabled;

                if (canSpawnInNoDeaths)
                {
                    useNoDeathsPower = true;
                    break;
                }
            }

            if (!useNoDeathsPower)
            {
                dynamicPower = currentPower;
            }

            var moldCount = moldSpreadManager ? moldSpreadManager.generatedMold.Count : 0;
            var probabilities = new List<int>();
            for (var j = 0; j < roundManager.currentLevel.OutsideEnemies.Count; j++)
            {
                var enemyType = roundManager.currentLevel.OutsideEnemies[j].enemyType;

                if (firstTimeSpawning)
                {
                    entitySpawnCounts[enemyType] = 0;
                    entitySpawnedAtLeastOne[enemyType] = false;
                }

                if (entitySpawnCounts[enemyType] <= 0 && enemyType.DiversityPowerLevel >
                    roundManager.currentMaxOutsideDiversityLevel - currentDiversityLevel ||
                    enemyType.PowerLevel > roundManager.currentMaxOutsidePower - currentPower ||
                    entitySpawnCounts[enemyType] >= enemyType.MaxCount || enemyType.spawningDisabled)
                {
                    probabilities.Add(0);
                    continue;
                }

                var probability = roundManager.increasedOutsideEnemySpawnRateIndex == j ? 100 :
                    !enemyType.useNumberSpawnedFalloff ? (int)(roundManager.currentLevel.OutsideEnemies[j].rarity *
                                                               enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour /
                                                                   Imperium.TimeOfDay.totalTime)) :
                    (int)(roundManager.currentLevel.OutsideEnemies[j].rarity *
                          (enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour / Imperium.TimeOfDay.totalTime) *
                           enemyType.numberSpawnedFalloff.Evaluate(entitySpawnCounts[enemyType] / 10f)));

                if (enemyType.spawnFromWeeds)
                {
                    probability = (int)Mathf.Clamp(probability * (moldCount / 60f), 0, 200);
                }

                probabilities.Add(probability);
            }

            firstTimeSpawning = false;

            if (probabilities.Sum() <= 0) continue;

            var randomWeightedIndex = roundManager.GetRandomWeightedIndex(
                probabilities.ToArray(), outsideEntitySimulator
            );
            var spawningEntity = roundManager.currentLevel.OutsideEnemies[randomWeightedIndex].enemyType;

            Imperium.IO.LogDebug($"[ORACLE] Picked outdoor entity: {spawningEntity.enemyName}");

            var spawnPoints = spawningEntity.WaterType switch
            {
                EnemyWaterType.WaterOnly => roundManager.outsideAIWaterNodes,
                EnemyWaterType.LandOnly => roundManager.outsideAIDryNodes,
                _ => roundManager.outsideAINodes
            };

            var spawnedBefore = entitySpawnedAtLeastOne[spawningEntity];

            float groupSize = Mathf.Max(spawningEntity.spawnInGroupsOf, 1);
            var spawnedAtLeastOne = false;
            for (var k = 0; k < groupSize; k++)
            {
                Imperium.IO.LogDebug(
                    $"[ORACLE] Dynamic Power: {dynamicPower}, Max: {roundManager.currentMaxOutsidePower}, Req: {spawningEntity.PowerLevel}"
                );

                if (spawningEntity.PowerLevel > roundManager.currentMaxOutsidePower - dynamicPower)
                {
                    break;
                }

                var position = spawnPoints[anomalySimulator.Next(0, spawnPoints.Length)].transform.position;
                position = roundManager.GetRandomNavMeshPositionInBoxPredictable(
                    position, 10f, default, outsideEntityPlacementSimulator,
                    roundManager.GetLayermaskForEnemySizeLimit(spawningEntity)
                );
                position = PositionWithDenialPointsChecked(
                    position, spawnPoints, spawningEntity, -1, outsideEntityPlacementSimulator
                );

                currentPower += spawningEntity.PowerLevel;
                currentPowerNoDeaths += spawningEntity.PowerLevel;
                dynamicPower += spawningEntity.PowerLevel;

                entitySpawnCounts[spawningEntity]++;

                spawning.Add(new SpawnReport
                {
                    Entity = spawningEntity,
                    Position = position,
                    SpawnTime = (int)currentDayTime
                });

                spawnedAtLeastOne = true;
            }

            if (spawnedAtLeastOne && !spawnedBefore)
            {
                currentDiversityLevel += spawningEntity.DiversityPowerLevel;
                entitySpawnedAtLeastOne[spawningEntity] = true;
            }
        }

        return spawning;
    }

    private static List<SpawnReport> SimulateDaytimeSpawnCycle(
        Random anomalySimulator,
        Random daytimeEntitySimulator,
        Random daytimeEntityPlacementSimulator,
        ref float currentPower,
        ref float currentPowerNoDeaths,
        IDictionary<EnemyType, int> entitySpawnCounts,
        IDictionary<EnemyType, bool> entitySpawnedAtLeastOne,
        ref bool firstTimeSpawning,
        float normalizedTimeOfDay,
        int currentHour,
        float currentDayTime
    )
    {
        var roundManager = Imperium.RoundManager;
        var spawning = new List<SpawnReport>();

        if (roundManager.currentLevel.DaytimeEnemies is not { Count: > 0 } ||
            currentPower > roundManager.currentLevel.maxDaytimeEnemyPowerCount)
        {
            return spawning;
        }

        // Get time of next hour since AdvanceHourAndSpawnNewBatchOfEnemies increases currentHour before spawning
        var timeUpToCurrentHour = Imperium.TimeOfDay.lengthOfHours * currentHour;
        var baseEntityAmount = roundManager.currentLevel.daytimeEnemySpawnChanceThroughDay.Evaluate(
            timeUpToCurrentHour / Imperium.TimeOfDay.totalTime
        );
        var entityAmount = Mathf.Clamp(
            daytimeEntitySimulator.Next((int)(baseEntityAmount - roundManager.currentLevel.daytimeEnemiesProbabilityRange),
                (int)(baseEntityAmount + roundManager.currentLevel.daytimeEnemiesProbabilityRange)),
            0, 20
        );
        // var spawnPoints = GameObject.FindGameObjectsWithTag("OutsideAINode");

        for (var i = 0; i < entityAmount; i++)
        {
            var dynamicPower = currentPowerNoDeaths;
            var useNoDeathsPower = false;
            foreach (var entity in roundManager.currentLevel.DaytimeEnemies)
            {
                if (firstTimeSpawning)
                {
                    entitySpawnCounts[entity.enemyType] = 0;
                    entitySpawnedAtLeastOne[entity.enemyType] = false;
                }

                var canSpawnInNoDeaths =
                    entity.enemyType.PowerLevel <= roundManager.currentLevel.maxDaytimeEnemyPowerCount -
                    roundManager.currentDaytimeEnemyPowerNoDeaths &&
                    entitySpawnCounts[entity.enemyType] < entity.enemyType.MaxCount &&
                    entity.enemyType.normalizedTimeInDayToLeave >= normalizedTimeOfDay &&
                    !entity.enemyType.spawningDisabled;

                if (canSpawnInNoDeaths)
                {
                    useNoDeathsPower = true;
                    break;
                }
            }

            if (!useNoDeathsPower)
            {
                dynamicPower = currentPower;
            }

            var probabilities = new List<int>();
            foreach (var entity in roundManager.currentLevel.DaytimeEnemies)
            {
                if (firstTimeSpawning)
                {
                    entitySpawnCounts[entity.enemyType] = 0;
                    entitySpawnedAtLeastOne[entity.enemyType] = false;
                }

                if (entity.enemyType.PowerLevel > roundManager.currentLevel.maxDaytimeEnemyPowerCount - dynamicPower ||
                    entitySpawnCounts[entity.enemyType] >= entity.enemyType.MaxCount ||
                    entity.enemyType.normalizedTimeInDayToLeave < currentDayTime / roundManager.timeScript.totalTime ||
                    entity.enemyType.spawningDisabled)
                {
                    probabilities.Add(0);
                    continue;
                }

                var probability = (int)(entity.rarity * entity.enemyType.probabilityCurve.Evaluate(
                    timeUpToCurrentHour / roundManager.timeScript.totalTime));
                probabilities.Add(probability);
            }

            firstTimeSpawning = false;

            if (probabilities.Sum() <= 0) break;

            var index = roundManager.GetRandomWeightedIndex(probabilities.ToArray(), daytimeEntitySimulator);
            var enemyType = roundManager.currentLevel.DaytimeEnemies[index].enemyType;

            float groupSize = Mathf.Max(enemyType.spawnInGroupsOf, 1);

            var spawnPoints = enemyType.WaterType switch
            {
                EnemyWaterType.WaterOnly => roundManager.outsideAIWaterNodes,
                EnemyWaterType.LandOnly => roundManager.outsideAIDryNodes,
                _ => roundManager.outsideAINodes
            };

            for (var j = 0; j < groupSize; j++)
            {
                if (enemyType.PowerLevel > roundManager.currentLevel.maxDaytimeEnemyPowerCount - dynamicPower)
                {
                    break;
                }

                var position = spawnPoints[anomalySimulator.Next(0, spawnPoints.Length)].transform.position;
                position = roundManager.GetRandomNavMeshPositionInBoxPredictable(
                    position, 10f, default, daytimeEntityPlacementSimulator,
                    roundManager.GetLayermaskForEnemySizeLimit(enemyType)
                );
                position = PositionWithDenialPointsChecked(
                    position, spawnPoints, enemyType, -1, daytimeEntityPlacementSimulator
                );

                currentPower += enemyType.PowerLevel;
                currentPowerNoDeaths += enemyType.PowerLevel;
                dynamicPower += enemyType.PowerLevel;

                entitySpawnCounts[enemyType]++;
                entitySpawnedAtLeastOne[enemyType] = true;

                spawning.Add(new SpawnReport
                {
                    Entity = enemyType,
                    Position = position,
                    SpawnTime = (int)currentDayTime
                });
            }
        }

        return spawning;
    }

    private static bool IndoorEnemyCannotBeSpawned(
        int entityIndex,
        float currentPowerLevel,
        int diversityLevel,
        IDictionary<EnemyType, int> entitySpawnCounts
    )
    {
        var currentLevel = Imperium.RoundManager.currentLevel;
        var entityType = currentLevel.Enemies[entityIndex].enemyType;

        if (
            (
                entitySpawnCounts[entityType] > 0 ||
                entityType.DiversityPowerLevel <= Imperium.RoundManager.currentMaxInsideDiversityLevel - diversityLevel
            ) &&
            !entityType.spawningDisabled
        )
        {
            if (!(entityType.PowerLevel > Imperium.RoundManager.currentMaxInsidePower - currentPowerLevel))
            {
                return entitySpawnCounts[entityType] >= entityType.MaxCount;
            }
        }

        return true;
    }

    private static Vector3 PositionWithDenialPointsChecked(
        Vector3 spawnPosition,
        IReadOnlyList<GameObject> spawnPoints,
        EnemyType enemyType,
        float distanceFromShip = -1f,
        Random randomSimulator = null
    )
    {
        if (spawnPoints.Count == 0) return spawnPosition;

        var num = 0;
        var flag = false;
        for (var i = 0; i < spawnPoints.Count - 1; i++)
        {
            foreach (var denialPoint in Imperium.RoundManager.spawnDenialPoints)
            {
                flag = true;
                if (Vector3.Distance(spawnPosition, denialPoint.transform.position) <
                    16f || distanceFromShip != -1 &&
                    Vector3.Distance(spawnPosition, Imperium.StartOfRound.shipLandingPosition.position) <
                    distanceFromShip)
                {
                    num = (num + 1) % spawnPoints.Count;
                    spawnPosition = spawnPoints[num].transform.position;
                    spawnPosition = Imperium.RoundManager.GetRandomNavMeshPositionInBoxPredictable(
                        spawnPosition, 10f, default, randomSimulator,
                        Imperium.RoundManager.GetLayermaskForEnemySizeLimit(enemyType)
                    );
                    flag = false;
                    break;
                }
            }

            if (flag) break;
        }

        return spawnPosition;
    }
}
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

    internal void Simulate() => Simulate(false, null);
    internal void Resimulate(string reason) => Simulate(false, reason);

    internal void Simulate(bool initial, string reason)
    {
        ImpUtils.RunSafe(() => StartCoroutine(simulateUnsafe(initial, reason)), "Oracle simulation failed");
    }

    private IEnumerator simulateUnsafe(bool initial, string reason)
    {
        if (!Imperium.IsSceneLoaded.Value) yield break;

        yield return 0;

        var currentHour = Reflection.Get<RoundManager, int>(Imperium.RoundManager, "currentHour");
        if (!initial) currentHour += Imperium.RoundManager.hourTimeBetweenEnemySpawnBatches;

        var AnomalySimulator = ImpUtils.CloneRandom(Imperium.RoundManager.AnomalyRandom);
        var EntitySimulator = ImpUtils.CloneRandom(Imperium.RoundManager.EnemySpawnRandom);
        var OutsideEnemySpawnSimulator = ImpUtils.CloneRandom(Imperium.RoundManager.OutsideEnemySpawnRandom);
        var WeedEntitySimulator = ImpUtils.CloneRandom(Imperium.RoundManager.WeedEnemySpawnRandom);

        var roundManager = Imperium.RoundManager;
        var currentLevel = roundManager.currentLevel;

        // Variables that change in cycles
        var currentTime = Imperium.TimeOfDay.currentDayTime;

        var indoorPower = roundManager.currentEnemyPower;
        var outdoorPower = roundManager.currentOutsideEnemyPower;
        var daytimePower = roundManager.currentDaytimeEnemyPower;
        var cannotSpawnMoreInsideEnemies = roundManager.cannotSpawnMoreInsideEnemies;

        var indoorEntityCounts = currentLevel.Enemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.numberSpawned);
        var outdoorEntityCounts = currentLevel.OutsideEnemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.numberSpawned);
        var daytimeEntityCounts = currentLevel.DaytimeEnemies
            .Distinct()
            .ToDictionary(entity => entity.enemyType, entity => entity.enemyType.numberSpawned);

        var firstTimeSpawningEnemies = Reflection.Get<RoundManager, bool>(
            roundManager, "firstTimeSpawningEnemies"
        );
        var firstTimeSpawningOutsideEnemies = Reflection.Get<RoundManager, bool>(
            roundManager, "firstTimeSpawningOutsideEnemies"
        );
        var firstTimeSpawningWeedEnemies = Reflection.Get<RoundManager, bool>(
            roundManager, "firstTimeSpawningWeedEnemies"
        );
        var firstTimeSpawningDaytimeEnemies = Reflection.Get<RoundManager, bool>(
            roundManager, "firstTimeSpawningDaytimeEnemies"
        );

        // Cycle Variables
        State.Value.CurrentCycle = Mathf.RoundToInt(
            currentHour * Imperium.TimeOfDay.lengthOfHours / Imperium.TimeOfDay.totalTime * 9
        );

        for (var i = State.Value.CurrentCycle; i <= 9; i++)
        {
            State.Value.Cycles[i].CycleTime = currentTime;

            if (!Imperium.MoonManager.DaytimeSpawningPaused.Value)
            {
                State.Value.DaytimeCycles[i] = SimulateDaytimeSpawnCycle(
                    AnomalySimulator, EntitySimulator,
                    ref daytimePower,
                    daytimeEntityCounts,
                    ref firstTimeSpawningDaytimeEnemies,
                    currentHour, currentTime
                );
            }

            if (!Imperium.MoonManager.OutdoorSpawningPaused.Value)
            {
                State.Value.OutdoorCycles[i] = SimulateOutdoorSpawnCycle(
                    AnomalySimulator, OutsideEnemySpawnSimulator,
                    ref outdoorPower,
                    outdoorEntityCounts,
                    ref firstTimeSpawningOutsideEnemies,
                    currentHour, currentTime
                );

                State.Value.OutdoorCycles[i] = State.Value.OutdoorCycles[i].Concat(SimulateWeedSpawnCycle(
                    AnomalySimulator, WeedEntitySimulator,
                    ref outdoorPower,
                    outdoorEntityCounts,
                    ref firstTimeSpawningWeedEnemies,
                    currentHour, currentTime
                )).ToList();
            }

            var spawnTimes = new List<int>();
            if (!Imperium.MoonManager.IndoorSpawningPaused.Value)
            {
                State.Value.IndoorCycles[i] = SimulateIndoorSpawnCycle(
                    AnomalySimulator, EntitySimulator,
                    ref indoorPower,
                    indoorEntityCounts,
                    ref cannotSpawnMoreInsideEnemies,
                    ref firstTimeSpawningEnemies,
                    currentHour, currentTime
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

        yield return 0;

        State.Refresh();
    }

    private static List<SpawnReport> SimulateIndoorSpawnCycle(
        Random anomalySimulator,
        Random entitySimulator,
        ref float currentPower,
        IDictionary<EnemyType, int> entitySpawnCounts,
        ref bool cannotSpawnMoreInsideEnemies,
        ref bool firstTimeSpawning,
        int currentHour,
        float currentDayTime
    )
    {
        var roundManager = Imperium.RoundManager;
        var spawning = new List<SpawnReport>();
        var freeVents = Imperium.RoundManager.allEnemyVents.Where(t => !t.occupied).ToList();
        var timeUpToCurrentHour = Imperium.TimeOfDay.lengthOfHours * currentHour;

        if (!freeVents.Any() || cannotSpawnMoreInsideEnemies) return spawning;

        // Get time of next hour since AdvanceHourAndSpawnNewBatchOfEnemies increases currentHour before spawning
        var baseEntityAmount = roundManager.currentLevel.enemySpawnChanceThroughoutDay.Evaluate(
            currentDayTime / roundManager.timeScript.totalTime);
        if (StartOfRound.Instance.isChallengeFile) baseEntityAmount += 1f;

        var lower = baseEntityAmount + Mathf.Abs(Imperium.TimeOfDay.daysUntilDeadline - 3) / 1.6f;
        var lowerBound = (int)(lower - roundManager.currentLevel.spawnProbabilityRange);
        var upperBound = (int)(baseEntityAmount + roundManager.currentLevel.spawnProbabilityRange);
        var entityAmount = Mathf.Clamp(anomalySimulator.Next(
            Math.Min(lowerBound, upperBound),
            Math.Max(lowerBound, upperBound)
        ), roundManager.minEnemiesToSpawn, 20);

        entityAmount = Mathf.Clamp(entityAmount, 0, freeVents.Count);

        if (currentPower >= roundManager.currentMaxInsidePower)
        {
            cannotSpawnMoreInsideEnemies = true;
            return spawning;
        }

        for (var i = 0; i < entityAmount; i++)
        {
            var probabilities = new List<int>();
            var spawnTime = anomalySimulator.Next(
                (int)(10f + timeUpToCurrentHour),
                (int)(Imperium.TimeOfDay.lengthOfHours * roundManager.hourTimeBetweenEnemySpawnBatches +
                      timeUpToCurrentHour)
            );
            var spawnVentIndex = anomalySimulator.Next(freeVents.Count);
            var spawnVent = freeVents[spawnVentIndex];

            for (var j = 0; j < roundManager.currentLevel.Enemies.Count; j++)
            {
                var enemyType = roundManager.currentLevel.Enemies[j].enemyType;

                var entityCannotBeSpawned = enemyType.spawningDisabled ||
                                            enemyType.PowerLevel > roundManager.currentMaxInsidePower - currentPower ||
                                            entitySpawnCounts[enemyType] >= enemyType.MaxCount;

                if (firstTimeSpawning)
                {
                    firstTimeSpawning = false;
                    entitySpawnCounts[enemyType] = 0;
                }

                if (entityCannotBeSpawned)
                {
                    probabilities.Add(0);
                    continue;
                }

                var probability = roundManager.increasedInsideEnemySpawnRateIndex == j
                    ? 100
                    : !enemyType.useNumberSpawnedFalloff
                        ? (int)(roundManager.currentLevel.Enemies[j].rarity *
                                enemyType.probabilityCurve.Evaluate(currentDayTime / roundManager.timeScript.totalTime)
                        )
                        : (int)(roundManager.currentLevel.Enemies[j].rarity * (
                                enemyType.probabilityCurve.Evaluate(currentDayTime / roundManager.timeScript.totalTime) *
                                enemyType.numberSpawnedFalloff.Evaluate(entitySpawnCounts[enemyType] / 10f)
                            )
                        );

                if (enemyType.increasedChanceInterior != -1 &&
                    roundManager.currentDungeonType != enemyType.increasedChanceInterior)
                {
                    probability = (int)Mathf.Min(probability * 1.7f, 100);
                }

                probabilities.Add(probability);
            }

            if (probabilities.Sum() == 0) continue;
            var index = roundManager.GetRandomWeightedIndex(probabilities.ToArray(), entitySimulator);

            var spawningEntity = roundManager.currentLevel.Enemies[index].enemyType;
            currentPower += spawningEntity.PowerLevel;
            entitySpawnCounts[spawningEntity]++;
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

    private static List<SpawnReport> SimulateOutdoorSpawnCycle(
        Random anomalySimulator,
        Random outsideEntitySimulator,
        ref float currentPower,
        IDictionary<EnemyType, int> entitySpawnCounts,
        ref bool firstTimeSpawning,
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
            roundManager.minOutsideEnemiesToSpawn, 20);

        var spawnPoints = GameObject.FindGameObjectsWithTag("OutsideAINode");

        for (var i = 0; i < entityAmount; i++)
        {
            var probabilities = new List<int>();
            for (var j = 0; j < roundManager.currentLevel.OutsideEnemies.Count; j++)
            {
                var enemyType = roundManager.currentLevel.OutsideEnemies[j].enemyType;

                if (firstTimeSpawning)
                {
                    firstTimeSpawning = false;
                    entitySpawnCounts[enemyType] = 0;
                }

                if (enemyType.PowerLevel > roundManager.currentMaxOutsidePower - currentPower ||
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
                probabilities.Add(probability);
            }

            if (probabilities.Sum() == 0) continue;

            var randomWeightedIndex = roundManager.GetRandomWeightedIndex(
                probabilities.ToArray(), outsideEntitySimulator
            );
            var spawningEntity = roundManager.currentLevel.OutsideEnemies[randomWeightedIndex].enemyType;

            if (spawningEntity.requireNestObjectsToSpawn)
            {
                var nests = FindObjectsByType<EnemyAINestSpawnObject>(FindObjectsSortMode.None);
                if (nests.All(t => t.enemyType != spawningEntity)) continue;
            }

            float groupSize = Mathf.Max(spawningEntity.spawnInGroupsOf, 1);
            for (var k = 0; k < groupSize; k++)
            {
                if (spawningEntity.PowerLevel > roundManager.currentMaxOutsidePower - currentPower)
                {
                    break;
                }

                var position = spawnPoints[anomalySimulator.Next(0, spawnPoints.Length)].transform.position;
                position = roundManager.GetRandomNavMeshPositionInBoxPredictable(
                    position, 10f, default, anomalySimulator,
                    roundManager.GetLayermaskForEnemySizeLimit(spawningEntity)
                );
                position = PositionWithDenialPointsChecked(position, spawnPoints, spawningEntity, anomalySimulator);

                currentPower += spawningEntity.PowerLevel;
                entitySpawnCounts[spawningEntity]++;

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

    private static List<SpawnReport> SimulateWeedSpawnCycle(
        Random anomalySimulator,
        Random weedEntitySimulator,
        ref float currentPower,
        IDictionary<EnemyType, int> entitySpawnCounts,
        ref bool firstTimeSpawning,
        int currentHour,
        float currentDayTime
    )
    {
        var roundManager = Imperium.RoundManager;
        var spawning = new List<SpawnReport>();

        var moldSpreadManager = FindObjectOfType<MoldSpreadManager>();
        var moldCount = moldSpreadManager ? moldSpreadManager.generatedMold.Count : 0;
        if (moldCount <= 30 || weedEntitySimulator.Next(0, 80) > moldCount) return spawning;

        var timeUpToCurrentHour = Imperium.TimeOfDay.lengthOfHours * currentHour;
        var entityAmount = weedEntitySimulator.Next(1, 3);

        var spawnPoints = GameObject.FindGameObjectsWithTag("OutsideAINode");

        for (var i = 0; i < entityAmount; i++)
        {
            var probabilities = new List<int>();
            var totalProbability = 0;
            for (var j = 0; j < roundManager.WeedEnemies.Count; j++)
            {
                var enemyType = roundManager.WeedEnemies[j].enemyType;

                if (firstTimeSpawning)
                {
                    entitySpawnCounts[enemyType] = 0;
                }

                if (enemyType.PowerLevel > roundManager.currentMaxOutsidePower - currentPower ||
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
                    probability = (int)Mathf.Clamp(probability * (probability / 60f), 0f, 200f);
                }

                probabilities.Add(probability);
                totalProbability += probability;
            }

            firstTimeSpawning = false;

            if (totalProbability <= 20) break;

            if (probabilities.Sum() == 0) continue;

            var randomWeightedIndex = roundManager.GetRandomWeightedIndex(
                probabilities.ToArray(), weedEntitySimulator
            );
            var spawningEntity = roundManager.WeedEnemies[randomWeightedIndex].enemyType;
            float groupSize = Mathf.Max(spawningEntity.spawnInGroupsOf, 1);

            var result = false;
            for (var k = 0; k < groupSize; k++)
            {
                if (spawningEntity.PowerLevel > roundManager.currentMaxOutsidePower - currentPower)
                {
                    break;
                }

                var position = spawnPoints[anomalySimulator.Next(0, spawnPoints.Length)].transform.position;
                position = roundManager.GetRandomNavMeshPositionInBoxPredictable(
                    position, 10f, default, anomalySimulator,
                    roundManager.GetLayermaskForEnemySizeLimit(spawningEntity)
                );
                position = PositionWithDenialPointsChecked(position, spawnPoints, spawningEntity, anomalySimulator);

                currentPower += spawningEntity.PowerLevel;
                entitySpawnCounts[spawningEntity]++;

                spawning.Add(new SpawnReport
                {
                    Entity = spawningEntity,
                    Position = position,
                    SpawnTime = (int)currentDayTime
                });

                result = true;
            }

            if (!result) break;
        }

        return spawning;
    }

    private static List<SpawnReport> SimulateDaytimeSpawnCycle(
        Random anomalySimulator,
        Random entitySimulator,
        ref float currentPower,
        IDictionary<EnemyType, int> entitySpawnCounts,
        ref bool firstTimeSpawning,
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
            anomalySimulator.Next((int)(baseEntityAmount - roundManager.currentLevel.daytimeEnemiesProbabilityRange),
                (int)(baseEntityAmount + roundManager.currentLevel.daytimeEnemiesProbabilityRange)),
            0, 20
        );
        var spawnPoints = GameObject.FindGameObjectsWithTag("OutsideAINode");

        for (var i = 0; i < entityAmount; i++)
        {
            var probabilities = new List<int>();
            foreach (var entity in roundManager.currentLevel.DaytimeEnemies)
            {
                if (firstTimeSpawning)
                {
                    firstTimeSpawning = false;
                    entitySpawnCounts[entity.enemyType] = 0;
                }

                if (entity.enemyType.PowerLevel > roundManager.currentLevel.maxDaytimeEnemyPowerCount - currentPower ||
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

            // Breaks here since SpawnRandomDaytimeEnemy should return false when all probabilities = 0
            if (probabilities.Sum() == 0) break;

            var index = roundManager.GetRandomWeightedIndex(probabilities.ToArray(), entitySimulator);
            var enemyType = roundManager.currentLevel.DaytimeEnemies[index].enemyType;

            float groupSize = Mathf.Max(enemyType.spawnInGroupsOf, 1);


            for (var j = 0; j < groupSize; j++)
            {
                if (enemyType.PowerLevel > roundManager.currentLevel.maxDaytimeEnemyPowerCount - currentPower)
                {
                    break;
                }

                var position = spawnPoints[anomalySimulator.Next(0, spawnPoints.Length)].transform.position;
                position = roundManager.GetRandomNavMeshPositionInBoxPredictable(
                    position, 10f, default, entitySimulator,
                    roundManager.GetLayermaskForEnemySizeLimit(enemyType)
                );
                position = PositionWithDenialPointsChecked(position, spawnPoints, enemyType, anomalySimulator);

                currentPower += enemyType.PowerLevel;
                entitySpawnCounts[enemyType]++;

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

    private static Vector3 PositionWithDenialPointsChecked(
        Vector3 spawnPosition,
        IReadOnlyList<GameObject> spawnPoints,
        EnemyType enemyType,
        Random randomSimulator
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
                    16f)
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
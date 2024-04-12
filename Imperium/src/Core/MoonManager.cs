#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Netcode;
using Imperium.Types;
using Imperium.Util;

#endregion

namespace Imperium.Core;

/// <summary>
///     A moon manager for every moon is instantiated at the start of the game, it holds the spawn lists as well
///     as a copy of the original vanilla data used to reset values.
/// </summary>
public class MoonManager
{
    // This is used to keep track of entities that spawn in the current level, every entity that does not
    // spawn natively in vanilla is added with a 0 rarity
    internal readonly Dictionary<string, SpawnableEnemyWithRarity> IndoorEntities;
    internal readonly Dictionary<string, SpawnableEnemyWithRarity> OutdoorEntities;
    internal readonly Dictionary<string, SpawnableEnemyWithRarity> DaytimeEntities;
    internal readonly Dictionary<string, SpawnableItemWithRarity> Scrap;

    // Entities that spawn in the current level in vanilla
    private readonly Dictionary<string, SpawnableEnemyWithRarity> NativeIndoorEntities;
    private readonly Dictionary<string, SpawnableEnemyWithRarity> NativeOutdoorEntities;
    private readonly Dictionary<string, SpawnableEnemyWithRarity> NativeDaytimeEntities;
    private readonly Dictionary<string, SpawnableItemWithRarity> NativeScrap;

    // Original moon values for resetting functionality
    internal readonly MoonData OriginalMoonData;

    // Selectable level the moon manager is representing
    private readonly SelectableLevel level;

    private static MoonManager[] MoonManagers;
    public static MoonManager Current => MoonManagers[Imperium.StartOfRound.currentLevelID];

    // Stores the amount of scrap spawned and the amount of scrap spawned if the level is a challenge moon
    // Note: This is being simulated before the actual calculation in RoundManager.SpawnScrapInLevel() happens
    public int ScrapAmount;
    public int ChallengeScrapAmount;

    internal static void Create(ObjectManager objectManager)
    {
        MoonManagers = new MoonManager[Imperium.StartOfRound.levels.Length];
        for (var i = 0; i < Imperium.StartOfRound.levels.Length; i++)
        {
            MoonManagers[i] = new MoonManager(Imperium.StartOfRound.levels[i], objectManager);
        }
    }

    private MoonManager(SelectableLevel level, ObjectManager objectManager)
    {
        this.level = level;
        // Gets all the original spawn values, grouped by name to account for duplicates (e.g. Bottles on Assurance)
        OriginalMoonData = new MoonData
        {
            IndoorEntityRarities = this.level.Enemies
                .GroupBy(entity => entity.enemyType)
                .ToDictionary(entry => entry.Key.enemyName, entry => entry.Sum(entity => entity.rarity)),
            OutdoorEntityRarities = this.level.OutsideEnemies
                .GroupBy(entity => entity.enemyType)
                .ToDictionary(entry => entry.Key.enemyName, entry => entry.Sum(entity => entity.rarity)),
            DaytimeEntityRarities = this.level.DaytimeEnemies
                .GroupBy(entity => entity.enemyType)
                .ToDictionary(entry => entry.Key.enemyName, entry => entry.Sum(entity => entity.rarity)),
            ScrapRarities = this.level.spawnableScrap
                .GroupBy(scrap => scrap.spawnableItem)
                .ToDictionary(entry => entry.Key.itemName, entry => entry.Sum(scrap => scrap.rarity)),
            maxIndoorPower = level.maxEnemyPowerCount,
            maxOutdoorPower = level.maxOutsideEnemyPowerCount,
            maxDaytimePower = level.maxDaytimeEnemyPowerCount,
            indoorDeviation = level.daytimeEnemiesProbabilityRange,
            daytimeDeviation = level.daytimeEnemiesProbabilityRange
        };

        NativeIndoorEntities = level.Enemies
            .GroupBy(entity => entity.enemyType.enemyName)
            .ToDictionary(entry => entry.Key, entry => entry.First());
        NativeOutdoorEntities = level.OutsideEnemies
            .GroupBy(entity => entity.enemyType.enemyName)
            .ToDictionary(entry => entry.Key, entry => entry.First());
        NativeDaytimeEntities = level.DaytimeEnemies
            .GroupBy(entity => entity.enemyType.enemyName)
            .ToDictionary(entry => entry.Key, entry => entry.First());
        NativeScrap = level.spawnableScrap
            .GroupBy(scrap => scrap.spawnableItem.itemName)
            .ToDictionary(entry => entry.Key, entry => entry.First());

        // Generates all spawn lists with all spawnable objects and their current level rarities
        // objects that don't natively spawn have their rarity = 0 and are added to the game level's spawn list
        IndoorEntities = objectManager.AllIndoorEntities.Value
            .ToDictionary(
                value => value.Key,
                value => NativeIndoorEntities
                    .TryGetValue(value.Key, out var entity)
                    ? entity
                    : ImpUtils.AddEntityToSpawnList(value.Value, level.Enemies));
        OutdoorEntities = objectManager.AllOutdoorEntities.Value
            .ToDictionary(
                value => value.Key,
                value => NativeOutdoorEntities
                    .TryGetValue(value.Key, out var entity)
                    ? entity
                    : ImpUtils.AddEntityToSpawnList(value.Value, level.OutsideEnemies));
        DaytimeEntities = objectManager.AllDaytimeEntities.Value
            .ToDictionary(
                value => value.Key,
                value => NativeDaytimeEntities
                    .TryGetValue(value.Key, out var entity)
                    ? entity
                    : ImpUtils.AddEntityToSpawnList(value.Value, level.DaytimeEnemies));

        Scrap = objectManager.AllScrap.Value
            .ToDictionary(
                value => value.Key,
                value => NativeScrap
                    .TryGetValue(value.Key, out var scrap)
                    ? scrap
                    : ImpUtils.AddScrapToSpawnList(value.Value, level.spawnableScrap));
    }

    internal bool IsEntityNative(string entityName)
    {
        return NativeIndoorEntities.ContainsKey(entityName)
               || NativeOutdoorEntities.ContainsKey(entityName)
               || NativeDaytimeEntities.ContainsKey(entityName);
    }

    internal bool IsScrapNative(string scrapName) => NativeScrap.ContainsKey(scrapName);

    internal void ResetIndoorEntities()
    {
        foreach (var entity in IndoorEntities.Values)
        {
            entity.rarity = OriginalMoonData.IndoorEntityRarities.GetValueOrDefault(entity.enemyType.enemyName, 0);
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void ResetOutdoorEntities()
    {
        foreach (var entity in OutdoorEntities.Values)
        {
            entity.rarity = OriginalMoonData.OutdoorEntityRarities.GetValueOrDefault(entity.enemyType.enemyName, 0);
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void ResetDaytimeEntities()
    {
        foreach (var entity in DaytimeEntities.Values)
        {
            entity.rarity = OriginalMoonData.DaytimeEntityRarities.GetValueOrDefault(entity.enemyType.enemyName, 0);
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void ResetScrap()
    {
        foreach (var scrap in level.spawnableScrap)
        {
            scrap.rarity = OriginalMoonData.ScrapRarities.GetValueOrDefault(scrap.spawnableItem.itemName, 0);
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void EqualIndoorEntities()
    {
        foreach (var entity in IndoorEntities.Values)
        {
            entity.rarity = 100;
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void EqualOutdoorEntities()
    {
        foreach (var entity in OutdoorEntities.Values)
        {
            entity.rarity = 100;
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void EqualDaytimeEntities()
    {
        foreach (var entity in DaytimeEntities.Values)
        {
            entity.rarity = 100;
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void EqualScrap()
    {
        foreach (var scrap in Scrap.Values)
        {
            scrap.rarity = 100;
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }
}
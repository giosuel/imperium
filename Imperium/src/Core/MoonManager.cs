#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Netcode;
using Imperium.Types;

#endregion

namespace Imperium.Core;

/// <summary>
///     A moon manager for every moon is instantiated at the start of the game, it holds the spawn lists as well
///     as a copy of the original vanilla data used to reset values.
/// </summary>
public class MoonManager
{
    // Original moon values for resetting functionality
    internal readonly MoonData OriginalMoonData;

    // Selectable level the moon manager is representing
    internal readonly SelectableLevel Level;

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
        Level = level;
        // Gets all the original spawn values, grouped by name to account for duplicates (e.g. Bottles on Assurance)
        OriginalMoonData = new MoonData
        {
            IndoorEntityRarities = Level.Enemies
                .GroupBy(entity => entity.enemyType)
                .ToDictionary(entry => entry.Key, entry => entry.Sum(entity => entity.rarity)),
            OutdoorEntityRarities = Level.OutsideEnemies
                .GroupBy(entity => entity.enemyType)
                .ToDictionary(entry => entry.Key, entry => entry.Sum(entity => entity.rarity)),
            DaytimeEntityRarities = Level.DaytimeEnemies
                .GroupBy(entity => entity.enemyType)
                .ToDictionary(entry => entry.Key, entry => entry.Sum(entity => entity.rarity)),
            ScrapRarities = Level.spawnableScrap
                .GroupBy(scrap => scrap.spawnableItem)
                .ToDictionary(entry => entry.Key, entry => entry.Sum(scrap => scrap.rarity)),
            maxIndoorPower = level.maxEnemyPowerCount,
            maxOutdoorPower = level.maxOutsideEnemyPowerCount,
            maxDaytimePower = level.maxDaytimeEnemyPowerCount,
            indoorDeviation = level.daytimeEnemiesProbabilityRange,
            daytimeDeviation = level.daytimeEnemiesProbabilityRange
        };

        // Add all entities and scrap, that are not native in the current level, to the spawn lists with a rarity of 0
        objectManager.AllIndoorEntities.Value
            .Where(entity => !OriginalMoonData.IndoorEntityRarities.ContainsKey(entity))
            .ToList()
            .ForEach(
                entity => level.Enemies.Add(new SpawnableEnemyWithRarity { enemyType = entity, rarity = 0 })
            );

        objectManager.AllOutdoorEntities.Value
            .Where(entity => !OriginalMoonData.OutdoorEntityRarities.ContainsKey(entity))
            .ToList()
            .ForEach(
                entity => level.OutsideEnemies.Add(new SpawnableEnemyWithRarity { enemyType = entity, rarity = 0 })
            );

        objectManager.AllDaytimeEntities.Value
            .Where(entity => !OriginalMoonData.DaytimeEntityRarities.ContainsKey(entity))
            .ToList()
            .ForEach(
                entity => level.DaytimeEnemies.Add(new SpawnableEnemyWithRarity { enemyType = entity, rarity = 0 })
            );

        objectManager.AllScrap.Value
            .Where(entity => !OriginalMoonData.ScrapRarities.ContainsKey(entity))
            .ToList()
            .ForEach(
                item => level.spawnableScrap.Add(new SpawnableItemWithRarity { spawnableItem = item, rarity = 0 })
            );
    }

    internal bool IsEntityNative(EnemyType entity)
    {
        return OriginalMoonData.IndoorEntityRarities.ContainsKey(entity)
               || OriginalMoonData.OutdoorEntityRarities.ContainsKey(entity)
               || OriginalMoonData.DaytimeEntityRarities.ContainsKey(entity);
    }

    internal bool IsScrapNative(Item scrap) => OriginalMoonData.ScrapRarities.ContainsKey(scrap);

    internal void ResetIndoorEntities()
    {
        foreach (var entity in Level.Enemies)
        {
            entity.rarity = OriginalMoonData.IndoorEntityRarities.GetValueOrDefault(entity.enemyType, 0);
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void ResetOutdoorEntities()
    {
        foreach (var entity in Level.OutsideEnemies)
        {
            entity.rarity = OriginalMoonData.OutdoorEntityRarities.GetValueOrDefault(entity.enemyType, 0);
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void ResetDaytimeEntities()
    {
        foreach (var entity in Level.DaytimeEnemies)
        {
            entity.rarity = OriginalMoonData.DaytimeEntityRarities.GetValueOrDefault(entity.enemyType, 0);
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void ResetScrap()
    {
        foreach (var scrap in Level.spawnableScrap)
        {
            scrap.rarity = OriginalMoonData.ScrapRarities.GetValueOrDefault(scrap.spawnableItem, 0);
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void EqualIndoorEntities()
    {
        foreach (var entity in Level.Enemies)
        {
            entity.rarity = 100;
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void EqualOutdoorEntities()
    {
        foreach (var entity in Level.OutsideEnemies)
        {
            entity.rarity = 100;
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void EqualDaytimeEntities()
    {
        foreach (var entity in Level.DaytimeEnemies)
        {
            entity.rarity = 100;
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void EqualScrap()
    {
        foreach (var scrap in Level.spawnableScrap)
        {
            scrap.rarity = 100;
        }

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal static void ToggleDoors(bool isOpen)
    {
        Imperium.ObjectManager.CurrentLevelDoors.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(door =>
            {
                var animation = door.gameObject.GetComponent<AnimatedObjectTrigger>();
                animation.TriggerAnimation(Imperium.Player);
                door.isDoorOpened = isOpen;
                door.navMeshObstacle.enabled = !isOpen;
            });
    }

    internal static void ToggleDoorLocks(bool isLocked)
    {
        Imperium.ObjectManager.CurrentLevelDoors.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(door =>
            {
                if (isLocked && !door.isLocked)
                {
                    door.LockDoor();
                }
                else if (door.isLocked)
                {
                    door.UnlockDoor();
                }
            });
    }

    internal static void ToggleSecurityDoors(bool isOn)
    {
        Imperium.ObjectManager.CurrentLevelSecurityDoors.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(door => door.OnPowerSwitch(!isOn));
    }

    internal static void ToggleTurrets(bool isOn)
    {
        Imperium.ObjectManager.CurrentLevelTurrets.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(turret => turret.ToggleTurretEnabled(isOn));
    }

    internal static void ToggleLandmines(bool isOn)
    {
        Imperium.ObjectManager.CurrentLevelLandmines.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(mine => mine.ToggleMine(isOn));
    }

    internal static void ToggleBreakers(bool isOn)
    {
        Imperium.ObjectManager.CurrentLevelBreakerBoxes.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(box =>
            {
                foreach (var breakerSwitch in box.breakerSwitches)
                {
                    var animation = breakerSwitch.gameObject.GetComponent<AnimatedObjectTrigger>();
                    if (animation.boolValue != isOn)
                    {
                        animation.boolValue = isOn;
                        animation.setInitialState = isOn;
                        breakerSwitch.SetBool("turnedLeft", isOn);
                        box.SwitchBreaker(isOn);
                    }
                }
            });
    }
}
#region

using System.Collections.Generic;
using GameNetcodeStuff;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.API;

/// <summary>
///     Provides lists that are bound to game objects.
///     These lists are consistent with vanilla and Imperium modfications.
///     Instantiation of new items by other mods are NOT detected. Lists may hold null items.
/// </summary>
public static class Resources
{
    #region Static Resources

    /// <summary>
    ///     List of all loaded objects of the <see cref="EnemyType" /> type.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<EnemyType>> LoadedEntities
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<EnemyType>>.Wrap(Imperium.ObjectManager.LoadedEntities);
        }
    }

    /// <summary>
    ///     List of all loaded objects of the <see cref="Item" /> type.
    ///     Excludes all items which names are listed in <see cref="ImpConstants.ItemBlacklist" />.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<Item>> LoadedItems
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<Item>>.Wrap(Imperium.ObjectManager.LoadedItems);
        }
    }

    /// <summary>
    ///     List of all loaded objects of the <see cref="Item" /> type, that also have the <see cref="Item.isScrap" /> flag.
    ///     Excludes all items which names are listed in <see cref="ImpConstants.ItemBlacklist" />.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<Item>> LoadedScrap
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<Item>>.Wrap(Imperium.ObjectManager.LoadedScrap);
        }
    }

    #endregion

    #region Level Resources

    /// <summary>
    ///     List of all the entities in the current level.
    ///     This is updated whenever
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<EnemyAI>> CurrentEntities
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<EnemyAI>>.Wrap(Imperium.ObjectManager.CurrentLevelEntities);
        }
    }

    /// <summary>
    ///     List of all the items and scrap in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<GrabbableObject>> CurrentItems
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<GrabbableObject>>.Wrap(Imperium.ObjectManager.CurrentLevelItems);
        }
    }

    /// <summary>
    ///     List of all the players in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<PlayerControllerB>> CurrentPlayers
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<PlayerControllerB>>.Wrap(Imperium.ObjectManager.CurrentPlayers);
        }
    }

    /// <summary>
    ///     List of all the doors in the current scene.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<DoorLock>> CurrentLevelDoors
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<DoorLock>>.Wrap(Imperium.ObjectManager.CurrentLevelDoors);
        }
    }

    /// <summary>
    ///     List of all the security doors in the current scene.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<TerminalAccessibleObject>> CurrentLevelSecurityDoors
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<TerminalAccessibleObject>>.Wrap(
                Imperium.ObjectManager.CurrentLevelSecurityDoors
            );
        }
    }

    /// <summary>
    ///     List of all the turrets in the current scene.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<Turret>> CurrentLevelTurrets
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<Turret>>.Wrap(Imperium.ObjectManager.CurrentLevelTurrets);
        }
    }

    /// <summary>
    ///     List of all the landmines in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<Landmine>> CurrentLevelLandmines
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<Landmine>>.Wrap(Imperium.ObjectManager.CurrentLevelLandmines);
        }
    }

    /// <summary>
    ///     List of all the spike traps in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<SpikeRoofTrap>> CurrentLevelSpikeTraps
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<SpikeRoofTrap>>.Wrap(Imperium.ObjectManager.CurrentLevelSpikeTraps);
        }
    }

    /// <summary>
    ///     List of all the breaker boxes in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<BreakerBox>> CurrentLevelBreakerBoxes
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<BreakerBox>>.Wrap(Imperium.ObjectManager.CurrentLevelBreakerBoxes);
        }
    }

    /// <summary>
    ///     List of all the steam valves in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<SteamValveHazard>> CurrentLevelSteamValves
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<SteamValveHazard>>.Wrap(
                Imperium.ObjectManager.CurrentLevelSteamValves
            );
        }
    }

    /// <summary>
    ///     List of all the vents in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<EnemyVent>> CurrentLevelVents
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<EnemyVent>>.Wrap(Imperium.ObjectManager.CurrentLevelVents);
        }
    }

    /// <summary>
    ///     List of all the spider webs in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<SandSpiderWebTrap>> CurrentLevelSpiderWebs
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<SandSpiderWebTrap>>.Wrap(
                Imperium.ObjectManager.CurrentLevelSpiderWebs
            );
        }
    }

    /// <summary>
    ///     List of all the company cruisers in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpImmutableBinding<IReadOnlyCollection<VehicleController>> CurrentLevelCruisers
    {
        get
        {
            APIHelpers.AssertImperiumReady();

            return ImpImmutableBinding<IReadOnlyCollection<VehicleController>>.Wrap(
                Imperium.ObjectManager.CurrentLevelVehicles
            );
        }
    }

    #endregion
}
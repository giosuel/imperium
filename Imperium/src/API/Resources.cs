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
    /// <summary>
    ///     List of all the entities in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<EnemyAI>> CurrentLevelEntities
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelEntities;
        }
    }

    /// <summary>
    ///     List of all the items and scrap in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<GrabbableObject>> CurrentLevelItems
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelItems;
        }
    }

    /// <summary>
    ///     List of all the players in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<PlayerControllerB>> CurrentPlayers
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentPlayers;
        }
    }

    /// <summary>
    ///     List of all the doors in the current scene.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<DoorLock>> CurrentLevelDoors
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelDoors;
        }
    }

    /// <summary>
    ///     List of all the security doors in the current scene.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<TerminalAccessibleObject>> CurrentLevelSecurityDoors
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelSecurityDoors;
        }
    }

    /// <summary>
    ///     List of all the turrets in the current scene.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<Turret>> CurrentLevelTurrets
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelTurrets;
        }
    }

    /// <summary>
    ///     List of all the landmines in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<Landmine>> CurrentLevelLandmines
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelLandmines;
        }
    }

    /// <summary>
    ///     List of all the spike traps in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<SpikeRoofTrap>> CurrentLevelSpikeTraps
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelSpikeTraps;
        }
    }

    /// <summary>
    ///     List of all the breaker boxes in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<BreakerBox>> CurrentLevelBreakerBoxes
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelBreakerBoxes;
        }
    }

    /// <summary>
    ///     List of all the steam valves in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<SteamValveHazard>> CurrentLevelSteamValves
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelSteamValves;
        }
    }

    /// <summary>
    ///     List of all the vents in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<EnemyVent>> CurrentLevelVents
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelVents;
        }
    }

    /// <summary>
    ///     List of all the spider webs in the current level.
    /// </summary>
    /// <exception cref="ImperiumAPIException">Thrown when Imperium is not yet ready to handle calls.</exception>
    public static ImpBinding<HashSet<SandSpiderWebTrap>> CurrentLevelSpiderWebs
    {
        get
        {
            if (!Imperium.IsImperiumLaunched) throw new ImperiumAPIException("Imperium API is not ready.");
            return Imperium.ObjectManager.CurrentLevelSpiderWebs;
        }
    }
}
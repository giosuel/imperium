#region

using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Imperium.Netcode;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Core;

internal class ObjectManager : ImpLifecycleObject
{
    internal ObjectManager(ImpBinaryBinding sceneLoaded, ImpBinding<int> playersConnected)
        : base(sceneLoaded, playersConnected)
    {
        FetchGlobalSpawnLists();
        FetchPlayers();

        RefreshLevelItems();
        RefreshLevelObstacles();

        LogObjects();
    }

    protected override void OnSceneLoad()
    {
        RefreshLevelItems();
        RefreshLevelObstacles();

        LogObjects();
    }

    protected override void OnPlayersUpdate(int playersConnected) => FetchPlayers();

    // Caches other game objects so they don't have to be searched for every time with
    // the expensive resource lookup. This is per map and gets reset on scene reload.
    private readonly Dictionary<string, GameObject> ObjectCache = new();

    // Holds all the entities that can be spawned in Lethal Company, including the ones that are not in any
    // spawn list of any moon (e.g. Red Pill, Lasso Man)
    // Loaded on Imperium initialization.
    internal readonly ImpBinding<Dictionary<string, EnemyType>> AllEntities = new([]);
    internal readonly ImpBinding<Dictionary<string, EnemyType>> AllIndoorEntities = new([]);
    internal readonly ImpBinding<Dictionary<string, EnemyType>> AllOutdoorEntities = new([]);
    internal readonly ImpBinding<Dictionary<string, EnemyType>> AllDaytimeEntities = new([]);

    internal readonly ImpBinding<Dictionary<string, Item>> AllItems = new([]);
    internal readonly ImpBinding<Dictionary<string, Item>> AllScrap = new([]);
    internal readonly ImpBinding<Dictionary<string, GameObject>> AllMapHazards = new([]);
    private readonly ImpBinding<Dictionary<string, GameObject>> AllStaticPrefabs = new([]);

    // These lists hold the currently existing objects on the map
    // These are used by the object list in Imperium UI and is always up to date but
    // CAN CONTAIN NULL elements that have been marked for but not yet deleted
    // during the last refresh.
    // Loaded on ship landing.
    internal readonly ImpBinding<HashSet<DoorLock>> CurrentLevelDoors = new([]);
    internal readonly ImpBinding<HashSet<PowerSwitchable>> CurrentLevelSecurityDoors = new([]);
    internal readonly ImpBinding<HashSet<Turret>> CurrentLevelTurrets = new([]);
    internal readonly ImpBinding<HashSet<Landmine>> CurrentLevelLandmines = new([]);
    internal readonly ImpBinding<HashSet<SpikeRoofTrap>> CurrentLevelSpikeTraps = new([]);
    internal readonly ImpBinding<HashSet<BreakerBox>> CurrentLevelBreakerBoxes = new([]);
    internal readonly ImpBinding<HashSet<GameObject>> CurrentLevelSteamleaks = new([]);
    internal readonly ImpBinding<HashSet<EnemyVent>> CurrentLevelVents = new([]);
    internal readonly ImpBinding<HashSet<SandSpiderWebTrap>> CurrentLevelSpiderWebs = new([]);
    internal readonly ImpBinding<HashSet<EnemyAI>> CurrentLevelEntities = new([]);
    internal readonly ImpBinding<HashSet<GrabbableObject>> CurrentLevelItems = new([]);
    internal readonly ImpBinding<HashSet<PlayerControllerB>> CurrentPlayers = new([]);

    // Used by the server to execute a despawn request from a client via network ID
    private readonly Dictionary<ulong, GameObject> CurrentLevelObjects = [];

    internal static void SpawnEntity(
        string entityName,
        Vector3? position = null,
        int amount = 1,
        int health = -1
    )
    {
        var playerTransform = Imperium.Player.transform;
        ImpNetSpawning.Instance.SpawnEntityServerRpc(
            entityName,
            new ImpVector(position ?? playerTransform.position + playerTransform.forward * 3f),
            amount,
            health
        );
    }

    internal static void SpawnItem(
        string itemName,
        int spawningPlayerId,
        Vector3? position = null,
        int amount = 1,
        int value = -1
    )
    {
        var playerCamera = Imperium.Player.gameplayCamera.transform;
        var playerCameraForward = playerCamera.forward;
        var defaultSpawnPosition = playerCamera.position + 3 * new Vector3(
            playerCameraForward.x,
            0,
            playerCameraForward.z
        );
        ImpNetSpawning.Instance.SpawnItemServerRpc(
            itemName,
            spawningPlayerId,
            new ImpVector(position ?? defaultSpawnPosition),
            amount,
            value
        );
    }

    internal static void SpawnMapHazard(
        string objectName,
        Vector3? position = null,
        int amount = 1
    )
    {
        var playerTransform = Imperium.Player.transform;

        ImpNetSpawning.Instance.SpawnMapHazardServerRpc(
            objectName,
            new ImpVector(position ?? playerTransform.position + playerTransform.forward * 3f),
            amount
        );
    }

    [ImpAttributes.HostOnly]
    internal void SpawnEntityServer(
        string entityName,
        Vector3 position,
        int amount,
        int health
    )
    {
        if (!AllEntities.Value.TryGetValue(entityName, out var entityType))
        {
            Imperium.Output.Error($"Entity {entityName} not found!");
            return;
        }

        for (var i = 0; i < amount; i++)
        {
            var entityObj = Object.Instantiate(
                entityType.enemyPrefab,
                position,
                Quaternion.Euler(Vector3.zero)
            );
            if (health > 0) entityObj.GetComponent<EnemyAI>().enemyHP = health;

            var netObject = entityObj.gameObject.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);
            CurrentLevelObjects[netObject.NetworkObjectId] = entityObj;
        }

        var mountString = amount == 1 ? "A" : $"{amount.ToString()}x";
        var verbString = amount == 1 ? "has" : "have";

        Imperium.Output.SendToClients(
            $"{mountString} loyal {GetDisplayName(entityType.enemyName)} {verbString} been spawned!"
        );

        ImpNetSpawning.Instance.OnEntitiesChangedClientRpc();
    }

    [ImpAttributes.HostOnly]
    internal void SpawnItemServer(
        string itemName,
        int spawningPlayerId,
        Vector3 position,
        int amount,
        int value
    )
    {
        if (!AllItems.Value.TryGetValue(itemName, out var itemType))
        {
            Imperium.Output.Error($"Item {itemName} not found!");
            return;
        }

        var prefab = itemType.spawnPrefab != null && itemType.spawnPrefab.GetComponent<GrabbableObject>() != null
            ? itemType.spawnPrefab
            : AllStaticPrefabs.Value[itemType.itemName];

        for (var i = 0; i < amount; i++)
        {
            var itemObj = Object.Instantiate(
                prefab,
                position,
                Quaternion.identity,
                Imperium.RoundManager.spawnedScrapContainer
            );

            var grabbableItem = itemObj.GetComponent<GrabbableObject>();
            grabbableItem.transform.rotation = Quaternion.Euler(itemType.restingRotation);

            if (value == -1) value = ImpUtils.RandomItemValue(itemType);
            grabbableItem.scrapValue = value;

            // Execute start immediately to initialize random generator for animated objects
            grabbableItem.Start();

            var netObject = itemObj.gameObject.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);
            CurrentLevelObjects[netObject.NetworkObjectId] = itemObj;

            // If player has free slot, place it in hand, otherwise leave it on the ground and play sound
            var invokingPlayer = Imperium.StartOfRound.allPlayerScripts[spawningPlayerId];
            var firstItemSlot = Reflection.Invoke<PlayerControllerB, int>(invokingPlayer, "FirstEmptyItemSlot");
            if (firstItemSlot != -1 && grabbableItem.grabbable)
            {
                grabbableItem.InteractItem();
                PlayerManager.GrabObject(grabbableItem, invokingPlayer);
            }
            else if (grabbableItem.itemProperties.dropSFX)
            {
                Imperium.Player.itemAudio.PlayOneShot(grabbableItem.itemProperties.dropSFX);
            }
        }

        var mountString = amount == 1 ? "A" : $"{amount.ToString()}x";
        var verbString = amount == 1 ? "has" : "have";

        Imperium.Output.SendToClients($"{mountString} {itemName} {verbString} been spawned!");

        ImpNetSpawning.Instance.OnItemsChangedClientRpc();
    }

    private readonly Dictionary<string, string> displayNameMap = [];

    internal string GetDisplayName(string inGameName) => displayNameMap.GetValueOrDefault(inGameName, inGameName);

    [ImpAttributes.HostOnly]
    internal void SpawnMapHazardServer(string objectName, Vector3 position, int amount)
    {
        for (var i = 0; i < amount; i++)
        {
            switch (objectName)
            {
                case "Turret":
                    SpawnTurret(position);
                    break;
                case "Spike Trap":
                    SpawnSpikeTrap(position);
                    break;
                case "Landmine":
                    SpawnLandmine(position);
                    break;
                case "SpiderWeb":
                    Imperium.Output.Error("Spider web spawning not implemented yet");
                    break;
                default:
                    Imperium.Output.Error($"Failed to spawn map hazard {objectName}");
                    return;
            }
        }

        var mountString = amount == 1 ? "A" : $"{amount.ToString()}x";
        var verbString = amount == 1 ? "has" : "have";

        Imperium.Output.SendToClients($"{mountString} {objectName} {verbString} been spawned!");

        ImpNetSpawning.Instance.OnMapHazardsChangedClientRpc();
    }

    [ImpAttributes.HostOnly]
    private void SpawnLandmine(Vector3 position)
    {
        var hazardObj = Object.Instantiate(AllMapHazards.Value["Landmine"], position, Quaternion.Euler(Vector3.zero));
        hazardObj.transform.Find("Landmine").rotation = Quaternion.Euler(270, 0, 0);
        hazardObj.transform.localScale = new Vector3(0.4574f, 0.4574f, 0.4574f);

        var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        netObject.Spawn(destroyWithScene: true);
        CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void SpawnTurret(Vector3 position)
    {
        var hazardObj = Object.Instantiate(AllMapHazards.Value["Turret"], position, Quaternion.Euler(Vector3.zero));

        var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        netObject.Spawn(destroyWithScene: true);
        CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void SpawnSpikeTrap(Vector3 position)
    {
        var hazardObj = Object.Instantiate(
            AllMapHazards.Value["Spike Trap"],
            position,
            Quaternion.Euler(Vector3.zero)
        );

        var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        netObject.Spawn(destroyWithScene: true);
        CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void DespawnObject(GameObject gameObject)
    {
        if (gameObject.TryGetComponent<GrabbableObject>(out var grabbableObject))
        {
            if (grabbableObject.isHeld && grabbableObject.playerHeldBy is not null)
            {
                ImpNetPlayer.Instance.DiscardHotbarItemServerRpc(
                    PlayerManager.GetPlayerID(grabbableObject.playerHeldBy),
                    PlayerManager.GetItemHolderSlot(grabbableObject)
                );
            }
        }

        if (gameObject.TryGetComponent<NetworkObject>(out var networkObject))
        {
            try
            {
                networkObject.Despawn();
            }
            catch (NullReferenceException)
            {
            }
        }

        Object.Destroy(gameObject);
    }

    [ImpAttributes.HostOnly]
    internal bool DespawnObject(ulong netId)
    {
        if (!CurrentLevelObjects.TryGetValue(netId, out var obj))
        {
            Imperium.Output.Error($"Failed to despawn object with net ID {netId}");
            return false;
        }

        DespawnObject(obj);
        return true;
    }

    [ImpAttributes.LocalMethod]
    internal void EmptyVent(ulong netId)
    {
        if (!CurrentLevelObjects.TryGetValue(netId, out var obj) ||
            !obj.TryGetComponent<EnemyVent>(out var enemyVent))
        {
            Imperium.Output.Error($"Failed to empty vent with net ID {netId}");
            return;
        }

        enemyVent.occupied = false;
    }

    [ImpAttributes.LocalMethod]
    internal GameObject FindObject(string name)
    {
        if (ObjectCache.TryGetValue(name, out var v)) return v;
        var obj = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(
            obj => obj.name == name && obj.scene != SceneManager.GetSceneByName("HideAndDontSave"));
        if (!obj) return null;
        ObjectCache[name] = obj;
        return obj;
    }

    internal void ToggleObject(string name, bool isOn) => FindObject(name)?.SetActive(isOn);

    /// <summary>
    /// Fetches all game objects from resources to be used later for spawning
    ///
    /// - Entities (Indoor, Outdoor, Daytime)
    /// - Scrap and Items
    /// - Map Hazards
    /// - Other Static Prefabs (e.g. clipboard, player body)
    /// </summary>
    private void FetchGlobalSpawnLists()
    {
        var allEntities = new Dictionary<string, EnemyType>();
        var allIndoorEntities = new Dictionary<string, EnemyType>();
        var allOutdoorEntities = new Dictionary<string, EnemyType>();
        var allDaytimeEntities = new Dictionary<string, EnemyType>();

        foreach (var enemyType in Resources.FindObjectsOfTypeAll<EnemyType>().Distinct())
        {
            allEntities[enemyType.enemyName] = enemyType;

            if (enemyType.isDaytimeEnemy)
            {
                allDaytimeEntities[enemyType.enemyName] = enemyType;
            }
            else if (enemyType.isOutsideEnemy)
            {
                allOutdoorEntities[enemyType.enemyName] = enemyType;
            }
            else
            {
                allIndoorEntities[enemyType.enemyName] = enemyType;
            }
        }

        var allItems = Resources.FindObjectsOfTypeAll<Item>()
            .Where(item => !ImpConstants.ItemBlacklist.Contains(item.itemName))
            .ToDictionary(scrap => scrap.itemName);

        var allMapHazards = new Dictionary<string, GameObject>();
        var allStaticPrefabs = new Dictionary<string, GameObject>();
        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            switch (obj.name)
            {
                case "SpikeRoofTrapHazard":
                    allMapHazards["Spike Trap"] = obj;
                    break;
                case "TurretContainer":
                    allMapHazards["Turret"] = obj;
                    break;
                // Find all landmine containers (Not the actual mine objects which happen to have the same name)
                case "Landmine" when obj.transform.Find("Landmine") != null:
                    allMapHazards["Landmine"] = obj;
                    break;
                case "ClipboardManual":
                    allStaticPrefabs["clipboard"] = obj;
                    break;
                case "StickyNoteItem":
                    allStaticPrefabs["Sticky note"] = obj;
                    break;
            }
        }

        allStaticPrefabs["Body"] = Imperium.StartOfRound.ragdollGrabbableObjectPrefab;

        var allScrap = allItems
            .Where(scrap => scrap.Value.isScrap)
            .ToDictionary(entry => entry.Key, entry => entry.Value);

        AllEntities.Set(allEntities);
        AllIndoorEntities.Set(allIndoorEntities);
        AllOutdoorEntities.Set(allOutdoorEntities);
        AllDaytimeEntities.Set(allDaytimeEntities);

        AllItems.Set(allItems);
        AllScrap.Set(allScrap);
        AllMapHazards.Set(allMapHazards);
        AllStaticPrefabs.Set(allStaticPrefabs);

        GenerateDisplayNameMap();
    }

    internal void RefreshLevelItems()
    {
        HashSet<GrabbableObject> currentLevelItems = [];
        foreach (var obj in Resources.FindObjectsOfTypeAll<GrabbableObject>())
        {
            // Ignore objects that are hidden
            if (obj.gameObject.scene == SceneManager.GetSceneByName("HideAndDontSave")) continue;

            currentLevelItems.Add(obj);
            CurrentLevelObjects[obj.GetComponent<NetworkObject>().NetworkObjectId] = obj.gameObject;
        }

        CurrentLevelItems.Set(currentLevelItems);
    }

    internal void RefreshLevelEntities()
    {
        HashSet<EnemyAI> currentLevelEntities = [];
        foreach (var obj in Resources.FindObjectsOfTypeAll<EnemyAI>())
        {
            // Ignore objects that are hidden
            if (obj.gameObject.scene == SceneManager.GetSceneByName("HideAndDontSave")) continue;

            currentLevelEntities.Add(obj);
            CurrentLevelObjects[obj.GetComponent<NetworkObject>().NetworkObjectId] = obj.gameObject;
        }

        CurrentLevelEntities.Set(currentLevelEntities);
    }

    internal void RefreshLevelObstacles()
    {
        HashSet<DoorLock> currentLevelDoors = [];
        HashSet<PowerSwitchable> currentLevelSecurityDoors = [];
        HashSet<Turret> currentLevelTurrets = [];
        HashSet<Landmine> currentLevelLandmines = [];
        HashSet<SpikeRoofTrap> currentLevelSpikeTraps = [];
        HashSet<BreakerBox> currentLevelBreakerBoxes = [];
        HashSet<GameObject> currentLevelSteamleaks = [];
        HashSet<EnemyVent> currentLevelVents = [];
        HashSet<SandSpiderWebTrap> currentLevelSpiderWebs = [];

        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            // Ignore objects that are hidden
            if (obj.scene == SceneManager.GetSceneByName("HideAndDontSave")) continue;

            if (obj.name == "FogZone")
            {
                currentLevelSteamleaks.Add(obj);
                continue;
            }

            foreach (var component in obj.GetComponents<Component>())
            {
                switch (component)
                {
                    case DoorLock doorLock when !currentLevelDoors.Contains(doorLock):
                        currentLevelDoors.Add(doorLock);
                        break;
                    case PowerSwitchable powerSwitch when !currentLevelSecurityDoors.Contains(powerSwitch):
                        currentLevelSecurityDoors.Add(powerSwitch);
                        break;
                    case Turret turret when !currentLevelTurrets.Contains(turret):
                        currentLevelTurrets.Add(turret);
                        break;
                    case Landmine landmine when !currentLevelLandmines.Contains(landmine):
                        currentLevelLandmines.Add(landmine);
                        break;
                    case SpikeRoofTrap spikeTrap when !currentLevelSpikeTraps.Contains(spikeTrap):
                        currentLevelSpikeTraps.Add(spikeTrap);
                        break;
                    case BreakerBox breakerBox when !currentLevelBreakerBoxes.Contains(breakerBox):
                        currentLevelBreakerBoxes.Add(breakerBox);
                        break;
                    case EnemyVent enemyVent when !currentLevelVents.Contains(enemyVent):
                        currentLevelVents.Add(enemyVent);
                        break;
                    case SandSpiderWebTrap spiderWeb when !currentLevelSpiderWebs.Contains(spiderWeb):
                        currentLevelSpiderWebs.Add(spiderWeb);
                        break;
                }
            }

            var networkObject = obj.GetComponent<NetworkObject>();
            if (!networkObject)
            {
                networkObject = obj.GetComponentInChildren<NetworkObject>();
                if (!networkObject)
                {
                    continue;
                }
            }

            CurrentLevelObjects[networkObject.NetworkObjectId] = obj.gameObject;
        }

        if (currentLevelDoors.Count > 0)
        {
            CurrentLevelDoors.Set(currentLevelDoors.Union(currentLevelDoors).ToHashSet());
        }

        if (currentLevelSecurityDoors.Count > 0)
        {
            CurrentLevelSecurityDoors.Set(CurrentLevelSecurityDoors.Value.Union(currentLevelSecurityDoors).ToHashSet());
        }

        if (currentLevelTurrets.Count > 0)
        {
            CurrentLevelTurrets.Set(CurrentLevelTurrets.Value.Union(currentLevelTurrets).ToHashSet());
        }

        if (currentLevelLandmines.Count > 0)
        {
            CurrentLevelLandmines.Set(CurrentLevelLandmines.Value.Union(currentLevelLandmines).ToHashSet());
        }

        if (currentLevelSpikeTraps.Count > 0)
        {
            CurrentLevelSpikeTraps.Set(CurrentLevelSpikeTraps.Value.Union(currentLevelSpikeTraps).ToHashSet());
        }

        if (currentLevelBreakerBoxes.Count > 0)
        {
            CurrentLevelBreakerBoxes.Set(CurrentLevelBreakerBoxes.Value.Union(currentLevelBreakerBoxes).ToHashSet());
        }

        if (currentLevelSteamleaks.Count > 0)
        {
            CurrentLevelSteamleaks.Set(CurrentLevelSteamleaks.Value.Union(currentLevelSteamleaks).ToHashSet());
        }

        if (currentLevelVents.Count > 0)
        {
            CurrentLevelVents.Set(CurrentLevelVents.Value.Union(currentLevelVents).ToHashSet());
        }

        if (currentLevelSpiderWebs.Count > 0)
        {
            CurrentLevelSpiderWebs.Set(CurrentLevelSpiderWebs.Value.Union(currentLevelSpiderWebs).ToHashSet());
        }
    }

    private void GenerateDisplayNameMap()
    {
        foreach (var (entityName, entity) in AllEntities.Value.Where(entity => entity.Value))
        {
            if (!entity || entity.enemyPrefab) return;
            var displayName = entity.enemyPrefab.GetComponentInChildren<ScanNodeProperties>()?.headerText;
            if (!string.IsNullOrEmpty(displayName)) displayNameMap[entityName] = displayName;
        }

        foreach (var (entityName, entity) in AllScrap.Value.Where(scrap => scrap.Value))
        {
            if (!entity || entity.spawnPrefab) return;
            var displayName = entity.spawnPrefab.GetComponentInChildren<ScanNodeProperties>()?.headerText;
            if (!string.IsNullOrEmpty(displayName)) displayNameMap[entityName] = displayName;
        }

        foreach (var (entityName, entity) in AllItems.Value.Where(item => item.Value))
        {
            if (!entity || entity.spawnPrefab) return;
            var displayName = entity.spawnPrefab.GetComponentInChildren<ScanNodeProperties>()?.headerText;
            if (!string.IsNullOrEmpty(displayName)) displayNameMap[entityName] = displayName;
        }
    }

    private void FetchPlayers()
    {
        CurrentPlayers.Set(
            Resources.FindObjectsOfTypeAll<PlayerControllerB>()
                .Where(obj => obj.gameObject.scene != SceneManager.GetSceneByName("HideAndDontSave"))
                .ToHashSet()
        );
    }

    private void LogObjects()
    {
        Imperium.Output.LogBlock([
            "Imperium scanned the current level for obstacles.",
            $"   > {CurrentLevelDoors.Value.Count}x Doors",
            $"   > {CurrentLevelSecurityDoors.Value.Count}x Security doors",
            $"   > {CurrentLevelTurrets.Value.Count}x Turrets",
            $"   > {CurrentLevelLandmines.Value.Count}x Landmines",
            $"   > {CurrentLevelBreakerBoxes.Value.Count}x Breaker boxes",
            $"   > {CurrentLevelSpiderWebs.Value.Count}x Spider webs"
        ]);
    }
}
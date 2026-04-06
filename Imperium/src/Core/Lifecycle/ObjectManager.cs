#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameNetcodeStuff;
using Imperium.API.Types.Networking;
using Imperium.Core.Scripts;
using Imperium.Netcode;
using Imperium.Util;
using Imperium.Util.Binding;
using LethalNetworkAPI;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

#endregion

namespace Imperium.Core.Lifecycle;

/// <summary>
/// Lifecycle object that manages all object-related functionality. Keeps track of loaded and currently active objects.
/// </summary>
internal class ObjectManager : ImpLifecycleObject
{
    /*
     * Lists of globally loaded objects.
     *
     * These lists hold all the entities that can be spawned in Lethal Company, including the ones that are not in any
     * spawn list of any moon (e.g. Red Pill, Lasso Man).
     *
     * Loaded when Imperium initializes (Stage 1).
     */
    internal readonly ImpBinding<IReadOnlyCollection<Item>> LoadedItems = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<Item>> LoadedScrap = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<EnemyType>> LoadedEntities = new([]);
    internal readonly ImpBinding<IReadOnlyDictionary<string, BuyableVehicle>> LoadedVehicles = new();
    internal readonly ImpBinding<IReadOnlyDictionary<string, IndoorMapHazardType>> LoadedMapHazards = new();

    // Lists of bjects with network behaviours (e.g. clipboard, body, company cruiser)
    internal readonly ImpBinding<IReadOnlyDictionary<string, NetworkObject>> LoadedStaticPrefabs = new();
    internal readonly ImpBinding<IReadOnlyDictionary<string, SpawnableOutsideObject>> LoadedOutsideObjects = new();

    // Lists of bjects without network behaviours (e.g. trees, vain shrouds, rocks)
    internal readonly ImpBinding<IReadOnlyDictionary<string, GameObject>> LoadedLocalStaticPrefabs = new();

    /*
     * Lists of objects loaded in the current scene.
     *
     * These lists hold the currently existing objects in the scene.
     * These are used by the object list in Imperium UI and is always up-to-date but
     * CAN CONTAIN NULL elements that have been marked for but not yet deleted during the last refresh.
     * Always ensure to check for null values before using the values in these lists.
     *
     * Loaded when Imperium launches (Stage 2).
     */
    internal readonly ImpBinding<IReadOnlyCollection<Turret>> CurrentLevelTurrets = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<DoorLock>> CurrentLevelDoors = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<EnemyVent>> CurrentLevelVents = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<EnemyAI>> CurrentLevelEntities = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<Landmine>> CurrentLevelLandmines = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<StoryLog>> CurrentLevelStoryLogs = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<PlayerControllerB>> CurrentPlayers = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<GrabbableObject>> CurrentLevelItems = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<BreakerBox>> CurrentLevelBreakerBoxes = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SpikeRoofTrap>> CurrentLevelSpikeTraps = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<VehicleController>> CurrentLevelVehicles = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SteamValveHazard>> CurrentLevelSteamValves = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SandSpiderWebTrap>> CurrentLevelSpiderWebs = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<TerminalAccessibleObject>> CurrentLevelSecurityDoors = new([]);

    /*
     * Lists of local objects that don't have a network object or script to reference
     */
    internal readonly ImpBinding<IReadOnlyCollection<GameObject>> CurrentLevelOutsideObjects = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<GameObject>> CurrentLevelVainShrouds = new([]);

    /*
     * Event that signalizes a change in any of the object lists
     */
    internal event Action CurrentLevelObjectsChanged;

    /*
     * Misc scene objects.
     */
    internal readonly ImpBinding<IReadOnlyCollection<RandomScrapSpawn>> CurrentScrapSpawnPoints = new([]);

    /*
     * Cache of game objects indexed by name for visualizers and other object access.
     *
     * Cleared when the ship is landing / taking off.
     */
    private readonly Dictionary<string, GameObject> ObjectCache = new();

    /*
     * List of Network IDs of disabled objects. Used to sync object active status over the network.
     */
    internal readonly ImpNetworkBinding<HashSet<ulong>> DisabledObjects = new(
        "DisabledObjects", Imperium.Networking, []
    );

    /*
     * Used by the server to execute a despawn request from a client via network ID
     */
    private readonly Dictionary<ulong, NetworkObject> CurrentLevelObjects = [];

    private readonly Dictionary<string, string> displayNameMap = [];
    private readonly Dictionary<string, string> overrideDisplayNameMap = [];

    private readonly ImpNetMessage<EntitySpawnRequest> entitySpawnMessage = new(
        "SpawnEntity", Imperium.Networking
    );

    private readonly ImpNetMessage<ItemSpawnRequest> itemSpawnMessage = new(
        "SpawnItem", Imperium.Networking
    );

    private readonly ImpNetMessage<VehicleSpawnRequest> vehicleSpawnMessage = new(
        "SpawnVehicle", Imperium.Networking
    );

    private readonly ImpNetMessage<VehicleSpawnResponse> vehicleSpawnResponseMessage = new(
        "SpawnVehicleResponse", Imperium.Networking
    );

    private readonly ImpNetMessage<MapHazardSpawnRequest> mapHazardSpawnMessage = new(
        "MapHazardSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<StaticPrefabSpawnRequest> staticPrefabSpawnMessage = new(
        "StaticPrefabSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<StaticPrefabSpawnRequest> localStaticPrefabSpawnMessage = new(
        "LocalStaticPrefabSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<StaticPrefabSpawnRequest> outsideObjectPrefabSpawnMessage = new(
        "OutsideObjectSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<ObjectTeleportRequest> objectTeleportationRequest = new(
        "ObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<LocalObjectTeleportRequest> localObjectTeleportationMessage = new(
        "LocalObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<BurstCadaverBloomRequest> burstCadaverBloomMessage = new(
        "BurstCadaverBloom", Imperium.Networking
    );

    private readonly ImpNetMessage<ulong> burstSteamValve = new(
        "BurstSteamValve", Imperium.Networking
    );

    private readonly ImpNetMessage<EntityDespawnRequest> entityDespawnMessage = new(
        "DespawnEntity", Imperium.Networking
    );

    private readonly ImpNetMessage<VehicleDespawnRequest> vehicleDespawnMessage = new(
        "DespawnVehicle", Imperium.Networking
    );

    private readonly ImpNetMessage<ulong> itemDespawnMessage = new(
        "DespawnItem", Imperium.Networking
    );

    private readonly ImpNetMessage<ulong> obstacleDespawnMessage = new(
        "DespawnObstacle", Imperium.Networking
    );

    private readonly ImpNetMessage<LocalObjectDespawnRequest> localObjectDespawnMessage = new(
        "DespawnLocalObject", Imperium.Networking
    );

    private readonly ImpNetEvent objectsChangedEvent = new(
        "ObjectsChanged", Imperium.Networking
    );

    // List of prefab names of outside objects. Used to identify outside objects.
    private readonly HashSet<string> OutsideObjectPrefabNameMap =
    [
        "GiantPumpkin(Clone)",
        "LargeRock1(Clone)",
        "LargeRock2(Clone)",
        "LargeRock3(Clone)",
        "LargeRock4(Clone)",
        "GreyRockGrouping2(Clone)",
        "GreyRockGrouping4(Clone)",
        "tree(Clone)",
        "treeLeaflessBrown.001 Variant(Clone)",
        "treeLeafless.002_LOD0(Clone)",
        "treeLeafless.003_LOD0(Clone)"
    ];

    /*
     * Collections for the entity name system.
     */
    private readonly List<string> AvailableEntityNames = ImpAssets.EntityNames.Select(entityName => entityName).ToList();
    private readonly Dictionary<int, string> EntityNameMap = [];
    private bool JohnExists;

    /*
     * Assets loaded from the game's resources after loading objects
     */
    internal AudioClip BeaconDrop;

    private LayerMask terrainMask;

    protected override void Init()
    {
        FetchGlobalSpawnLists();
        FetchPlayers();

        RefreshLevelObjects();

        LogObjects();

        objectsChangedEvent.OnClientRecive += RefreshLevelObjects;
        burstSteamValve.OnClientRecive += OnSteamValveBurst;
        burstCadaverBloomMessage.OnClientRecive += OnCadaverBloomMessageBurst;
        vehicleSpawnResponseMessage.OnClientRecive += OnSpawnVehicleClient;
        objectTeleportationRequest.OnClientRecive += OnObjectTeleportationRequestClient;

        localObjectDespawnMessage.OnClientRecive += OnDespawnLocalObject;
        localStaticPrefabSpawnMessage.OnClientRecive += OnSpawnLocalStaticPrefabClient;
        outsideObjectPrefabSpawnMessage.OnClientRecive += OnSpawnOutsideObjectClient;
        localObjectTeleportationMessage.OnClientRecive += OnLocalObjectTeleportationMessageClient;

        if (NetworkManager.Singleton.IsHost)
        {
            entitySpawnMessage.OnServerReceive += OnSpawnEntity;
            itemSpawnMessage.OnServerReceive += OnSpawnItem;
            vehicleSpawnMessage.OnServerReceive += OnSpawnVehicle;
            mapHazardSpawnMessage.OnServerReceive += OnSpawnMapHazard;
            staticPrefabSpawnMessage.OnServerReceive += OnSpawnStaticPrefabServer;

            entityDespawnMessage.OnServerReceive += OnDespawnEntity;
            vehicleDespawnMessage.OnServerReceive += OnDespawnVehicle;
            itemDespawnMessage.OnServerReceive += OnDespawnItem;
            obstacleDespawnMessage.OnServerReceive += OnDespawnObstacle;

            objectTeleportationRequest.OnServerReceive += OnObjectTeleportationRequestServer;
        }

        terrainMask = LayerMask.NameToLayer("Terrain");
    }

    protected override void OnSceneLoad()
    {
        RefreshLevelObjects();

        LogObjects();

        // Reload objects that are hidden on the moon but visible in space
        Imperium.Settings.Rendering.SpaceSun.Refresh();
        Imperium.Settings.Rendering.StarsOverlay.Refresh();
    }

    protected override void OnPlayersUpdate(int playersConnected) => FetchPlayers();

    [ImpAttributes.RemoteMethod]
    internal void SpawnEntity(EntitySpawnRequest request) => entitySpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnItem(ItemSpawnRequest request) => itemSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnVehicle(VehicleSpawnRequest request) => vehicleSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnMapHazard(MapHazardSpawnRequest request) => mapHazardSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnStaticPrefab(StaticPrefabSpawnRequest request)
    {
        if (!LoadedStaticPrefabs.Value.ContainsKey(request.Name))
        {
            Imperium.IO.LogError($"[SPAWN] Unable to find requested static prefab '{request.Name}'.");
            return;
        }

        staticPrefabSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnLocalStaticPrefab(StaticPrefabSpawnRequest request)
    {
        if (!LoadedLocalStaticPrefabs.Value.ContainsKey(request.Name))
        {
            Imperium.IO.LogError($"[SPAWN] Unable to find requested local static prefab '{request.Name}'.");
            return;
        }

        localStaticPrefabSpawnMessage.DispatchToClients(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnOutsideObject(StaticPrefabSpawnRequest request)
    {
        if (!LoadedOutsideObjects.Value.ContainsKey(request.Name))
        {
            Imperium.IO.LogError($"[SPAWN] Unable to find requested outside object '{request.Name}'.");
            return;
        }

        outsideObjectPrefabSpawnMessage.DispatchToClients(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnItem(ulong itemNetId) => itemDespawnMessage.DispatchToServer(itemNetId);

    [ImpAttributes.RemoteMethod]
    internal void DespawnEntity(EntityDespawnRequest request) => entityDespawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void DespawnVehicle(VehicleDespawnRequest request) => vehicleDespawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void DespawnObstacle(ulong obstacleNetId) => obstacleDespawnMessage.DispatchToServer(obstacleNetId);

    [ImpAttributes.RemoteMethod]
    internal void DespawnLocalObject(LocalObjectDespawnRequest request)
    {
        localObjectDespawnMessage.DispatchToClients(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void TeleportObject(ObjectTeleportRequest request) => objectTeleportationRequest.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void TeleportLocalObject(LocalObjectTeleportRequest request)
    {
        localObjectTeleportationMessage.DispatchToClients(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void InvokeObjectsChanged() => objectsChangedEvent.DispatchToClients();

    [ImpAttributes.RemoteMethod]
    internal void BurstSteamValve(ulong valveNetId) => burstSteamValve.DispatchToClients(valveNetId);

    internal string GetDisplayName(string inGameName) => displayNameMap.GetValueOrDefault(inGameName, inGameName);
    internal string GetOverrideDisplayName(string inGameName) => overrideDisplayNameMap.GetValueOrDefault(inGameName);

    [ImpAttributes.LocalMethod]
    internal void EmptyVent(ulong netId)
    {
        if (!CurrentLevelObjects.TryGetValue(netId, out var obj) ||
            !obj.TryGetComponent<EnemyVent>(out var enemyVent))
        {
            Imperium.IO.LogError($"Failed to empty vent with net ID {netId}");
            return;
        }

        enemyVent.occupied = false;
    }

    internal GameObject FindObject(string objName)
    {
        if (ObjectCache.TryGetValue(objName, out var v)) return v;
        var obj = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj =>
            obj.name == objName && obj.scene != SceneManager.GetSceneByName("HideAndDontSave"));
        if (!obj) return null;
        ObjectCache[objName] = obj;
        return obj;
    }

    internal void ToggleObject(string objName, bool isOn)
    {
        var obj = FindObject(objName);
        if (obj) obj.SetActive(isOn);
    }

    /// <summary>
    ///     Fetches all game objects from resources to be used later for spawning
    ///     - Entities (Indoor, Outdoor, Daytime)
    ///     - Scrap and Items
    ///     - Map Hazards
    ///     - Other Static Prefabs (e.g. clipboard, player body)
    /// </summary>
    private void FetchGlobalSpawnLists()
    {
        var allEntities = new HashSet<EnemyType>();

        EnemyType redPillType = null;
        var shiggyExists = false;

        foreach (var enemyType in Resources.FindObjectsOfTypeAll<EnemyType>().Distinct())
        {
            if (!enemyType.enemyPrefab) continue;
            allEntities.Add(enemyType);

            switch (enemyType.enemyName)
            {
                case "Red pill":
                    redPillType = enemyType;
                    break;
                case "Shiggy":
                    shiggyExists = true;
                    break;
            }
        }

        // Instantiate shiggy type if not already exists and if redpill has been found
        if (redPillType && !shiggyExists) allEntities.Add(CreateShiggyType(redPillType));

        var allItems = Resources.FindObjectsOfTypeAll<Item>()
            .Where(item => item.spawnPrefab && !ImpConstants.ItemBlacklist.Contains(item.itemName))
            .ToHashSet();
        BeaconDrop = allItems.First(item => item.itemName == "Radar-booster").dropSFX;

        var allScrap = allItems.Where(scrap => scrap.isScrap).ToHashSet();

        var allVehicles = Imperium.Terminal.buyableVehicles
            .GroupBy(v => v.vehicleDisplayName)
            .Select(g => g.First())
            .ToDictionary(vehicle => vehicle.vehicleDisplayName, vehicle => vehicle);

        var allMapHazards = Resources.FindObjectsOfTypeAll<IndoorMapHazardType>()
            .Where(obj => obj.prefabToSpawn)
            .GroupBy(obj => obj.prefabToSpawn.name)
            .Select(obj => obj.First())
            .ToDictionary(obj => obj.prefabToSpawn.name);

        var allOutsideObjects = Resources.FindObjectsOfTypeAll<SpawnableOutsideObject>()
            .Where(obj => obj.prefabToSpawn)
            .GroupBy(obj => obj.prefabToSpawn.name)
            .Select(obj => obj.First())
            .ToDictionary(obj => obj.prefabToSpawn.name);

        var allStaticPrefabs = new Dictionary<string, NetworkObject>();
        var allLocalStaticPrefabs = new Dictionary<string, GameObject>();

        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            switch (obj.name)
            {
                case "RagdollGrabbableObject":
                    allStaticPrefabs["Body"] = obj.GetComponent<NetworkObject>();
                    break;
                case "ClipboardManual":
                    allStaticPrefabs["Clipboard"] = obj.GetComponent<NetworkObject>();
                    break;
                case "StickyNoteItem":
                    allStaticPrefabs["StickyNote"] = obj.GetComponent<NetworkObject>();
                    break;
                case "MoldSpore 1":
                    allLocalStaticPrefabs["MoldSpore"] = obj;
                    break;
            }
        }

        LoadedItems.Set(allItems);
        LoadedScrap.Set(allScrap);
        LoadedEntities.Set(allEntities);
        LoadedVehicles.Set(allVehicles);
        LoadedMapHazards.Set(allMapHazards);
        LoadedStaticPrefabs.Set(allStaticPrefabs);
        LoadedOutsideObjects.Set(allOutsideObjects);
        LoadedLocalStaticPrefabs.Set(allLocalStaticPrefabs);

        GenerateDisplayNameMaps();
    }

    private static EnemyType CreateShiggyType(EnemyType type)
    {
        var shiggyType = Instantiate(type);
        shiggyType.enemyName = "Shiggy";

        return shiggyType;
    }

    internal string GetEntityName(EnemyAI instance)
    {
        var instanceId = instance.GetInstanceID();
        if (!JohnExists && instance.enemyType.enemyName == "Bush Wolf")
        {
            JohnExists = true;
            EntityNameMap[instanceId] = "John";
            return "John";
        }

        if (!EntityNameMap.TryGetValue(instanceId, out var entityName))
        {
            if (AvailableEntityNames.Count == 0)
            {
                Imperium.IO.LogInfo("[OBJ] Somehow Imperium is out of entity names. Falling back to instance ID.");
                return instanceId.ToString();
            }

            var newNameIndex = Random.Range(0, AvailableEntityNames.Count);

            entityName = AvailableEntityNames[newNameIndex];
            EntityNameMap[instanceId] = entityName;

            AvailableEntityNames.RemoveAt(newNameIndex);
        }

        return entityName;
    }

    internal void RefreshLevelEntities()
    {
        HashSet<EnemyAI> currentLevelEntities = [];
        foreach (var obj in FindObjectsOfType<EnemyAI>())
        {
            // Ignore objects that are hidden
            if (obj.gameObject.scene == SceneManager.GetSceneByName("HideAndDontSave")) continue;

            currentLevelEntities.Add(obj);

            var entityNetObj = obj.GetComponent<NetworkObject>();
            CurrentLevelObjects[entityNetObj.NetworkObjectId] = entityNetObj;
        }

        CurrentLevelEntities.Set(currentLevelEntities);
        CurrentLevelObjectsChanged?.Invoke();
    }

    internal void RefreshLevelObjects()
    {
        var stopwatch = Stopwatch.StartNew();
        var stopwatch2 = Stopwatch.StartNew();

        HashSet<DoorLock> currentLevelDoors = [];
        HashSet<Turret> currentLevelTurrets = [];
        HashSet<EnemyVent> currentLevelVents = [];
        HashSet<EnemyAI> currentLevelEntities = [];
        HashSet<Landmine> currentLevelLandmines = [];
        HashSet<StoryLog> currentLevelStoryLogs = [];
        HashSet<GrabbableObject> currentLevelItems = [];
        HashSet<GameObject> currentLevelVainShrouds = [];
        HashSet<BreakerBox> currentLevelBreakerBoxes = [];
        HashSet<SpikeRoofTrap> currentLevelSpikeTraps = [];
        HashSet<GameObject> currentLevelOutsideObjects = [];
        HashSet<VehicleController> currentLevelVehicles = [];
        HashSet<SteamValveHazard> currentLevelSteamValves = [];
        HashSet<SandSpiderWebTrap> currentLevelSpiderWebs = [];
        HashSet<RandomScrapSpawn> currentScrapSpawnPoints = [];
        HashSet<TerminalAccessibleObject> currentLevelSecurityDoors = [];

        foreach (var obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            // This is cursed but there is no other way
            if (obj.name.Contains("MoldSpore 1") && currentLevelVainShrouds.Add(obj)) continue;

            if (obj.layer == terrainMask
                && OutsideObjectPrefabNameMap.Contains(obj.name)
                && currentLevelOutsideObjects.Add(obj)
               )
            {
                continue;
            }

            foreach (var component in obj.GetComponents<Component>())
            {
                switch (component)
                {
                    case DoorLock doorLock:
                        currentLevelDoors.Add(doorLock);
                        break;
                    case TerminalAccessibleObject { isBigDoor: true } securityDoor:
                        currentLevelSecurityDoors.Add(securityDoor);
                        break;
                    case Turret turret:
                        currentLevelTurrets.Add(turret);
                        break;
                    case Landmine landmine:
                        currentLevelLandmines.Add(landmine);
                        break;
                    case SpikeRoofTrap spikeTrap:
                        currentLevelSpikeTraps.Add(spikeTrap);
                        break;
                    case BreakerBox breakerBox:
                        currentLevelBreakerBoxes.Add(breakerBox);
                        break;
                    case EnemyVent enemyVent:
                        currentLevelVents.Add(enemyVent);
                        break;
                    case SteamValveHazard steamValve:
                        currentLevelSteamValves.Add(steamValve);
                        break;
                    case SandSpiderWebTrap spiderWeb:
                        currentLevelSpiderWebs.Add(spiderWeb);
                        break;
                    case RandomScrapSpawn scrapSpawn:
                        currentScrapSpawnPoints.Add(scrapSpawn);
                        break;
                    case VehicleController vehicleController:
                        currentLevelVehicles.Add(vehicleController);
                        break;
                    case GrabbableObject item:
                        currentLevelItems.Add(item);
                        break;
                    case EnemyAI entity:
                        currentLevelEntities.Add(entity);
                        break;
                    case StoryLog storyLog:
                        currentLevelStoryLogs.Add(storyLog);
                        break;
                    case NetworkObject netObj:
                        CurrentLevelObjects[netObj.NetworkObjectId] = netObj;
                        break;
                }
            }
        }

        CurrentLevelItems.Set(currentLevelItems);
        CurrentLevelDoors.Set(currentLevelDoors);
        CurrentLevelVents.Set(currentLevelVents);
        CurrentLevelTurrets.Set(currentLevelTurrets);
        CurrentLevelVehicles.Set(currentLevelVehicles);
        CurrentLevelEntities.Set(currentLevelEntities);
        CurrentLevelLandmines.Set(currentLevelLandmines);
        CurrentLevelStoryLogs.Set(currentLevelStoryLogs);
        CurrentLevelSpiderWebs.Set(currentLevelSpiderWebs);
        CurrentLevelSpikeTraps.Set(currentLevelSpikeTraps);
        CurrentLevelVainShrouds.Set(currentLevelVainShrouds);
        CurrentLevelSteamValves.Set(currentLevelSteamValves);
        CurrentScrapSpawnPoints.Set(currentScrapSpawnPoints);
        CurrentLevelBreakerBoxes.Set(currentLevelBreakerBoxes);
        CurrentLevelSecurityDoors.Set(currentLevelSecurityDoors);
        CurrentLevelOutsideObjects.Set(currentLevelOutsideObjects);

        stopwatch.Stop();
        Imperium.IO.LogDebug($"[PROFILE] Objects refresh time : {stopwatch.ElapsedMilliseconds}");

        CurrentLevelObjectsChanged?.Invoke();

        stopwatch2.Stop();
        Imperium.IO.LogDebug($"[PROFILE] Total objects refresh time : {stopwatch2.ElapsedMilliseconds}");
    }

    private void GenerateDisplayNameMaps()
    {
        foreach (var entity in LoadedEntities.Value)
        {
            if (!entity.enemyPrefab) continue;
            var displayName = entity.enemyPrefab.GetComponentInChildren<ScanNodeProperties>()?.headerText;
            if (!string.IsNullOrEmpty(displayName)) displayNameMap[entity.enemyName] = displayName;
        }

        foreach (var item in LoadedItems.Value)
        {
            if (!item.spawnPrefab) continue;
            var displayName = item.spawnPrefab.GetComponentInChildren<ScanNodeProperties>()?.headerText;
            if (!string.IsNullOrEmpty(displayName)) displayNameMap[item.itemName] = displayName;
        }

        displayNameMap["MoldSpore"] = "Vain Shroud";
        displayNameMap["Maneater"] = "Cave Dweller";
        displayNameMap["Cadaver Bloom"] = "Cadaver Bloom";

        overrideDisplayNameMap["StickyNote"] = "Sticky Note";
        overrideDisplayNameMap["Clipboard"] = "Clipboard";
        overrideDisplayNameMap["CompanyCruiserManual"] = "Company Cruiser Manual";
        overrideDisplayNameMap["Body"] = "Player Body";
        overrideDisplayNameMap["GiantPumpkin"] = "Giant Pumpkin";
        overrideDisplayNameMap["LargeRock1"] = "Large Rock 1";
        overrideDisplayNameMap["LargeRock2"] = "Large Rock 2";
        overrideDisplayNameMap["LargeRock3"] = "Large Rock 3";
        overrideDisplayNameMap["LargeRock4"] = "Large Rock 4";
        overrideDisplayNameMap["GreyRockGrouping2"] = "Grey Rock Grouping 2";
        overrideDisplayNameMap["GreyRockGrouping4"] = "Grey Rock Grouping 4";
        overrideDisplayNameMap["tree"] = "Tree";
        overrideDisplayNameMap["treeLeafless"] = "Tree Leafless";
        overrideDisplayNameMap["treeLeaflessBrown.001 Variant"] = "Tree Leafless Brown";
        overrideDisplayNameMap["treeLeafless.002_LOD0"] = "Tree Leafless 2 (Snowy)";
        overrideDisplayNameMap["treeLeafless.003_LOD0"] = "Tree Leafless 3 (Snowy)";

        // Copied names for instantiated objects
        overrideDisplayNameMap["GiantPumpkin(Clone)"] = "Giant Pumpkin";
        overrideDisplayNameMap["LargeRock1(Clone)"] = "Large Rock 1";
        overrideDisplayNameMap["LargeRock2(Clone)"] = "Large Rock 2";
        overrideDisplayNameMap["LargeRock3(Clone)"] = "Large Rock 3";
        overrideDisplayNameMap["LargeRock4(Clone)"] = "Large Rock 4";
        overrideDisplayNameMap["GreyRockGrouping2(Clone)"] = "Grey Rock Grouping 2";
        overrideDisplayNameMap["GreyRockGrouping4(Clone)"] = "Grey Rock Grouping 4";
        overrideDisplayNameMap["tree(Clone)"] = "Tree";
        overrideDisplayNameMap["treeLeaflessBrown.001 Variant(Clone)"] = "Tree Leafless 1";
        overrideDisplayNameMap["treeLeafless.002_LOD0(Clone)"] = "Tree Leafless 2 (Snowy)";
        overrideDisplayNameMap["treeLeafless.003_LOD0(Clone)"] = "Tree Leafless 3 (Snowy)";
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
        Imperium.IO.LogBlock([
            "Imperium scanned the current level for obstacles.",
            $"   > {CurrentLevelDoors.Value.Count}x Doors",
            $"   > {CurrentLevelSecurityDoors.Value.Count}x Security doors",
            $"   > {CurrentLevelTurrets.Value.Count}x Turrets",
            $"   > {CurrentLevelLandmines.Value.Count}x Landmines",
            $"   > {CurrentLevelBreakerBoxes.Value.Count}x Breaker boxes",
            $"   > {CurrentLevelSpiderWebs.Value.Count}x Spider webs"
        ]);
    }

    #region RPC Handlers

    [ImpAttributes.HostOnly]
    private void OnSpawnEntity(EntitySpawnRequest request, ulong clientId)
    {
        var spawningEntity = LoadedEntities.Value.FirstOrDefault(entity => entity.enemyName == request.Name);
        var enemyPrefab = spawningEntity?.enemyPrefab;

        if (!spawningEntity || !enemyPrefab || !enemyPrefab.GetComponent<EnemyAI>())
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find requested entity '{request.Name}'.");
            return;
        }

        // Raycast to find the ground to spawn the entity on
        var hasGround = Physics.Raycast(
            new Ray(request.SpawnPosition + Vector3.up * 2f, Vector3.down),
            out var groundInfo, 100, ImpConstants.IndicatorMask
        );

        var player = Imperium.StartOfRound.allPlayerScripts.First(player => player.actualClientId == clientId);
        var actualSpawnPosition = hasGround
            ? groundInfo.point
            : clientId.GetPlayerController()!.transform.position;

        for (var i = 0; i < request.Amount; i++)
        {
            var entityObj = request.Name switch
            {
                "Shiggy" => InstantiateShiggy(spawningEntity, actualSpawnPosition),
                _ => Instantiate(
                    enemyPrefab,
                    actualSpawnPosition,
                    Quaternion.LookRotation(player.transform.position - actualSpawnPosition)
                )
            };
            var entity = entityObj.GetComponent<EnemyAI>();
            Imperium.RoundManager.SpawnedEnemies.Add(entity);

            if (request.Health > 0) entity.enemyHP = request.Health;

            var netObject = entityObj.gameObject.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);
            CurrentLevelObjects[netObject.NetworkObjectId] = netObject;

            // Checked if spawned entity is a masked and the masked parameters are set
            if (
                entityObj.TryGetComponent<MaskedPlayerEnemy>(out var maskedEntity)
                && request is { MaskedPlayerId: > -1, MaskedName: not null }
            )
            {
                AssignMaskedToPlayer(maskedEntity, (ulong)request.MaskedPlayerId, request.MaskedName);
            }
            else if (entityObj.TryGetComponent<CadaverBloomAI>(out _))
            {
                // Send delayed burst command if entity is cadaver bloom
                IEnumerator Routine()
                {
                    yield return new WaitForSeconds(0.2f);
                    burstCadaverBloomMessage.DispatchToClients(new BurstCadaverBloomRequest
                    {
                        NetObj = netObject,
                        PlayerId = clientId,
                        Position = actualSpawnPosition
                    });
                }

                StartCoroutine(Routine());
            }
        }

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        var verbString = request.Amount == 1 ? "has" : "have";

        if (request.SendNotification)
        {
            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"{mountString} loyal {GetDisplayName(request.Name)} {verbString} been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
    }

    private static GameObject InstantiateShiggy(EnemyType enemyType, Vector3 spawnPosition)
    {
        var shiggyPrefab = Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
        shiggyPrefab.name = "ShiggyEntity";
        Destroy(shiggyPrefab.GetComponent<TestEnemy>());
        Destroy(shiggyPrefab.GetComponent<HDAdditionalLightData>());
        Destroy(shiggyPrefab.GetComponent<Light>());
        Destroy(shiggyPrefab.GetComponent<AudioSource>());
        foreach (var componentsInChild in shiggyPrefab.GetComponentsInChildren<BoxCollider>())
        {
            Destroy(componentsInChild);
        }

        var shiggyAI = shiggyPrefab.AddComponent<ShiggyAI>();
        shiggyAI.enemyType = enemyType;

        return shiggyPrefab;
    }

    private static void AssignMaskedToPlayer(MaskedPlayerEnemy maskedEntity, ulong playerId, string maskedName)
    {
        var mimickPlayer = playerId.GetPlayerController();
        if (!mimickPlayer) return;

        maskedEntity.mimickingPlayer = mimickPlayer;
        maskedEntity.SetSuit(mimickPlayer.currentSuitID);
        maskedEntity.SetEnemyOutside(!mimickPlayer.isInsideFactory);
        maskedEntity.SetVisibilityOfMaskedEnemy();

        var usernameBillboard = maskedEntity.transform.Find("PlayerUsernameCanvas");
        var usernameBillboardText = usernameBillboard.GetComponentInChildren<TextMeshProUGUI>();
        var usernameBillboardAlpha = usernameBillboard.GetComponentInChildren<CanvasGroup>();
        usernameBillboardText.text = maskedName;
        usernameBillboardAlpha.alpha = 1;
        usernameBillboard.gameObject.SetActive(true);
    }

    [ImpAttributes.LocalMethod]
    private void DespawnLocalObject(LocalObjectType type, Vector3 position, GameObject obj)
    {
        if (!obj)
        {
            Imperium.IO.LogError(
                $"[SPAWN] [R] Failed to despawn local object of type '{type}' at {Formatting.FormatVector(position)}."
            );
            return;
        }

        Destroy(obj);
        RefreshLevelObjects();
    }

    [ImpAttributes.LocalMethod]
    private static void TeleportLocalObject(LocalObjectType type, Vector3 position, GameObject obj, Vector3 destination)
    {
        if (!obj)
        {
            Imperium.IO.LogError(
                $"[SPAWN] [R] Failed to local teleport object of type '{type}' at {Formatting.FormatVector(position)}."
            );
            return;
        }

        obj.transform.position = destination;
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnItem(ItemSpawnRequest request, ulong clientId)
    {
        var spawningItem = LoadedItems.Value.FirstOrDefault(item => item.itemName == request.Name);
        var itemPrefab = spawningItem?.spawnPrefab;

        if (!spawningItem || !itemPrefab || !itemPrefab.GetComponent<GrabbableObject>())
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find requested item '{request.Name}'.");
            return;
        }

        for (var i = 0; i < request.Amount; i++)
        {
            var itemObj = Instantiate(
                itemPrefab,
                request.SpawnPosition,
                Quaternion.identity,
                Imperium.RoundManager.spawnedScrapContainer
            );

            var grabbableItem = itemObj.GetComponent<GrabbableObject>();

            var value = request.Value;

            if (spawningItem)
            {
                if (value == -1) value = ImpUtils.RandomItemValue(spawningItem);
                grabbableItem.transform.rotation = Quaternion.Euler(spawningItem.restingRotation);
            }

            grabbableItem.SetScrapValue(value);

            // Execute start immediately to initialize random generator for animated objects
            grabbableItem.Start();

            var netObject = itemObj.gameObject.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);
            CurrentLevelObjects[netObject.NetworkObjectId] = netObject;

            // If player has free slot, place it in hand, otherwise leave it on the ground and play sound
            var spawnedInInventory = false;
            if (request.SpawnInInventory)
            {
                var invokingPlayer = Imperium.StartOfRound.allPlayerScripts.First(player =>
                    player.actualClientId == clientId
                );
                var firstItemSlot = invokingPlayer.FirstEmptyItemSlot();
                if (firstItemSlot != -1 && grabbableItem.grabbable)
                {
                    grabbableItem.InteractItem();
                    PlayerManager.GrabObject(grabbableItem, invokingPlayer);
                    spawnedInInventory = true;
                }
            }

            if (!spawnedInInventory)
            {
                var itemTransform = grabbableItem.transform;
                itemTransform.position = request.SpawnPosition + Vector3.up;
                grabbableItem.startFallingPosition = itemTransform.position;
                if (grabbableItem.transform.parent)
                {
                    grabbableItem.startFallingPosition = grabbableItem.transform.parent.InverseTransformPoint(
                        grabbableItem.startFallingPosition
                    );
                }

                grabbableItem.FallToGround();

                if (grabbableItem.itemProperties.dropSFX)
                {
                    Imperium.Player.itemAudio.PlayOneShot(grabbableItem.itemProperties.dropSFX);
                }
            }
        }

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        var verbString = request.Amount == 1 ? "has" : "have";

        if (request.SendNotification)
        {
            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"{mountString} {request.Name} {verbString} been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnMapHazard(MapHazardSpawnRequest request, ulong clientId)
    {
        if (!LoadedMapHazards.Value.TryGetValue(request.Name, out var hazardType) || !hazardType.prefabToSpawn)
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find indoor hazard prefab '{request.Name}'.");
            return;
        }

        for (var i = 0; i < request.Amount; i++)
        {
            var hazardObj = Instantiate(
                hazardType.prefabToSpawn,
                request.SpawnPosition, Quaternion.identity
            );

            var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);
            CurrentLevelObjects[netObject.NetworkObjectId] = netObject;
        }

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        var verbString = request.Amount == 1 ? "has" : "have";

        if (request.SendNotification)
        {
            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"{mountString} {request.Name} {verbString} been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.LocalMethod]
    private void OnSpawnOutsideObjectClient(StaticPrefabSpawnRequest request)
    {
        if (!LoadedOutsideObjects.Value.TryGetValue(request.Name, out var outsideObject))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find outside object '{request.Name}'.");
            return;
        }

        var mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");

        for (var i = 0; i < request.Amount; i++)
        {
            var obj = Instantiate(outsideObject.prefabToSpawn, mapPropsContainer.transform);
            obj.transform.position = request.SpawnPosition;
        }

        if (request.SendNotification)
        {
            var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
            var verbString = request.Amount == 1 ? "has" : "have";

            var objectName = overrideDisplayNameMap.GetValueOrDefault(request.Name)
                             ?? displayNameMap.GetValueOrDefault(request.Name)
                             ?? request.Name;

            Imperium.IO.Send(
                $"{mountString} {objectName} {verbString} been spawned!",
                type: NotificationType.Spawning
            );
        }

        RefreshLevelObjects();
    }

    [ImpAttributes.LocalMethod]
    private void OnSpawnLocalStaticPrefabClient(StaticPrefabSpawnRequest request)
    {
        if (!LoadedLocalStaticPrefabs.Value.TryGetValue(request.Name, out var staticPrefab))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find local static prefab '{request.Name}'.");
            return;
        }

        var rotationOffset = Quaternion.identity;

        if (staticPrefab.TryGetComponent<SpawnableOutsideObject>(out var outsideObject))
        {
            rotationOffset = Quaternion.Euler(outsideObject.rotationOffset);
        }

        var mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");

        for (var i = 0; i < request.Amount; i++)
        {
            var obj = Instantiate(staticPrefab, mapPropsContainer.transform);
            obj.transform.position = request.SpawnPosition;
            obj.transform.rotation = rotationOffset;
        }

        if (request.SendNotification)
        {
            var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
            var verbString = request.Amount == 1 ? "has" : "have";

            var objectName = overrideDisplayNameMap.GetValueOrDefault(request.Name)
                             ?? displayNameMap.GetValueOrDefault(request.Name)
                             ?? request.Name;

            Imperium.IO.Send(
                $"{mountString} {objectName} {verbString} been spawned!",
                type: NotificationType.Spawning
            );
        }

        RefreshLevelObjects();
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnVehicle(VehicleSpawnRequest request, ulong clientId)
    {
        if (!LoadedVehicles.Value.TryGetValue(request.Name, out var spawningVehicle))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find requested item '{request.Name}'.");
            return;
        }

        if (!spawningVehicle.vehiclePrefab)
        {
            Imperium.IO.LogError($"[SPAWN] [R] Requested vehicle does not have a spawn prefab '{request.Name}'.");
            return;
        }

        // Raycast to find the ground to spawn the entity on
        var hasGround = Physics.Raycast(
            new Ray(request.SpawnPosition + Vector3.up * 2f, Vector3.down),
            out var groundInfo, 100, ImpConstants.IndicatorMask
        );
        var actualSpawnPosition = hasGround
            ? groundInfo.point
            : clientId.GetPlayerController()!.transform.position;

        var vehicleObj = Instantiate(
            spawningVehicle.vehiclePrefab,
            actualSpawnPosition + Vector3.up * 2.5f,
            Quaternion.identity,
            RoundManager.Instance.VehiclesContainer
        );

        var vehicleNetObject = vehicleObj.gameObject.GetComponentInChildren<NetworkObject>();
        vehicleNetObject.Spawn();
        CurrentLevelObjects[vehicleNetObject.NetworkObjectId] = vehicleNetObject;

        if (spawningVehicle.secondaryPrefab)
        {
            var secondaryObj = Instantiate(
                spawningVehicle.secondaryPrefab,
                actualSpawnPosition + Vector3.up * 2.5f,
                Quaternion.identity,
                RoundManager.Instance.VehiclesContainer
            );

            var secondaryNetObj = secondaryObj.gameObject.GetComponentInChildren<NetworkObject>();
            secondaryNetObj.Spawn();
            CurrentLevelObjects[secondaryNetObj.NetworkObjectId] = secondaryNetObj;
        }

        if (request.SendNotification)
        {
            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"A trusty {request.Name} has been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();

        vehicleSpawnResponseMessage.DispatchToClients(new VehicleSpawnResponse
        {
            NetObj = vehicleNetObject
        });
    }

    [ImpAttributes.LocalMethod]
    private void OnSpawnVehicleClient(VehicleSpawnResponse response)
    {
        if (!response.NetObj.TryGet(out var vehicleNetObj))
        {
            Imperium.IO.LogError("[SPAWN] Failed to initialize spawned vehicle.");
            return;
        }

        var vehicle = vehicleNetObj.GetComponent<VehicleController>();

        vehicle.mainRigidbody.MovePosition(vehicle.transform.position);
        vehicle.hasBeenSpawned = true;
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnStaticPrefabServer(StaticPrefabSpawnRequest request, ulong client)
    {
        if (!LoadedStaticPrefabs.Value.TryGetValue(request.Name, out var staticPrefab))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find static prefab '{request.Name}' requested by {client}.");
            return;
        }

        for (var i = 0; i < request.Amount; i++)
        {
            var staticObj = Instantiate(staticPrefab.gameObject, request.SpawnPosition, Quaternion.Euler(Vector3.zero));

            var netObject = staticObj.gameObject.GetComponent<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);

            CurrentLevelObjects[netObject.NetworkObjectId] = netObject;
        }

        if (request.SendNotification)
        {
            var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
            var verbString = request.Amount == 1 ? "has" : "have";

            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"{mountString} {request.Name} {verbString} been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnObjectTeleportationRequestServer(ObjectTeleportRequest request, ulong clientId)
    {
        objectTeleportationRequest.DispatchToClients(request);
    }

    [ImpAttributes.LocalMethod]
    private void OnObjectTeleportationRequestClient(ObjectTeleportRequest request)
    {
        if (!CurrentLevelObjects.TryGetValue(request.NetworkId, out var obj) || !obj)
        {
            Imperium.IO.LogError($"[NET] Failed to teleport object item with net ID {request.NetworkId}");
            return;
        }

        if (obj.TryGetComponent<GrabbableObject>(out var item))
        {
            var itemTransform = item.transform;
            itemTransform.position = request.Destination + Vector3.up;
            item.startFallingPosition = itemTransform.position;
            if (item.transform.parent)
            {
                item.startFallingPosition = item.transform.parent.InverseTransformPoint(item.startFallingPosition);
            }

            item.FallToGround();
            item.PlayDropSFX();
        }
        else if (obj.TryGetComponent<Landmine>(out _))
        {
            obj.transform.parent.position = request.Destination;
        }
        else
        {
            obj.transform.position = request.Destination;
        }
    }

    [ImpAttributes.LocalMethod]
    private void OnLocalObjectTeleportationMessageClient(LocalObjectTeleportRequest request)
    {
        switch (request.Type)
        {
            case LocalObjectType.VainShroud:
                TeleportLocalObject(
                    request.Type,
                    request.Position,
                    CurrentLevelVainShrouds.Value
                        .Where(obj => obj)
                        .FirstOrDefault(obj => obj.transform.position == request.Position),
                    request.Destination
                );
                break;
            case LocalObjectType.OutsideObject:
                TeleportLocalObject(
                    request.Type,
                    request.Position,
                    CurrentLevelOutsideObjects.Value
                        .Where(obj => obj)
                        .FirstOrDefault(obj => obj.transform.position == request.Position),
                    request.Destination
                );
                break;
            default:
                Imperium.IO.LogError($"[NET] Local teleportation request has invalid outside object type '{request.Type}'");
                break;
        }
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnItem(ulong itemNetId, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(itemNetId, out var obj))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn item with net ID {itemNetId}");
            return;
        }

        if (obj.TryGetComponent<GrabbableObject>(out var grabbableObject))
        {
            if (grabbableObject.isHeld && grabbableObject.playerHeldBy is not null)
            {
                Imperium.PlayerManager.DropItem(new DropItemRequest
                {
                    PlayerId = grabbableObject.playerHeldBy.playerClientId,
                    ItemIndex = PlayerManager.GetItemHolderSlot(grabbableObject)
                });
            }
        }

        DespawnObject(obj.gameObject);
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnEntity(EntityDespawnRequest request, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(request.NetId, out var obj) || !obj)
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn entity with net ID {request.NetId}");
            return;
        }

        if (obj.TryGetComponent<SandSpiderAI>(out var sandSpider))
        {
            for (var i = 0; i < sandSpider.webTraps.Count; i++)
            {
                sandSpider.BreakWebServerRpc(i, (int)clientId);
            }
        }

        DespawnObject(obj.gameObject, request.IsRespawn);
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnVehicle(VehicleDespawnRequest request, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(request.NetId, out var obj) || !obj)
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn vehicle with net ID {request.NetId}");
            return;
        }

        var vehicle = obj.GetComponent<VehicleController>();

        // Despawn vehicle but don't destroy local object. Local object will destroy itself on each client.
        if (request.IsRespawn || vehicle.carDestroyed)
        {
            obj.Despawn(destroy: false);
        }
        else
        {
            vehicle.DestroyCarClientRpc((int)clientId);
        }

        if (!request.IsRespawn) objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnObstacle(ulong obstacleNetId, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(obstacleNetId, out var obj))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn obstacle with net ID {obstacleNetId}");
            return;
        }

        DespawnObject(obj.gameObject);
    }

    [ImpAttributes.LocalMethod]
    private void OnDespawnLocalObject(LocalObjectDespawnRequest request)
    {
        switch (request.Type)
        {
            case LocalObjectType.VainShroud:
                DespawnLocalObject(request.Type, request.Position, CurrentLevelVainShrouds.Value
                    .Where(obj => obj)
                    .FirstOrDefault(obj => obj.transform.position == request.Position)
                );
                break;
            case LocalObjectType.OutsideObject:
                DespawnLocalObject(request.Type, request.Position, CurrentLevelOutsideObjects.Value
                    .Where(obj => obj)
                    .FirstOrDefault(obj => obj.transform.position == request.Position)
                );
                break;
            default:
                Imperium.IO.LogError($"[NET] Despawn request has invalid outside object type '{request.Type}'");
                break;
        }
    }

    [ImpAttributes.LocalMethod]
    private static void OnSteamValveBurst(ulong valveNetId)
    {
        var steamValve = Imperium.ObjectManager.CurrentLevelObjects[valveNetId].GetComponent<SteamValveHazard>();
        steamValve.valveHasBurst = true;
        steamValve.valveHasBeenRepaired = false;
        steamValve.BurstValve();
    }

    [ImpAttributes.LocalMethod]
    private static void OnCadaverBloomMessageBurst(BurstCadaverBloomRequest request)
    {
        if (!request.NetObj.TryGet(out var networkObject))
        {
            Imperium.IO.LogError("Failed to burst cadaver bloom. Network object not found.");
            return;
        }

        if (!networkObject.TryGetComponent<CadaverBloomAI>(out var cadaverBloom))
        {
            Imperium.IO.LogError("Failed to burst cadaver bloom. Network object does not have enemy script.");
            return;
        }

        var player = Imperium.StartOfRound.allPlayerScripts.First(player => player.actualClientId == request.PlayerId);
        cadaverBloom.BurstForth(
            player, false, request.Position,
            Quaternion.LookRotation(player.transform.position - request.Position).eulerAngles
        );
    }

    #endregion

    [ImpAttributes.HostOnly]
    private void DespawnObject(GameObject obj, bool isRespawn = false)
    {
        if (!obj) return;

        try
        {
            if (obj.TryGetComponent<NetworkObject>(out var networkObject)) networkObject.Despawn();
        }
        finally
        {
            Destroy(obj);
            if (!isRespawn) objectsChangedEvent.DispatchToClients();
        }
    }
}
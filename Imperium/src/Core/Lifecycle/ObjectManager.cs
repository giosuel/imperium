#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GameNetcodeStuff;
using Imperium.API.Types.Networking;
using Imperium.Core.Scripts;
using Imperium.Core.Scripts.Tags;
using Imperium.Extensions;
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
///     Lifecycle object that manages all object-related functionality. Keeps track of loaded and currently active objects.
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
    internal readonly ImpBinding<IReadOnlyCollection<SteamValveHazard>> CurrentLevelSteamValves = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SandSpiderWebTrap>> CurrentLevelSpiderWebs = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<VehicleController>> CurrentLevelVehicles = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<TerminalAccessibleObject>> CurrentLevelSecurityDoors = new([]);

    /*
     * Lists of local objects that don't have a network object or script to reference
     */
    internal readonly ImpBinding<IReadOnlyCollection<GameObject>> CurrentLevelVainShrouds = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<ImpOutsideObjectTag>> CurrentLevelOutsideObjects = new([]);

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
    internal readonly ImpNetworkBinding<HashSet<NetworkObjectReference>> DisabledObjects = new(
        "DisabledObjects", Imperium.Networking, []
    );

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

    private readonly ImpNetMessage<ObjectTeleportRequest> objectTeleportationMessage = new(
        "ObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<OutsideObjectTeleportRequest> outsideObjectTeleportationMessage = new(
        "OutsideObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<VainShroudTeleportRequest> vainShroudTeleportationMessage = new(
        "VainShroudTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<SpiderWebTeleportRequest> spiderWebTeleportationMessage = new(
        "SpiderWebTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<BurstCadaverBloomRequest> burstCadaverBloomMessage = new(
        "BurstCadaverBloom", Imperium.Networking
    );

    private readonly ImpNetMessage<NetworkObjectReference> burstSteamValve = new(
        "BurstSteamValve", Imperium.Networking
    );

    private readonly ImpNetMessage<EntityDespawnRequest> entityDespawnMessage = new(
        "DespawnEntity", Imperium.Networking
    );

    private readonly ImpNetMessage<VehicleDespawnRequest> vehicleDespawnMessage = new(
        "DespawnVehicle", Imperium.Networking
    );

    private readonly ImpNetMessage<NetworkObjectReference> itemDespawnMessage = new(
        "DespawnItem", Imperium.Networking
    );

    private readonly ImpNetMessage<NetworkObjectReference> obstacleDespawnMessage = new(
        "DespawnObstacle", Imperium.Networking
    );

    private readonly ImpNetMessage<SpiderWebDespawnRequest> spiderWebDespawnMessage = new(
        "DespawnSpiderWeb", Imperium.Networking
    );

    private readonly ImpNetMessage<OutsideObjectDespawnRequest> outsideObjectDespawnMessage = new(
        "DespawnOutsideObject", Imperium.Networking
    );

    private readonly ImpNetMessage<VainShroudDespawnRequest> vainShroudDespawnMessage = new(
        "DespawnVainShroud", Imperium.Networking
    );

    private readonly ImpNetEvent objectsChangedEvent = new(
        "ObjectsChanged", Imperium.Networking
    );

    private readonly ImpTimer periodicUpdateTimer = ImpTimer.ForInterval(1);

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

    protected override void Init()
    {
        FetchGlobalSpawnLists();
        FetchPlayers();

        TriggerRefresh();

        LogObjects();

        objectsChangedEvent.OnClientRecive += TriggerRefresh;

        burstSteamValve.OnClientRecive += OnSteamValveBurstClient;
        vehicleSpawnResponseMessage.OnClientRecive += OnSpawnVehicleClient;
        burstCadaverBloomMessage.OnClientRecive += OnCadaverBloomMessageBurstClient;

        outsideObjectPrefabSpawnMessage.OnClientRecive += OnSpawnOutsideObjectClient;
        localStaticPrefabSpawnMessage.OnClientRecive += OnSpawnLocalStaticPrefabClient;

        vainShroudDespawnMessage.OnClientRecive += OnDespawnVainShroudClient;
        outsideObjectDespawnMessage.OnClientRecive += OnDespawnOutsideObjectClient;

        objectTeleportationMessage.OnClientRecive += OnObjectTeleportationClient;
        spiderWebTeleportationMessage.OnClientRecive += OnSpiderWebTeleportationClient;
        vainShroudTeleportationMessage.OnClientRecive += OnVainShroudTeleportationClient;
        outsideObjectTeleportationMessage.OnClientRecive += OnOutsideObjectTeleportationClient;

        if (NetworkManager.Singleton.IsHost)
        {
            itemSpawnMessage.OnServerReceive += OnSpawnItem;
            entitySpawnMessage.OnServerReceive += OnSpawnEntity;
            vehicleSpawnMessage.OnServerReceive += OnSpawnVehicle;
            mapHazardSpawnMessage.OnServerReceive += OnSpawnMapHazard;
            staticPrefabSpawnMessage.OnServerReceive += OnSpawnStaticPrefabServer;
            outsideObjectPrefabSpawnMessage.OnServerReceive += OnSpawnOutsideObjectServer;
            localStaticPrefabSpawnMessage.OnServerReceive += OnSpawnLocalStaticPrefabServer;

            itemDespawnMessage.OnServerReceive += OnDespawnItem;
            entityDespawnMessage.OnServerReceive += OnDespawnEntity;
            vehicleDespawnMessage.OnServerReceive += OnDespawnVehicle;
            obstacleDespawnMessage.OnServerReceive += OnDespawnObstacle;
            spiderWebDespawnMessage.OnServerReceive += OnDespawnSpiderWeb;
            vainShroudDespawnMessage.OnServerReceive += OnDespawnVainShroudServer;
            outsideObjectDespawnMessage.OnServerReceive += OnDespawnOutsideObjectServer;

            objectTeleportationMessage.OnServerReceive += OnObjectTeleportationServer;
            spiderWebTeleportationMessage.OnServerReceive += OnSpiderWebTeleportationServer;
            vainShroudTeleportationMessage.OnServerReceive += OnVainShroudTeleportationServer;
            outsideObjectTeleportationMessage.OnServerReceive += OnOutsideObjectTeleportationServer;
        }
    }

    protected override void OnSceneLoad()
    {
        TriggerRefresh();

        LogObjects();

        // Reload objects that are hidden on the moon but visible in space
        Imperium.Settings.Rendering.SpaceSun.Refresh();
        Imperium.Settings.Rendering.StarsOverlay.Refresh();
    }

    protected override void OnPlayersUpdate(int playersConnected) => FetchPlayers();

    [ImpAttributes.RemoteMethod]
    internal void SpawnEntity(EntitySpawnRequest request)
    {
        entitySpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnItem(ItemSpawnRequest request)
    {
        itemSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnVehicle(VehicleSpawnRequest request)
    {
        vehicleSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnMapHazard(MapHazardSpawnRequest request)
    {
        mapHazardSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnStaticPrefab(StaticPrefabSpawnRequest request)
    {
        staticPrefabSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnLocalStaticPrefab(StaticPrefabSpawnRequest request)
    {
        localStaticPrefabSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnOutsideObject(StaticPrefabSpawnRequest request)
    {
        outsideObjectPrefabSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void TeleportObject(ObjectTeleportRequest request)
    {
        objectTeleportationMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void TeleportOutsideObject(OutsideObjectTeleportRequest request)
    {
        outsideObjectTeleportationMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void TeleportVainShroud(VainShroudTeleportRequest request)
    {
        vainShroudTeleportationMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void TeleportSpiderWeb(SpiderWebTeleportRequest request)
    {
        spiderWebTeleportationMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnItem(NetworkObjectReference itemNetObj)
    {
        itemDespawnMessage.DispatchToServer(itemNetObj);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnEntity(EntityDespawnRequest request)
    {
        entityDespawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnVehicle(VehicleDespawnRequest request)
    {
        vehicleDespawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnObstacle(NetworkObjectReference obstacleNetId)
    {
        obstacleDespawnMessage.DispatchToServer(obstacleNetId);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnOutsideObject(OutsideObjectDespawnRequest request)
    {
        outsideObjectDespawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnVainShroud(VainShroudDespawnRequest request)
    {
        vainShroudDespawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnSpiderWeb(SpiderWebDespawnRequest request)
    {
        spiderWebDespawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void BurstSteamValve(NetworkObjectReference valveNetObj)
    {
        burstSteamValve.DispatchToClients(valveNetObj);
    }

    internal string GetDisplayName(string inGameName) => displayNameMap.GetValueOrDefault(inGameName, inGameName);
    internal string GetOverrideDisplayName(string inGameName) => overrideDisplayNameMap.GetValueOrDefault(inGameName);

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

        // Add tag script to outside objects
        foreach (var obj in allOutsideObjects.Values)
        {
            if (!obj.prefabToSpawn.TryGetComponent<ImpOutsideObjectTag>(out _))
            {
                obj.prefabToSpawn.AddComponent<ImpOutsideObjectTag>();
            }
        }

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
        }

        CurrentLevelEntities.Set(currentLevelEntities);
        CurrentLevelObjectsChanged?.Invoke();
    }

    // Trigger object refresh in 100ms. This way newly spawned / despawned objects have time to register.
    internal void TriggerRefresh() => periodicUpdateTimer.SetCountdownTo(0.1f);

    private void RefreshLevelObjects()
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
        HashSet<ImpOutsideObjectTag> currentLevelOutsideObjects = [];
        HashSet<VehicleController> currentLevelVehicles = [];
        HashSet<SteamValveHazard> currentLevelSteamValves = [];
        HashSet<SandSpiderWebTrap> currentLevelSpiderWebs = [];
        HashSet<RandomScrapSpawn> currentScrapSpawnPoints = [];
        HashSet<TerminalAccessibleObject> currentLevelSecurityDoors = [];

        foreach (var obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            // This is cursed but there is no other way
            if (obj.name.Contains("MoldSpore 1") && currentLevelVainShrouds.Add(obj)) continue;

            // if (obj.layer == terrainMask
            //     && OutsideObjectPrefabNameMap.Contains(obj.name)
            //     && currentLevelOutsideObjects.Add(obj)
            //    )
            // {
            //     continue;
            // }

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
                    case GrabbableObject item:
                        currentLevelItems.Add(item);
                        break;
                    case EnemyAI entity:
                        currentLevelEntities.Add(entity);
                        break;
                    case StoryLog storyLog:
                        currentLevelStoryLogs.Add(storyLog);
                        break;
                    case VehicleController vehicleController:
                        currentLevelVehicles.Add(vehicleController);
                        break;
                    case ImpOutsideObjectTag outsideObjectTag:
                        currentLevelOutsideObjects.Add(outsideObjectTag);
                        break;
                }
            }
        }

        var hasSomethingChanged = false;

        SetIfChanged(CurrentLevelItems, currentLevelItems, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelDoors, currentLevelDoors, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelVents, currentLevelVents, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelTurrets, currentLevelTurrets, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelVehicles, currentLevelVehicles, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelEntities, currentLevelEntities, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelLandmines, currentLevelLandmines, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelStoryLogs, currentLevelStoryLogs, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelSpiderWebs, currentLevelSpiderWebs, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelSpikeTraps, currentLevelSpikeTraps, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelVainShrouds, currentLevelVainShrouds, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelSteamValves, currentLevelSteamValves, ref hasSomethingChanged);
        SetIfChanged(CurrentScrapSpawnPoints, currentScrapSpawnPoints, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelBreakerBoxes, currentLevelBreakerBoxes, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelSecurityDoors, currentLevelSecurityDoors, ref hasSomethingChanged);
        SetIfChanged(CurrentLevelOutsideObjects, currentLevelOutsideObjects, ref hasSomethingChanged);

        stopwatch.Stop();
        Imperium.IO.LogDebug($"[PROFILE] Objects refresh time : {stopwatch.ElapsedMilliseconds}");

        if (hasSomethingChanged)
        {
            Imperium.IO.LogInfo("SOMETHING HAS CHANGED");
            CurrentLevelObjectsChanged?.Invoke();
        }

        stopwatch2.Stop();
        Imperium.IO.LogDebug($"[PROFILE] Total objects refresh time : {stopwatch2.ElapsedMilliseconds}");
    }

    private static void SetIfChanged<T>(ImpBinding<IReadOnlyCollection<T>> current, HashSet<T> updated, ref bool hasChanged)
    {
        if (current.Value is HashSet<T> currentHashSet && currentHashSet.SetEquals(updated))
        {
            return;
        }

        current.Set(updated);
        hasChanged = true;
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
        displayNameMap["TurretContainer"] = "Turret";
        displayNameMap["SpikeRoofTrapHazard"] = "Spike Trap";

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

            var netObject = entityObj.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);

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
                        NetworkObj = netObject,
                        PlayerId = clientId,
                        Position = actualSpawnPosition
                    });
                }

                StartCoroutine(Routine());
            }
        }

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount}x";
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
    private void DespawnLocalObject(GameObject obj, Vector3 position)
    {
        if (!obj)
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to despawn local object at {Formatting.FormatVector(position)}."
            );
            return;
        }

        Destroy(obj);
        TriggerRefresh();
    }

    [ImpAttributes.LocalMethod]
    private static void TeleportLocalObject(GameObject obj, Vector3 position, Vector3 destination)
    {
        if (!obj)
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to teleport local object at {Formatting.FormatVector(position)}."
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

            var netObject = itemObj.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);

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

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount}x";
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

            var netObject = hazardObj.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);
        }

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount}x";
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
            var obj = Instantiate(outsideObject.prefabToSpawn, mapPropsContainer?.transform);
            obj.transform.position = request.SpawnPosition;
        }

        if (request.SendNotification)
        {
            var mountString = request.Amount == 1 ? "A" : $"{request.Amount}x";
            var verbString = request.Amount == 1 ? "has" : "have";

            var objectName = overrideDisplayNameMap.GetValueOrDefault(request.Name)
                             ?? displayNameMap.GetValueOrDefault(request.Name)
                             ?? request.Name;

            Imperium.IO.Send(
                $"{mountString} {objectName} {verbString} been spawned!",
                type: NotificationType.Spawning
            );
        }

        TriggerRefresh();
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnLocalStaticPrefabServer(StaticPrefabSpawnRequest request, ulong clientId)
    {
        localStaticPrefabSpawnMessage.DispatchToClients(request);
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnOutsideObjectServer(StaticPrefabSpawnRequest request, ulong clientId)
    {
        if (!LoadedOutsideObjects.Value.TryGetValue(request.Name, out var outsideObject))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find outside object '{request.Name}'.");
            return;
        }

        /*
         * It is possible for outside objects to be a network object, if they have been added by DawnLib.
         * In this case, we want to spawn the network object. Otherwise, we want every client to instantiate it individually.
         */
        if (outsideObject.prefabToSpawn.TryGetComponent<NetworkObject>(out _))
        {
            var mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");

            for (var i = 0; i < request.Amount; i++)
            {
                var obj = Instantiate(outsideObject.prefabToSpawn, mapPropsContainer?.transform);
                obj.transform.position = request.SpawnPosition;

                obj.GetComponent<NetworkObject>().Spawn();
            }

            if (request.SendNotification)
            {
                var mountString = request.Amount == 1 ? "A" : $"{request.Amount}x";
                var verbString = request.Amount == 1 ? "has" : "have";

                var objectName = overrideDisplayNameMap.GetValueOrDefault(request.Name)
                                 ?? displayNameMap.GetValueOrDefault(request.Name)
                                 ?? request.Name;

                Imperium.Networking.SendLog(new NetworkNotification
                {
                    Message = $"{mountString} {objectName} {verbString} been spawned!",
                    Type = NotificationType.Spawning
                });
            }

            objectsChangedEvent.DispatchToClients();
        }
        else
        {
            outsideObjectPrefabSpawnMessage.DispatchToClients(request);
        }
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
            var obj = Instantiate(staticPrefab, mapPropsContainer?.transform);
            obj.transform.position = request.SpawnPosition;
            obj.transform.rotation = rotationOffset;
        }

        if (request.SendNotification)
        {
            var mountString = request.Amount == 1 ? "A" : $"{request.Amount}x";
            var verbString = request.Amount == 1 ? "has" : "have";

            var objectName = overrideDisplayNameMap.GetValueOrDefault(request.Name)
                             ?? displayNameMap.GetValueOrDefault(request.Name)
                             ?? request.Name;

            Imperium.IO.Send(
                $"{mountString} {objectName} {verbString} been spawned!",
                type: NotificationType.Spawning
            );
        }

        TriggerRefresh();
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

        var vehicleNetObject = vehicleObj.GetComponentInChildren<NetworkObject>();
        vehicleNetObject.Spawn();

        if (spawningVehicle.secondaryPrefab)
        {
            var secondaryObj = Instantiate(
                spawningVehicle.secondaryPrefab,
                actualSpawnPosition + Vector3.up * 2.5f,
                Quaternion.identity,
                RoundManager.Instance.VehiclesContainer
            );

            var secondaryNetObj = secondaryObj.GetComponentInChildren<NetworkObject>();
            secondaryNetObj.Spawn();
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

            var netObject = staticObj.GetComponent<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);
        }

        if (request.SendNotification)
        {
            var mountString = request.Amount == 1 ? "A" : $"{request.Amount}x";
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
    private void OnObjectTeleportationServer(ObjectTeleportRequest request, ulong clientId)
    {
        objectTeleportationMessage.DispatchToClients(request);
    }

    [ImpAttributes.HostOnly]
    private void OnOutsideObjectTeleportationServer(OutsideObjectTeleportRequest request, ulong clientId)
    {
        outsideObjectTeleportationMessage.DispatchToClients(request);
    }

    [ImpAttributes.HostOnly]
    private void OnVainShroudTeleportationServer(VainShroudTeleportRequest request, ulong clientId)
    {
        vainShroudTeleportationMessage.DispatchToClients(request);
    }

    [ImpAttributes.HostOnly]
    private void OnSpiderWebTeleportationServer(SpiderWebTeleportRequest request, ulong clientId)
    {
        spiderWebTeleportationMessage.DispatchToClients(request);
    }

    [ImpAttributes.LocalMethod]
    private void OnObjectTeleportationClient(ObjectTeleportRequest request)
    {
        if (!request.NetworkObj.TryGet(out var networkObj))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to teleport object. Invalid network object. ID: {request.NetworkObj.NetworkObjectId}"
            );
            return;
        }

        if (networkObj.TryGetComponent<GrabbableObject>(out var item))
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
        else if (networkObj.TryGetComponent<Landmine>(out _))
        {
            networkObj.transform.parent.position = request.Destination;
        }
        else
        {
            networkObj.transform.position = request.Destination;
        }
    }

    [ImpAttributes.LocalMethod]
    private void OnSpiderWebTeleportationClient(SpiderWebTeleportRequest request)
    {
        if (!request.SpiderNetObj.TryGetComponent<SandSpiderAI>(out _, out var sandSpider))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to teleport spider web. Invalid spider object. ID: {request.SpiderNetObj.NetworkObjectId}"
            );
            return;
        }

        if (request.TrapId < sandSpider.webTraps.Count)
        {
            sandSpider.webTraps[request.TrapId].transform.position = request.Position + Vector3.up;
        }
    }

    [ImpAttributes.LocalMethod]
    private void OnOutsideObjectTeleportationClient(OutsideObjectTeleportRequest request)
    {
        TeleportLocalObject(
            CurrentLevelOutsideObjects.Value
                .Where(obj => obj)
                .FirstOrDefault(obj => obj.transform.position == request.Position)?.gameObject,
            request.Position,
            request.Destination
        );
    }

    [ImpAttributes.LocalMethod]
    private void OnVainShroudTeleportationClient(VainShroudTeleportRequest request)
    {
        TeleportLocalObject(
            CurrentLevelVainShrouds.Value
                .Where(obj => obj)
                .FirstOrDefault(obj => obj.transform.position == request.Position),
            request.Position,
            request.Destination
        );
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnItem(NetworkObjectReference itemNetObj, ulong clientId)
    {
        if (!itemNetObj.TryGetComponent<GrabbableObject>(out var netObj, out var grabbableObject))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to despawn item. Invalid network object. ID: {itemNetObj.NetworkObjectId}"
            );
            return;
        }

        if (grabbableObject.isHeld && grabbableObject.playerHeldBy != null)
        {
            Imperium.PlayerManager.DropItem(new DropItemRequest
            {
                PlayerId = grabbableObject.playerHeldBy.playerClientId,
                ItemIndex = PlayerManager.GetItemHolderSlot(grabbableObject)
            });
        }

        // Check if the grabbable object is a bee hive. If so, despawn the bees as well.
        FindObjectsByType<RedLocustBees>(FindObjectsSortMode.None)
            .FirstOrDefault(bees => bees.hive == grabbableObject)
            ?.GetComponent<NetworkObject>()
            ?.Despawn();

        netObj.Despawn();
        objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnEntity(EntityDespawnRequest request, ulong clientId)
    {
        if (!request.EntityNetObj.TryGetComponent<EnemyAI>(out var netObj, out var entity))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to despawn entity. Invalid network object. ID: {request.EntityNetObj.NetworkObjectId}"
            );
            return;
        }

        // Remove all web traps if the destroyed entity was a spider
        if (entity.TryGetComponent<SandSpiderAI>(out var sandSpider))
        {
            foreach (var webTrap in sandSpider.webTraps.ToList())
            {
                sandSpider.BreakWebClientRpc(webTrap.transform.position, webTrap.trapID);
            }
        }

        netObj.Despawn();
        if (!request.IsRespawn) objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnVehicle(VehicleDespawnRequest request, ulong clientId)
    {
        if (!request.VehicleNetObj.TryGetComponent<VehicleController>(out var netObj, out var vehicle))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to despawn vehicle. Invalid network object. ID: {request.VehicleNetObj.NetworkObjectId}"
            );
            return;
        }

        // Despawn vehicle but don't destroy local object. Local object will destroy itself on each client.
        if (request.IsRespawn || vehicle.carDestroyed)
        {
            netObj.Despawn(destroy: false);
        }
        else
        {
            vehicle.DestroyCarClientRpc((int)clientId);
        }

        if (!request.IsRespawn) objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnObstacle(NetworkObjectReference obstacleNetObj, ulong clientId)
    {
        if (!obstacleNetObj.TryGet(out var netObj))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to despawn obstacle. Invalid network object. ID: {obstacleNetObj.NetworkObjectId}"
            );
            return;
        }

        netObj.Despawn();
        objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnOutsideObjectServer(OutsideObjectDespawnRequest request, ulong clientId)
    {
        var obj = CurrentLevelOutsideObjects.Value
            .Where(obj => obj)
            .FirstOrDefault(obj => obj.transform.position == request.Position)?.gameObject;

        /*
         * It is possible for outside objects to be a network object, if they have been added by DawnLib.
         * In this case, we want to despawn the network object. Otherwise, we want every client to destroy it individually.
         */
        if (obj && obj.TryGetComponent<NetworkObject>(out var netObj))
        {
            netObj.Despawn();
            objectsChangedEvent.DispatchToClients();
        }
        else
        {
            outsideObjectDespawnMessage.DispatchToClients(request);
        }
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnVainShroudServer(VainShroudDespawnRequest request, ulong clientId)
    {
        vainShroudDespawnMessage.DispatchToClients(request);
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnSpiderWeb(SpiderWebDespawnRequest request, ulong clientId)
    {
        if (!request.SpiderNetObj.TryGetComponent<SandSpiderAI>(out var netObj, out var sandSpider))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to despawn spider web. Invalid spider object. ID: {request.SpiderNetObj.NetworkObjectId}"
            );
            return;
        }

        if (request.TrapId < sandSpider.webTraps.Count)
        {
            sandSpider.BreakWebClientRpc(sandSpider.transform.position, request.TrapId);
        }
    }

    [ImpAttributes.LocalMethod]
    private void OnDespawnOutsideObjectClient(OutsideObjectDespawnRequest request)
    {
        DespawnLocalObject(
            CurrentLevelOutsideObjects.Value
                .Where(obj => obj)
                .FirstOrDefault(obj => obj.transform.position == request.Position)?.gameObject,
            request.Position
        );
    }

    [ImpAttributes.LocalMethod]
    private void OnDespawnVainShroudClient(VainShroudDespawnRequest request)
    {
        DespawnLocalObject(
            CurrentLevelVainShrouds.Value
                .Where(obj => obj)
                .FirstOrDefault(obj => obj.transform.position == request.Position),
            request.Position
        );
    }

    [ImpAttributes.LocalMethod]
    private static void OnSteamValveBurstClient(NetworkObjectReference valveNetObj)
    {
        if (!valveNetObj.TryGetComponent<SteamValveHazard>(out _, out var steamValve))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to burst steam valve. Invalid network object. ID: {valveNetObj.NetworkObjectId}"
            );
            return;
        }

        steamValve.valveHasBurst = true;
        steamValve.valveHasBeenRepaired = false;
        steamValve.BurstValve();
    }

    [ImpAttributes.LocalMethod]
    private static void OnCadaverBloomMessageBurstClient(BurstCadaverBloomRequest request)
    {
        if (!request.NetworkObj.TryGetComponent<CadaverBloomAI>(out _, out var cadaverBloom))
        {
            Imperium.IO.LogError(
                $"[OBJ] Failed to burst cadaver bloom. Invalid network object. ID: {request.NetworkObj.NetworkObjectId}"
            );
            return;
        }

        var player = Imperium.StartOfRound.allPlayerScripts.First(player => player.actualClientId == request.PlayerId);
        cadaverBloom.BurstForth(
            player, false, request.Position,
            Quaternion.LookRotation(player.transform.position - request.Position).eulerAngles
        );
    }

    #endregion

    private void Update()
    {
        if (periodicUpdateTimer.Tick()) RefreshLevelObjects();
    }
}
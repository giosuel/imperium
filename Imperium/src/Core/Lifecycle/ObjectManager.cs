#region

using System.Collections.Generic;
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
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

#endregion

namespace Imperium.Core.Lifecycle;

internal class ObjectManager : ImpLifecycleObject
{
    /*
     * Entity name system.
     */
    private readonly List<string> AvailableEntityNames = ImpAssets.EntityNames.Select(name => name).ToList();
    private readonly Dictionary<int, string> EntityNameMap = [];
    private bool JohnExists;

    /*
     * Lists of globally loaded objects.
     *
     * These lists hold all the entities that can be spawned in Lethal Company, including the ones that are not in any
     * spawn list of any moon (e.g. Red Pill, Lasso Man).
     *
     * Loaded on Imperium initialization.
     */
    internal readonly ImpBinding<IReadOnlyCollection<Item>> LoadedItems = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<Item>> LoadedScrap = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<EnemyType>> LoadedEntities = new([]);
    internal readonly ImpBinding<IReadOnlyDictionary<string, GameObject>> LoadedMapHazards = new();

    // Misc objects with network objects (e.g. clipboard, body, company cruiser)
    internal readonly ImpBinding<IReadOnlyDictionary<string, GameObject>> LoadedStaticPrefabs = new();
    internal readonly ImpBinding<IReadOnlyDictionary<string, SpawnableOutsideObject>> LoadedOutsideObjects = new();

    // Misc objects without network objects (e.g. trees, vain shrouds, rocks)
    internal readonly ImpBinding<IReadOnlyDictionary<string, GameObject>> LoadedLocalStaticPrefabs = new();

    /*
     * Lists of objects loaded in the current scene.
     *
     * These lists hold the currently existing objects on the map
     * These are used by the object list in Imperium UI and is always up-to-date but
     * CAN CONTAIN NULL elements that have been marked for but not yet deleted during the last refresh.
     *
     * Loaded on Imperium initialization. Refreshed when the ship is landing / taking off.
     */
    internal readonly ImpBinding<IReadOnlyCollection<Turret>> CurrentLevelTurrets = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<DoorLock>> CurrentLevelDoors = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<EnemyVent>> CurrentLevelVents = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<EnemyAI>> CurrentLevelEntities = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<Landmine>> CurrentLevelLandmines = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<PlayerControllerB>> CurrentPlayers = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<GrabbableObject>> CurrentLevelItems = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<BreakerBox>> CurrentLevelBreakerBoxes = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SpikeRoofTrap>> CurrentLevelSpikeTraps = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<VehicleController>> CurrentLevelCruisers = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SteamValveHazard>> CurrentLevelSteamValves = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SandSpiderWebTrap>> CurrentLevelSpiderWebs = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<TerminalAccessibleObject>> CurrentLevelSecurityDoors = new([]);

    // Local objects without a network object or script to reference
    internal readonly ImpBinding<IReadOnlyCollection<GameObject>> CurrentLevelVainShrouds = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<GameObject>> CurrentLevelOutsideObjects = new([]);

    // Event that is fired when multiple types of objects have been changed
    internal readonly ImpEvent CurrentLevelObjectsChanged = new();

    /*
     * Misc scene objects.
     */
    internal readonly ImpBinding<HashSet<RandomScrapSpawn>> CurrentScrapSpawnPoints = new([]);

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

    // Used by the server to execute a despawn request from a client via network ID
    private readonly Dictionary<ulong, GameObject> CurrentLevelObjects = [];

    private readonly Dictionary<string, string> displayNameMap = [];
    private readonly Dictionary<string, string> overrideDisplayNameMap = [];

    private readonly ImpNetMessage<EntitySpawnRequest> entitySpawnMessage = new("SpawnEntity", Imperium.Networking);
    private readonly ImpNetMessage<ItemSpawnRequest> itemSpawnMessage = new("SpawnItem", Imperium.Networking);

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

    private readonly ImpNetMessage<CompanyCruiserSpawnRequest> companyCruiserSpawnMessage = new(
        "CompanyCruiserSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<ObjectTeleportRequest> objectTeleportationRequest = new(
        "ObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<LocalObjectTeleportRequest> localObjectTeleportationRequest = new(
        "LocalObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<ulong> burstSteamValve = new("BurstSteamValve", Imperium.Networking);
    private readonly ImpNetMessage<ulong> entityDespawnMessage = new("DespawnEntity", Imperium.Networking);
    private readonly ImpNetMessage<ulong> itemDespawnMessage = new("DespawnItem", Imperium.Networking);
    private readonly ImpNetMessage<ulong> obstacleDespawnMessage = new("DespawnObstacle", Imperium.Networking);

    private readonly ImpNetMessage<LocalObjectDespawnRequest> localObjectDespawnMessage = new(
        "DespawnLocalObject", Imperium.Networking
    );

    private readonly ImpNetEvent objectsChangedEvent = new("ObjectsChanged", Imperium.Networking);

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

    internal ObjectManager(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected)
        : base(sceneLoaded, playersConnected)
    {
        FetchGlobalSpawnLists();
        FetchPlayers();

        RefreshLevelObjects();

        LogObjects();

        objectsChangedEvent.OnClientRecive += RefreshLevelObjects;
        burstSteamValve.OnClientRecive += OnSteamValveBurst;
        objectTeleportationRequest.OnClientRecive += OnObjectTeleportationRequestClient;

        localObjectDespawnMessage.OnClientRecive += OnDespawnLocalObject;
        localStaticPrefabSpawnMessage.OnClientRecive += OnSpawnLocalStaticPrefabClient;
        outsideObjectPrefabSpawnMessage.OnClientRecive += OnSpawnOutsideObjectClient;
        localObjectTeleportationRequest.OnClientRecive += OnLocalObjectTeleportationRequestClient;

        if (NetworkManager.Singleton.IsHost)
        {
            entitySpawnMessage.OnServerReceive += OnSpawnEntity;
            itemSpawnMessage.OnServerReceive += OnSpawnItem;
            mapHazardSpawnMessage.OnServerReceive += OnSpawnMapHazard;
            companyCruiserSpawnMessage.OnServerReceive += OnSpawnCompanyCruiser;
            staticPrefabSpawnMessage.OnServerReceive += OnSpawnStaticPrefabServer;

            entityDespawnMessage.OnServerReceive += OnDespawnEntity;
            itemDespawnMessage.OnServerReceive += OnDespawnItem;
            obstacleDespawnMessage.OnServerReceive += OnDespawnObstacle;

            objectTeleportationRequest.OnServerReceive += OnObjectTeleportationRequestServer;
        }
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
    internal void SpawnCompanyCruiser(CompanyCruiserSpawnRequest request)
    {
        companyCruiserSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnItem(ulong itemNetId) => itemDespawnMessage.DispatchToServer(itemNetId);

    [ImpAttributes.RemoteMethod]
    internal void DespawnEntity(ulong entityNetId) => entityDespawnMessage.DispatchToServer(entityNetId);

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
        localObjectTeleportationRequest.DispatchToClients(request);
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

    internal GameObject FindObject(string name)
    {
        if (ObjectCache.TryGetValue(name, out var v)) return v;
        var obj = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(
            obj => obj.name == name && obj.scene != SceneManager.GetSceneByName("HideAndDontSave"));
        if (!obj) return null;
        ObjectCache[name] = obj;
        return obj;
    }

    internal void ToggleObject(string name, bool isOn)
    {
        var obj = FindObject(name);
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
        var allScrap = allItems.Where(scrap => scrap.isScrap).ToHashSet();

        var allMapHazards = new Dictionary<string, GameObject>();
        var allStaticPrefabs = new Dictionary<string, GameObject>();
        var allLocalStaticPrefabs = new Dictionary<string, GameObject>();
        var allOutsideObjects = Resources.FindObjectsOfTypeAll<SpawnableOutsideObject>()
            .ToDictionary(obj => obj.prefabToSpawn.name);

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
                case "SteamValve":
                    allMapHazards["SteamValve"] = obj;
                    break;
                // Find all landmine containers (Not the actual mine objects which happen to have the same name)
                case "Landmine" when obj.transform.Find("Landmine"):
                    allMapHazards["Landmine"] = obj;
                    break;
                case "CompanyCruiser":
                    allStaticPrefabs["CompanyCruiser"] = obj;
                    break;
                case "CompanyCruiserManual":
                    allStaticPrefabs["CompanyCruiserManual"] = obj;
                    break;
                case "RagdollGrabbableObject":
                    allStaticPrefabs["Body"] = obj;
                    break;
                case "ClipboardManual":
                    allStaticPrefabs["Clipboard"] = obj;
                    break;
                case "StickyNoteItem":
                    allStaticPrefabs["StickyNote"] = obj;
                    break;
                case "MoldSpore":
                    allLocalStaticPrefabs["MoldSpore"] = obj;
                    break;
            }
        }

        LoadedItems.Set(allItems);
        LoadedScrap.Set(allScrap);
        LoadedEntities.Set(allEntities);
        LoadedMapHazards.Set(allMapHazards);
        LoadedStaticPrefabs.Set(allStaticPrefabs);
        LoadedOutsideObjects.Set(allOutsideObjects);
        LoadedLocalStaticPrefabs.Set(allLocalStaticPrefabs);

        GenerateDisplayNameMaps();
    }

    private static EnemyType CreateShiggyType(EnemyType type)
    {
        var shiggyType = Object.Instantiate(type);
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
        foreach (var obj in Object.FindObjectsOfType<EnemyAI>())
        {
            // Ignore objects that are hidden
            if (obj.gameObject.scene == SceneManager.GetSceneByName("HideAndDontSave")) continue;

            currentLevelEntities.Add(obj);
            CurrentLevelObjects[obj.GetComponent<NetworkObject>().NetworkObjectId] = obj.gameObject;
        }

        CurrentLevelEntities.Set(currentLevelEntities);
        CurrentLevelObjectsChanged.Trigger();
    }

    internal void RefreshLevelObjects()
    {
        HashSet<DoorLock> currentLevelDoors = [];
        HashSet<Turret> currentLevelTurrets = [];
        HashSet<EnemyVent> currentLevelVents = [];
        HashSet<EnemyAI> currentLevelEntities = [];
        HashSet<Landmine> currentLevelLandmines = [];
        HashSet<GrabbableObject> currentLevelItems = [];
        HashSet<GameObject> currentLevelVainShrouds = [];
        HashSet<BreakerBox> currentLevelBreakerBoxes = [];
        HashSet<SpikeRoofTrap> currentLevelSpikeTraps = [];
        HashSet<GameObject> currentLevelOutsideObjects = [];
        HashSet<SteamValveHazard> currentLevelSteamValves = [];
        HashSet<SandSpiderWebTrap> currentLevelSpiderWebs = [];
        HashSet<RandomScrapSpawn> currentScrapSpawnPoints = [];
        HashSet<VehicleController> currentLevelCompanyCruisers = [];
        HashSet<TerminalAccessibleObject> currentLevelSecurityDoors = [];

        foreach (var obj in Object.FindObjectsOfType<GameObject>())
        {
            // Ignore objects that are hidden
            if (obj.scene == SceneManager.GetSceneByName("HideAndDontSave")) continue;

            if (obj.name.Contains("MoldSpore") && currentLevelVainShrouds.Add(obj)) continue;
            if (OutsideObjectPrefabNameMap.Contains(obj.name) && currentLevelOutsideObjects.Add(obj))
            {
                continue;
            }

            foreach (var component in obj.GetComponents<Component>())
            {
                switch (component)
                {
                    case DoorLock doorLock when !currentLevelDoors.Contains(doorLock):
                        currentLevelDoors.Add(doorLock);
                        break;
                    case TerminalAccessibleObject securityDoor:
                        currentLevelSecurityDoors.Add(securityDoor);
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
                    case SteamValveHazard steamValve when !currentLevelSteamValves.Contains(steamValve):
                        currentLevelSteamValves.Add(steamValve);
                        break;
                    case SandSpiderWebTrap spiderWeb when !currentLevelSpiderWebs.Contains(spiderWeb):
                        currentLevelSpiderWebs.Add(spiderWeb);
                        break;
                    case RandomScrapSpawn scrapSpawn when !currentScrapSpawnPoints.Contains(scrapSpawn):
                        currentScrapSpawnPoints.Add(scrapSpawn);
                        break;
                    case VehicleController vehicleController when !currentLevelCompanyCruisers.Contains(vehicleController):
                        currentLevelCompanyCruisers.Add(vehicleController);
                        break;
                    case GrabbableObject item when !currentLevelItems.Contains(item):
                        currentLevelItems.Add(item);
                        break;
                    case EnemyAI entity when !currentLevelEntities.Contains(entity):
                        currentLevelEntities.Add(entity);
                        break;
                }
            }

            var networkObject = obj.GetComponent<NetworkObject>() ?? obj.GetComponentInChildren<NetworkObject>();
            if (networkObject) CurrentLevelObjects[networkObject.NetworkObjectId] = obj.gameObject;
        }

        CurrentLevelItems.Set(currentLevelItems);
        CurrentLevelEntities.Set(currentLevelEntities);
        CurrentLevelOutsideObjects.Set(currentLevelOutsideObjects);
        CurrentLevelDoors.Set(currentLevelDoors);
        CurrentLevelSecurityDoors.Set(currentLevelSecurityDoors);
        CurrentLevelTurrets.Set(currentLevelTurrets);
        CurrentLevelLandmines.Set(currentLevelLandmines);
        CurrentLevelSpikeTraps.Set(currentLevelSpikeTraps);
        CurrentLevelBreakerBoxes.Set(currentLevelBreakerBoxes);
        CurrentLevelVents.Set(currentLevelVents);
        CurrentLevelSteamValves.Set(currentLevelSteamValves);
        CurrentLevelSpiderWebs.Set(currentLevelSpiderWebs);
        CurrentScrapSpawnPoints.Set(currentScrapSpawnPoints);
        CurrentLevelCruisers.Set(currentLevelCompanyCruisers);
        CurrentLevelVainShrouds.Set(currentLevelVainShrouds);

        CurrentLevelObjectsChanged.Trigger();
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
        overrideDisplayNameMap["treeLeaflessBrown.001 Variant"] = "Tree Leafless 1";
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
        var actualSpawnPosition = hasGround
            ? groundInfo.point
            : clientId.GetPlayerController()!.transform.position;

        for (var i = 0; i < request.Amount; i++)
        {
            var entityObj = request.Name switch
            {
                "Shiggy" => InstantiateShiggy(spawningEntity, actualSpawnPosition),
                _ => Object.Instantiate(
                    enemyPrefab,
                    actualSpawnPosition,
                    Quaternion.identity
                )
            };

            if (request.Health > 0) entityObj.GetComponent<EnemyAI>().enemyHP = request.Health;

            var netObject = entityObj.gameObject.GetComponentInChildren<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);
            CurrentLevelObjects[netObject.NetworkObjectId] = entityObj;

            // Checked if spawned entity is a masked and the masked parameters are set
            if (
                entityObj.TryGetComponent<MaskedPlayerEnemy>(out var maskedEntity)
                && request is { MaskedPlayerId: > -1, MaskedName: not null }
            )
            {
                AssignMaskedToPlayer(maskedEntity, (ulong)request.MaskedPlayerId, request.MaskedName);
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
        var shiggyPrefab = Object.Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
        shiggyPrefab.name = "ShiggyEntity";
        Object.Destroy(shiggyPrefab.GetComponent<TestEnemy>());
        Object.Destroy(shiggyPrefab.GetComponent<HDAdditionalLightData>());
        Object.Destroy(shiggyPrefab.GetComponent<Light>());
        Object.Destroy(shiggyPrefab.GetComponent<AudioSource>());
        foreach (var componentsInChild in shiggyPrefab.GetComponentsInChildren<BoxCollider>())
        {
            Object.Destroy(componentsInChild);
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
    private static void DespawnLocalObject(LocalObjectType type, Vector3 position, GameObject obj)
    {
        if (!obj)
        {
            Imperium.IO.LogError(
                $"[SPAWN] [R] Failed to despawn local object of type '{type}' at {Formatting.FormatVector(position)}."
            );
            return;
        }

        Object.Destroy(obj);
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
            var itemObj = Object.Instantiate(
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
            CurrentLevelObjects[netObject.NetworkObjectId] = itemObj;

            // If player has free slot, place it in hand, otherwise leave it on the ground and play sound
            var spawnedInInventory = false;
            if (request.SpawnInInventory)
            {
                var invokingPlayer = Imperium.StartOfRound.allPlayerScripts[clientId];
                var firstItemSlot = Reflection.Invoke<PlayerControllerB, int>(invokingPlayer, "FirstEmptyItemSlot");
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
        for (var i = 0; i < request.Amount; i++)
        {
            switch (request.Name)
            {
                case "Turret":
                    SpawnTurret(request.SpawnPosition);
                    break;
                case "Spike Trap":
                    SpawnSpikeTrap(request.SpawnPosition);
                    break;
                case "Landmine":
                    SpawnLandmine(request.SpawnPosition);
                    break;
                case "SteamValve":
                    SpawnSteamValve(request.SpawnPosition);
                    break;
                default:
                    Imperium.IO.LogError($"[SPAWN] [R] Failed to spawn map hazard {request.Name}");
                    return;
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

    [ImpAttributes.LocalMethod]
    private void OnSpawnOutsideObjectClient(StaticPrefabSpawnRequest request)
    {
        if (!LoadedOutsideObjects.Value.TryGetValue(request.Name, out var outsideObject))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find outside object '{request.Name}'.");
            return;
        }

        for (var i = 0; i < request.Amount; i++)
        {
            Object.Instantiate(
                outsideObject.prefabToSpawn, request.SpawnPosition, Quaternion.Euler(outsideObject.rotationOffset)
            );
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

        for (var i = 0; i < request.Amount; i++)
        {
            Object.Instantiate(staticPrefab, request.SpawnPosition, rotationOffset);
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
    private void OnSpawnCompanyCruiser(CompanyCruiserSpawnRequest request, ulong clientId)
    {
        // Raycast to find the ground to spawn the entity on
        var hasGround = Physics.Raycast(
            new Ray(request.SpawnPosition + Vector3.up * 2f, Vector3.down),
            out var groundInfo, 100, ImpConstants.IndicatorMask
        );
        var actualSpawnPosition = hasGround
            ? groundInfo.point
            : clientId.GetPlayerController()!.transform.position;

        var cruiserObj = Object.Instantiate(
            LoadedStaticPrefabs.Value["CompanyCruiser"],
            actualSpawnPosition + Vector3.up * 2.5f,
            Quaternion.identity,
            RoundManager.Instance.VehiclesContainer
        );

        var vehicleNetObject = cruiserObj.gameObject.GetComponentInChildren<NetworkObject>();
        vehicleNetObject.Spawn();
        CurrentLevelObjects[vehicleNetObject.NetworkObjectId] = cruiserObj;

        var cruiserManualObj = Object.Instantiate(
            LoadedStaticPrefabs.Value["CompanyCruiserManual"],
            actualSpawnPosition + Vector3.up * 2.5f,
            Quaternion.identity,
            RoundManager.Instance.VehiclesContainer
        );
        var manualNetObject = cruiserManualObj.gameObject.GetComponentInChildren<NetworkObject>();
        manualNetObject.Spawn();
        CurrentLevelObjects[manualNetObject.NetworkObjectId] = cruiserObj;

        if (request.SendNotification)
        {
            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = "A trusty Company Cruiser has been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
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
            var staticObj = Object.Instantiate(staticPrefab, request.SpawnPosition, Quaternion.Euler(Vector3.zero));

            var netObject = staticObj.gameObject.GetComponent<NetworkObject>();
            netObject.Spawn(destroyWithScene: true);

            CurrentLevelObjects[netObject.NetworkObjectId] = staticObj;
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
    private void SpawnLandmine(Vector3 position)
    {
        var hazardObj = Object.Instantiate(
            LoadedMapHazards.Value["Landmine"], position, Quaternion.Euler(Vector3.zero)
        );
        hazardObj.transform.Find("Landmine").rotation = Quaternion.Euler(270, 0, 0);
        hazardObj.transform.localScale = new Vector3(0.4574f, 0.4574f, 0.4574f);

        var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        netObject.Spawn(destroyWithScene: true);
        CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void SpawnTurret(Vector3 position)
    {
        var hazardObj = Object.Instantiate(LoadedMapHazards.Value["Turret"], position, Quaternion.Euler(Vector3.zero));

        var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        netObject.Spawn(destroyWithScene: true);
        CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void SpawnSteamValve(Vector3 position)
    {
        var hazardObj =
            Object.Instantiate(LoadedMapHazards.Value["SteamValve"], position, Quaternion.Euler(Vector3.zero));

        var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        netObject.Spawn(destroyWithScene: true);
        CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void SpawnSpikeTrap(Vector3 position)
    {
        var hazardObj = Object.Instantiate(
            LoadedMapHazards.Value["Spike Trap"],
            position,
            Quaternion.Euler(Vector3.zero)
        );

        var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        netObject.Spawn(destroyWithScene: true);
        CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
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
            if (item.transform.parent != null)
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
    private void OnLocalObjectTeleportationRequestClient(LocalObjectTeleportRequest request)
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
                Imperium.IO.LogError($"[NET] Teleportation request has invalid outside object type '{request.Type}'");
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

        DespawnObject(obj, clientId);
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnEntity(ulong entityNetId, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(entityNetId, out var obj))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn entity with net ID {entityNetId}");
            return;
        }

        DespawnObject(obj, clientId);
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnObstacle(ulong obstacleNetId, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(obstacleNetId, out var obj))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn obstacle with net ID {obstacleNetId}");
            return;
        }

        DespawnObject(obj, clientId);
    }

    [ImpAttributes.LocalMethod]
    private void OnDespawnLocalObject(LocalObjectDespawnRequest request)
    {
        switch (request.Type)
        {
            case LocalObjectType.VainShroud:
                DespawnLocalObject(request.Type, request.Position, CurrentLevelVainShrouds.Value.FirstOrDefault(
                    obj => obj.transform.position == request.Position
                ));
                break;
            case LocalObjectType.OutsideObject:
                DespawnLocalObject(request.Type, request.Position, CurrentLevelOutsideObjects.Value.FirstOrDefault(
                    obj => obj.transform.position == request.Position
                ));
                break;
            default:
                Imperium.IO.LogError($"[NET] Despawn request has invalid outside object type '{request.Type}'");
                break;
        }
    }

    [ImpAttributes.HostOnly]
    private void DespawnObject(GameObject gameObject, ulong clientId)
    {
        if (!gameObject) return;

        if (gameObject.TryGetComponent<GrabbableObject>(out var grabbableObject))
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
        else if (gameObject.TryGetComponent<VehicleController>(out var companyCruiser))
        {
            if (companyCruiser.currentPassenger)
            {
                companyCruiser.currentPassenger.transform.SetParent(Imperium.StartOfRound.playersContainer);
                Imperium.PlayerManager.TeleportPlayer(new TeleportPlayerRequest
                {
                    PlayerId = companyCruiser.currentPassenger.playerClientId,
                    Destination = companyCruiser.currentPassenger.transform.position
                });
            }

            if (companyCruiser.currentDriver)
            {
                companyCruiser.currentDriver.transform.SetParent(Imperium.StartOfRound.playersContainer);
                Imperium.PlayerManager.TeleportPlayer(new TeleportPlayerRequest
                {
                    PlayerId = companyCruiser.currentDriver.playerClientId,
                    Destination = companyCruiser.currentDriver.transform.position
                });
            }
        }
        else if (gameObject.TryGetComponent<SandSpiderAI>(out var sandSpider))
        {
            for (var i = 0; i < sandSpider.webTraps.Count; i++)
            {
                sandSpider.BreakWebServerRpc(i, (int)clientId);
            }
        }

        try
        {
            if (gameObject.TryGetComponent<NetworkObject>(out var networkObject)) networkObject.Despawn();
        }
        finally
        {
            Object.Destroy(gameObject);
            objectsChangedEvent.DispatchToClients();
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

    #endregion
}
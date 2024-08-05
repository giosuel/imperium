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

#endregion

namespace Imperium.Core.Lifecycle;

internal class ObjectManager : ImpLifecycleObject
{
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
    internal readonly ImpBinding<IReadOnlyDictionary<string, GameObject>> LoadedStaticPrefabs = new();

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
    internal readonly ImpBinding<IReadOnlyCollection<GameObject>> CurrentLevelMoldSpores = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<BreakerBox>> CurrentLevelBreakerBoxes = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SpikeRoofTrap>> CurrentLevelSpikeTraps = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SteamValveHazard>> CurrentLevelSteamValves = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<SandSpiderWebTrap>> CurrentLevelSpiderWebs = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<VehicleController>> CurrentLevelCompanyCruisers = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<TerminalAccessibleObject>> CurrentLevelSecurityDoors = new([]);

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

    // Used to look up the unique identifier of a spawned static prefab
    internal readonly Dictionary<GameObject, ulong> StaticPrefabLookupMap = [];

    private readonly Dictionary<string, string> displayNameMap = [];

    private readonly ImpNetMessage<EntitySpawnRequest> entitySpawnMessage = new("SpawnEntity", Imperium.Networking);
    private readonly ImpNetMessage<ItemSpawnRequest> itemSpawnMessage = new("SpawnItem", Imperium.Networking);

    private readonly ImpNetMessage<MapHazardSpawnRequest> mapHazardSpawnMessage = new(
        "MapHazardSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<StaticPrefabSpawnRequest> staticPrefabSpawnMessage = new(
        "StaticPrefabSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<CompanyCruiserSpawnRequest> companyCruiserSpawnMessage = new(
        "CompanyCruiserSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<ObjectTeleportRequest> objectTeleportationRequest = new(
        "ObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<ulong> burstSteamValve = new("BurstSteamValve", Imperium.Networking);
    private readonly ImpNetMessage<ulong> entityDespawnMessage = new("DespawnEntity", Imperium.Networking);
    private readonly ImpNetMessage<ulong> itemDespawnMessage = new("DespawnItem", Imperium.Networking);
    private readonly ImpNetMessage<ulong> obstacleDespawnMessage = new("DespawnObstacle", Imperium.Networking);

    private readonly ImpNetEvent entitiesChanged = new("EntitiesChanged", Imperium.Networking);
    private readonly ImpNetEvent itemsChanged = new("ItemsChanged", Imperium.Networking);
    private readonly ImpNetEvent obstaclesChanged = new("ObstaclesChanged", Imperium.Networking);

    internal ObjectManager(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected)
        : base(sceneLoaded, playersConnected)
    {
        FetchGlobalSpawnLists();
        FetchPlayers();

        RefreshLevelItems();
        RefreshLevelObstacles();

        LogObjects();

        entitiesChanged.OnClientRecive += RefreshLevelEntities;
        itemsChanged.OnClientRecive += RefreshLevelItems;
        obstaclesChanged.OnClientRecive += RefreshLevelObstacles;
        burstSteamValve.OnClientRecive += OnSteamValveBurst;
        staticPrefabSpawnMessage.OnClientRecive += OnStaticPrefabSpawnClient;
        objectTeleportationRequest.OnClientRecive += OnObjectTeleportationRequestClient;

        if (NetworkManager.Singleton.IsHost)
        {
            entitySpawnMessage.OnServerReceive += OnSpawnEntity;
            itemSpawnMessage.OnServerReceive += OnSpawnItem;
            mapHazardSpawnMessage.OnServerReceive += OnSpawnMapHazard;
            staticPrefabSpawnMessage.OnServerReceive += OnStaticPrefabSpawnServer;
            companyCruiserSpawnMessage.OnServerReceive += OnSpawnCompanyCruiser;

            entityDespawnMessage.OnServerReceive += OnDespawnEntity;
            itemDespawnMessage.OnServerReceive += OnDespawnItem;
            obstacleDespawnMessage.OnServerReceive += OnDespawnObstacle;

            objectTeleportationRequest.OnServerReceive += OnObjectTeleportationRequestServer;
        }
    }

    protected override void OnSceneLoad()
    {
        RefreshLevelItems();
        RefreshLevelObstacles();

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
    internal void SpawnStaticPrefab(StaticPrefabSpawnRequest request) => staticPrefabSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawmCompanyCruiser(CompanyCruiserSpawnRequest request)
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
    internal void TeleportObject(ObjectTeleportRequest request) => objectTeleportationRequest.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void InvokeEntitiesChanged() => entitiesChanged.DispatchToClients();

    [ImpAttributes.RemoteMethod]
    internal void InvokeItemsChanged() => itemsChanged.DispatchToClients();

    [ImpAttributes.RemoteMethod]
    internal void InvokeObstaclesChanged() => obstaclesChanged.DispatchToClients();

    [ImpAttributes.RemoteMethod]
    internal void BurstSteamValve(ulong valveNetId) => burstSteamValve.DispatchToClients(valveNetId);

    internal string GetDisplayName(string inGameName) => displayNameMap.GetValueOrDefault(inGameName, inGameName);

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

    internal static void TeleportItem(GrabbableObject item, Vector3 position)
    {
        var itemTransform = item.transform;
        itemTransform.position = position + Vector3.up;
        item.startFallingPosition = itemTransform.position;
        if (item.transform.parent != null)
        {
            item.startFallingPosition = item.transform.parent.InverseTransformPoint(item.startFallingPosition);
        }

        item.FallToGround();
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
            .Where(item => !ImpConstants.ItemBlacklist.Contains(item.itemName))
            .ToHashSet();

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
                case "SteamValve":
                    allMapHazards["SteamValve"] = obj;
                    break;
                // Find all landmine containers (Not the actual mine objects which happen to have the same name)
                case "Landmine" when obj.transform.Find("Landmine"):
                    allMapHazards["Landmine"] = obj;
                    break;
                case "ClipboardManual":
                    allStaticPrefabs["clipboard"] = obj;
                    break;
                case "StickyNoteItem":
                    allStaticPrefabs["Sticky note"] = obj;
                    break;
                case "CompanyCruiser":
                    allStaticPrefabs["Company Cruiser"] = obj;
                    break;
            }
        }

        allStaticPrefabs["Body"] = Imperium.StartOfRound.ragdollGrabbableObjectPrefab;
        allStaticPrefabs["Mold"] = Object.FindObjectOfType<MoldSpreadManager>().moldPrefab;

        var allScrap = allItems.Where(scrap => scrap.isScrap).ToHashSet();

        LoadedItems.Set(allItems);
        LoadedScrap.Set(allScrap);
        LoadedEntities.Set(allEntities);
        LoadedMapHazards.Set(allMapHazards);
        LoadedStaticPrefabs.Set(allStaticPrefabs);

        GenerateDisplayNameMap();
    }

    private static EnemyType CreateShiggyType(EnemyType type)
    {
        var shiggyType = Object.Instantiate(type);
        shiggyType.enemyName = "Shiggy";

        return shiggyType;
    }

    internal string GetStaticPrefabName(string objectName)
    {
        return LoadedStaticPrefabs.Value.TryGetValue(objectName, out var prefab) ? prefab.name : objectName;
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
        HashSet<TerminalAccessibleObject> currentLevelSecurityDoors = [];
        HashSet<Turret> currentLevelTurrets = [];
        HashSet<Landmine> currentLevelLandmines = [];
        HashSet<SpikeRoofTrap> currentLevelSpikeTraps = [];
        HashSet<BreakerBox> currentLevelBreakerBoxes = [];
        HashSet<EnemyVent> currentLevelVents = [];
        HashSet<SteamValveHazard> currentLevelSteamValves = [];
        HashSet<SandSpiderWebTrap> currentLevelSpiderWebs = [];
        HashSet<RandomScrapSpawn> currentScrapSpawnPoints = [];
        HashSet<GameObject> currentMoldSpores = [];
        HashSet<VehicleController> currentLevelCompanyCruisers = [];

        foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            // Ignore objects that are hidden
            if (obj.scene == SceneManager.GetSceneByName("HideAndDontSave")) continue;
            if (obj.name.Contains("MoldSpore"))
            {
                currentMoldSpores.Add(obj);
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
                    case RandomScrapSpawn scrapSpawn:
                        currentScrapSpawnPoints.Add(scrapSpawn);
                        break;
                    case VehicleController vehicleController:
                        currentLevelCompanyCruisers.Add(vehicleController);
                        break;
                }
            }

            var networkObject = obj.GetComponent<NetworkObject>() ?? obj.GetComponentInChildren<NetworkObject>();
            if (networkObject) CurrentLevelObjects[networkObject.NetworkObjectId] = obj.gameObject;
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

        if (currentLevelVents.Count > 0)
        {
            CurrentLevelVents.Set(CurrentLevelVents.Value.Union(currentLevelVents).ToHashSet());
        }

        if (currentLevelSteamValves.Count > 0)
        {
            CurrentLevelSteamValves.Set(CurrentLevelSteamValves.Value.Union(currentLevelSteamValves).ToHashSet());
        }

        if (currentLevelSpiderWebs.Count > 0)
        {
            CurrentLevelSpiderWebs.Set(CurrentLevelSpiderWebs.Value.Union(currentLevelSpiderWebs).ToHashSet());
        }

        if (currentScrapSpawnPoints.Count > 0)
        {
            CurrentScrapSpawnPoints.Set(CurrentScrapSpawnPoints.Value.Union(currentScrapSpawnPoints).ToHashSet());
        }

        if (currentLevelCompanyCruisers.Count > 0)
        {
            CurrentLevelCompanyCruisers.Set(
                CurrentLevelCompanyCruisers.Value.Union(currentLevelCompanyCruisers).ToHashSet());
        }

        if (currentMoldSpores.Count > 0)
        {
            CurrentLevelMoldSpores.Set(CurrentLevelMoldSpores.Value.Union(currentMoldSpores).ToHashSet());
        }
    }

    private void GenerateDisplayNameMap()
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
        var spawningEntity = LoadedEntities.Value
            .FirstOrDefault(entity => entity.enemyName == request.Name
                                      && entity.enemyPrefab.name == request.PrefabName);
        if (!spawningEntity)
        {
            Imperium.IO.LogError($"[SPAWN] Entity {request.Name} not found!");
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
                    spawningEntity.enemyPrefab,
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

        entitiesChanged.DispatchToClients();
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

    [ImpAttributes.HostOnly]
    private void OnSpawnItem(ItemSpawnRequest request, ulong clientId)
    {
        var spawningItem = LoadedItems.Value
            .FirstOrDefault(item => item.itemName == request.Name && item.spawnPrefab?.name == request.PrefabName);
        var itemPrefab = spawningItem?.spawnPrefab ?? LoadedStaticPrefabs.Value[request.Name];

        if (!itemPrefab || !itemPrefab.GetComponent<GrabbableObject>())
        {
            Imperium.IO.LogError($"[SPAWN] Item {request.Name} not found!");
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
            if (request.SpawnInInventory)
            {
                var invokingPlayer = Imperium.StartOfRound.allPlayerScripts[clientId];
                var firstItemSlot = Reflection.Invoke<PlayerControllerB, int>(invokingPlayer, "FirstEmptyItemSlot");
                if (firstItemSlot != -1 && grabbableItem.grabbable)
                {
                    grabbableItem.InteractItem();
                    PlayerManager.GrabObject(grabbableItem, invokingPlayer);
                }
                else if (grabbableItem.itemProperties.dropSFX)
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

        itemsChanged.DispatchToClients();
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
                case "SpiderWeb":
                    Imperium.IO.LogError("[IMPL] Spider web spawning not implemented yet");
                    break;
                default:
                    Imperium.IO.LogError($"[SPAWN] Failed to spawn map hazard {request.Name}");
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

        obstaclesChanged.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnStaticPrefabSpawnServer(StaticPrefabSpawnRequest request, ulong clientId)
    {
        staticPrefabSpawnMessage.DispatchToClients(new StaticPrefabSpawnRequest
        {
            Name = request.Name,
            Amount = request.Amount,
            SpawnPosition = request.SpawnPosition,
            SendNotification = request.SendNotification,
            UniqueIdentifier = (ulong)Random.Range(111111, 999999)
        });
    }

    [ImpAttributes.LocalMethod]
    private void OnStaticPrefabSpawnClient(StaticPrefabSpawnRequest request)
    {
        for (var i = 0; i < request.Amount; i++)
        {
            switch (request.Name)
            {
                case "Mold":
                    CurrentLevelObjects[request.UniqueIdentifier] = Object.Instantiate(
                        LoadedStaticPrefabs.Value["Mold"], request.SpawnPosition, Quaternion.Euler(Vector3.zero)
                    );
                    StaticPrefabLookupMap[CurrentLevelObjects[request.UniqueIdentifier]] = request.UniqueIdentifier;
                    break;
                default:
                    Imperium.IO.LogError($"[SPAWN] Failed to spawn map hazard {request.Name}");
                    return;
            }
        }

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        var verbString = request.Amount == 1 ? "has" : "have";

        if (request.SendNotification)
        {
            Imperium.IO.Send(
                $"{mountString} {request.Name} {verbString} been spawned!",
                type: NotificationType.Spawning
            );
        }

        RefreshLevelObstacles();
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
            LoadedStaticPrefabs.Value["Company Cruiser"],
            actualSpawnPosition + Vector3.up * 2.5f,
            Quaternion.identity,
            RoundManager.Instance.VehiclesContainer
        );
        var vehicleController = cruiserObj.GetComponent<VehicleController>();
        vehicleController.mainRigidbody.MovePosition(actualSpawnPosition);
        vehicleController.hasBeenSpawned = true;

        var netObject = cruiserObj.gameObject.GetComponentInChildren<NetworkObject>();
        netObject.Spawn(destroyWithScene: true);
        CurrentLevelObjects[netObject.NetworkObjectId] = cruiserObj;

        if (request.SendNotification)
        {
            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = "A trusty Company Cruiser has been spawned!",
                Type = NotificationType.Spawning
            });
        }

        obstaclesChanged.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void SpawnLandmine(Vector3 position)
    {
        var hazardObj =
            Object.Instantiate(LoadedMapHazards.Value["Landmine"], position, Quaternion.Euler(Vector3.zero));
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
            Imperium.IO.LogError($"Failed to teleport object item with net ID {request.NetworkId}");
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

    [ImpAttributes.HostOnly]
    private void OnDespawnItem(ulong itemNetId, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(itemNetId, out var obj))
        {
            Imperium.IO.LogError($"Failed to despawn item with net ID {itemNetId}");
            return;
        }

        DespawnObject(obj);
        entitiesChanged.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnEntity(ulong entityNetId, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(entityNetId, out var obj))
        {
            Imperium.IO.LogError($"Failed to despawn entity with net ID {entityNetId}");
            return;
        }

        DespawnObject(obj);
        entitiesChanged.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnObstacle(ulong obstacleNetId, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(obstacleNetId, out var obj))
        {
            Imperium.IO.LogError($"Failed to despawn obstacle with net ID {obstacleNetId}");
            return;
        }

        DespawnObject(obj);
        obstaclesChanged.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private static void DespawnObject(GameObject gameObject)
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

        try
        {
            if (gameObject.TryGetComponent<NetworkObject>(out var networkObject)) networkObject.Despawn();
        }
        finally
        {
            Object.Destroy(gameObject);
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
#region

using System.Collections.Generic;
using System.Linq;
using DunGen;
using Imperium.Interface.ComponentManager;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.LevelEditor;

internal class ImpLevelEditor : MonoBehaviour
{
    private readonly List<DoorMarker> registeredMarkers = [];
    private readonly List<Vector3> markerPositions = [];

    private readonly ImpBinding<List<Tile>> Tiles = new([]);
    private readonly ImpBinding<List<Blocker>> Blockers = new([]);
    private readonly ImpBinding<List<Connector>> Connectors = new([]);

    private PreviewTile selectedTile;
    private PreviewBlocker selectedBlocker;
    private PreviewConnector selectedConnector;

    private bool previewValid;
    private float timeSinceCycling;

    internal static ImpLevelEditor Create() => new GameObject("Imp_LevelEditor").AddComponent<ImpLevelEditor>();

    private ComponentManager componentManager;

    private readonly PlacedDungeon dungeon = new();
    private PlacedTile currentTile;

    private BuildingTool buildingTool;

    private void Awake()
    {

        Imperium.IsSceneLoaded.onUpdate += OnSceneChange;

        Imperium.IngamePlayerSettings.playerInput.actions["ActivateItem"].performed += OnLeftClick;
        Imperium.IngamePlayerSettings.playerInput.actions["PingScan"].performed += OnRightClick;

        componentManager = Imperium.Interface.Get<ComponentManager>();
        componentManager.InitUI(Imperium.Interface.Theme);
        componentManager.BindUI(PickTile, PickBlocker, PickConnector, Tiles, Blockers, Connectors);

        var registeredBlockers = new HashSet<GameObject>();
        var registeredConnectors = new HashSet<GameObject>();

        foreach (var tileSet in Resources.FindObjectsOfTypeAll<TileSet>())
        {
            foreach (var tile in tileSet.TileWeights.Weights)
            {
                var doorways = tile.Value.GetComponentsInChildren<Doorway>();
                var aiNodesCount = tile.Value.GetComponentsInChildren<Transform>()
                    .Count(comp => comp.gameObject.CompareTag("AINode"));
                var scrapSpawnsCount = tile.Value.GetComponentsInChildren<RandomScrapSpawn>().Length;

                Tiles.Value.Add(new Tile
                {
                    Name = tile.Value.name,
                    Prefab = tile.Value,
                    OriginalRotation = tile.Value.transform.rotation,
                    Doorways = doorways,
                    DoorwayOrigins = doorways.Select(doorway => doorway.transform.localPosition).ToArray(),
                    DoorwayYRotations = doorways.Select(doorway => doorway.transform.rotation.eulerAngles.y).ToArray(),
                    AINodesCount = aiNodesCount,
                    ScrapSpawns = scrapSpawnsCount
                });

                foreach (var doorway in doorways)
                {
                    foreach (var blockerWeight in doorway.BlockerPrefabWeights)
                    {
                        if (!registeredBlockers.Contains(blockerWeight.GameObject))
                        {
                            Blockers.Value.Add(new Blocker
                            {
                                Name = blockerWeight.GameObject.name,
                                Prefab = blockerWeight.GameObject,
                                Socket = doorway.Socket,
                                OriginalRotation = blockerWeight.GameObject.transform.rotation
                            });
                            registeredBlockers.Add(blockerWeight.GameObject);
                        }
                    }

                    foreach (var connectorWeight in doorway.ConnectorPrefabWeights)
                    {
                        if (!registeredConnectors.Contains(connectorWeight.GameObject))
                        {
                            Connectors.Value.Add(new Connector
                            {
                                Name = connectorWeight.GameObject.name,
                                Prefab = connectorWeight.GameObject,
                                Socket = doorway.Socket,
                                OriginalRotation = connectorWeight.GameObject.transform.rotation
                            });
                            registeredConnectors.Add(connectorWeight.GameObject);
                        }
                    }
                }
            }
        }

        buildingTool = gameObject.AddComponent<BuildingTool>();
        buildingTool.Init(dungeon);

        Tiles.Refresh();
        Blockers.Refresh();
        Connectors.Refresh();
    }

    private void OnMouseCycle(bool forward)
    {
        if (selectedTile == null) return;

        var previousIndex = selectedTile.CurrentDoorIndex;

        var tempIndex = previousIndex;

        if (forward)
        {
            do
            {
                tempIndex = (tempIndex + 1) % selectedTile.Tile.DoorwayOrigins.Length;

                if (selectedTile.Tile.Doorways[tempIndex].socket.IsCompatible(currentPreviewMarker.Socket))
                {
                    selectedTile.CurrentDoorIndex = tempIndex;
                    break;
                }
            } while (tempIndex != previousIndex);
        }
        else
        {
            do
            {
                if (tempIndex == 0)
                {
                    tempIndex = selectedTile.Tile.DoorwayOrigins.Length - 1;
                }
                else
                {
                    tempIndex--;
                }

                if (selectedTile.Tile.Doorways[tempIndex].socket.IsCompatible(currentPreviewMarker.Socket))
                {
                    selectedTile.CurrentDoorIndex = tempIndex;
                    break;
                }
            } while (tempIndex != previousIndex);
        }

        if (highlightedMarker)
        {
            var doorPivot = highlightedMarker.DoorwayOrigin;
            selectedTile.PreviewPrefab.transform.position =
                doorPivot - selectedTile.Tile.DoorwayOrigins[selectedTile.CurrentDoorIndex];
            selectedTile.PreviewPrefab.transform.rotation = selectedTile.Tile.OriginalRotation;
            selectedTile.PreviewPrefab.transform.RotateAround(
                doorPivot,
                Vector3.up,
                highlightedMarker.DoorwayRotation.eulerAngles.y - 180 -
                selectedTile.Tile.DoorwayYRotations[selectedTile.CurrentDoorIndex]
            );
        }
    }

    private void OnSceneChange(bool isLoaded) => RegisterDoors();

    private void OnLeftClick(InputAction.CallbackContext _)
    {
        if (!highlightedMarker ||
            Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.ImpPositionIndicator.IsActive ||
            Imperium.ImpTapeMeasure.IsActive ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat ||
            Imperium.ShipBuildModeManager.InBuildMode) return;

        if (!highlightedMarker) return;

        if (selectedTile != null)
        {
            if (!previewValid) return;

            dungeon.Tiles.Add(GeneratePlacedTile(selectedTile));

            Imperium.IO.LogInfo(
                $"Placed tile {selectedTile.Tile.Name} at ({Formatting.FormatVector(selectedTile.PreviewPrefab.transform.position)}), rotation: {Formatting.FormatVector(selectedTile.PreviewPrefab.transform.rotation.eulerAngles)}");

            selectedTile.PreviewPrefab.SetActive(false);
            selectedTile = null;

            highlightedMarker.IsConnected = true;
            highlightedMarker.DisableCollider();

            RegisterDoors();
        }
        else if (selectedConnector != null)
        {
            if (!previewValid) return;

            var newConnector = Instantiate(selectedConnector.Connector.Prefab);
            newConnector.transform.position = selectedConnector.PreviewPrefab.transform.position;
            newConnector.transform.rotation = selectedConnector.PreviewPrefab.transform.rotation;
            Utils.SpawnNetworkChildren(newConnector);

            Imperium.IO.LogInfo(
                $"Placed connector {selectedConnector.Connector.Name} at ({Formatting.FormatVector(selectedConnector.PreviewPrefab.transform.position)}), rotation: {Formatting.FormatVector(selectedConnector.PreviewPrefab.transform.rotation.eulerAngles)}");

            selectedConnector.PreviewPrefab.SetActive(false);
            selectedConnector = null;

            highlightedMarker.HasDoorway = true;
            highlightedMarker.Disable();
        }
        else if (selectedBlocker != null)
        {
            if (!previewValid) return;

            var newBlocker = Instantiate(selectedBlocker.Blocker.Prefab);
            newBlocker.transform.position = selectedBlocker.PreviewPrefab.transform.position;
            newBlocker.transform.rotation = selectedBlocker.PreviewPrefab.transform.rotation;
            Utils.SpawnNetworkChildren(newBlocker);

            Imperium.IO.LogInfo(
                $"Placed blocker {selectedBlocker.Blocker.Name} at ({Formatting.FormatVector(selectedBlocker.PreviewPrefab.transform.position)}), rotation: {Formatting.FormatVector(selectedBlocker.PreviewPrefab.transform.rotation.eulerAngles)}");

            selectedBlocker.PreviewPrefab.SetActive(false);
            selectedBlocker = null;

            highlightedMarker.IsConnected = true;
            highlightedMarker.HasDoorway = true;
            highlightedMarker.Disable();
        }
        else if (!highlightedMarker.IsConnected)
        {
            componentManager.OpenForPrimary(highlightedMarker.Socket);
        }
        else if (!highlightedMarker.HasDoorway)
        {
            componentManager.OpenForSecondary(highlightedMarker.Socket);
        }
    }

    private void OnRightClick(InputAction.CallbackContext _)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat ||
            Imperium.ShipBuildModeManager.InBuildMode) return;

        selectedTile?.PreviewPrefab.SetActive(false);
        selectedBlocker?.PreviewPrefab.SetActive(false);
        selectedConnector?.PreviewPrefab.SetActive(false);

        selectedTile = null;
        selectedBlocker = null;
        selectedConnector = null;
    }

    private readonly Dictionary<string, GameObject> prefabPreviewCache = [];
    private readonly Dictionary<string, List<MeshRenderer>> prefabPreviewMeshRenderers = [];

    private void PickBlocker(Blocker blocker)
    {
        if (!prefabPreviewCache.TryGetValue(blocker.Name, out var previewPrefab))
        {
            previewPrefab = Instantiate(blocker.Prefab);
            prefabPreviewCache[blocker.Name] = previewPrefab;
        }

        previewPrefab = CreatePreviewPrefab(previewPrefab);

        selectedTile = null;
        selectedConnector = null;

        selectedBlocker = new PreviewBlocker
        {
            Blocker = blocker,
            PreviewPrefab = previewPrefab
        };

        componentPickedThisFrame = true;
        componentManager.Close();
    }

    private void PickConnector(Connector connector)
    {
        if (!prefabPreviewCache.TryGetValue(connector.Name, out var previewPrefab))
        {
            previewPrefab = Instantiate(connector.Prefab);
            prefabPreviewCache[connector.Name] = previewPrefab;
        }

        previewPrefab = CreatePreviewPrefab(previewPrefab);

        selectedTile = null;
        selectedBlocker = null;

        selectedConnector = new PreviewConnector
        {
            Connector = connector,
            PreviewPrefab = previewPrefab
        };

        componentPickedThisFrame = true;
        componentManager.Close();
    }

    private void PickTile(Tile tile)
    {
        if (!prefabPreviewCache.TryGetValue(tile.Name, out var previewPrefab))
        {
            previewPrefab = Instantiate(tile.Prefab);
            prefabPreviewCache[tile.Name] = previewPrefab;
        }

        previewPrefab = CreatePreviewPrefab(previewPrefab);

        selectedBlocker = null;
        selectedConnector = null;

        selectedTile = new PreviewTile
        {
            Tile = tile,
            PreviewPrefab = previewPrefab,
            CurrentDoorIndex = 0
        };

        componentPickedThisFrame = true;
        componentManager.Close();

        // Cycle to get the first valid door
        OnMouseCycle(true);
    }

    private void SetPreviewOkay(GameObject obj) => SetObjectPreviewMaterial(obj, ImpAssets.HologramOkay);
    private void SetPreviewError(GameObject obj) => SetObjectPreviewMaterial(obj, ImpAssets.HologramError);

    private void SetObjectPreviewMaterial(GameObject obj, Material material)
    {
        if (prefabPreviewMeshRenderers.TryGetValue(obj.name, out var meshRenderers))
        {
            foreach (var renderer in meshRenderers)
            {
                renderer.material = material;
                renderer.materials = Enumerable.Repeat(material, renderer.materials.Length).ToArray();
            }
        }
    }

    private GameObject CreatePreviewPrefab(GameObject obj)
    {
        Utils.SpawnNetworkChildren(obj);

        foreach (var component in obj.GetComponentsInChildren<Component>())
        {
            switch (component)
            {
                case MeshRenderer renderer:
                    ImpUtils.DictionaryGetOrNew(prefabPreviewMeshRenderers, obj.name).Add(renderer);
                    break;
                case SphereCollider:
                case CapsuleCollider:
                case BoxCollider:
                case MeshCollider:
                    Destroy(component);
                    break;
                case LocalPropSet:
                    Destroy(component.gameObject);
                    break;
            }
        }

        obj.SetActive(true);

        return obj;
    }

    private static PlacedTile GeneratePlacedTile(PreviewTile tile)
    {
        var tileObj = Instantiate(tile.Tile.Prefab);
        tileObj.transform.position = tile.PreviewPrefab.transform.position;
        tileObj.transform.rotation = tile.PreviewPrefab.transform.rotation;

        var aiNodes = tileObj.GetComponentsInChildren<Component>()
            .Where(comp => comp.CompareTag("AINode"))
            .Select(node =>
                {
                    var collider = node.gameObject.AddComponent<SphereCollider>();
                    collider.isTrigger = true;
                    return new TileProp
                    {
                        Object = node.gameObject,
                        Colliders = [node.gameObject.AddComponent<SphereCollider>()],
                        Type = TilePropType.AINode,
                        Parent = tileObj.transform
                    };
                }
            )
            .ToList();
        var scrapSpawns = tileObj.GetComponentsInChildren<RandomScrapSpawn>().Select(scrapSpawn =>
        {
            var collider = scrapSpawn.gameObject.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            return new TileProp
            {
                Object = scrapSpawn.gameObject,
                Colliders = [scrapSpawn.gameObject.AddComponent<SphereCollider>()],
                Type = TilePropType.ScrapSpawn,
                Parent = tileObj.transform
            };
        }).ToList();

        var globalProps = tileObj.GetComponentsInChildren<GlobalProp>().Select(globalProp =>
        {
            var colliders = globalProp.GetComponentsInChildren<Collider>();
            var obj = globalProp.gameObject;
            // Use the instance of the spawn prefab as object reference if global prop has SpawnSyncedObject.
            if (globalProp.TryGetComponent<SpawnSyncedObject>(out var syncedObject))
            {
                obj = Instantiate(
                    syncedObject.spawnPrefab,
                    syncedObject.transform.position,
                    syncedObject.transform.rotation,
                    obj.transform
                );
                colliders = obj.GetComponentsInChildren<Collider>();
            }

            return new TileProp
            {
                Object = obj,
                Colliders = colliders.ToHashSet(),
                MeshRenderers = obj.GetComponentsInChildren<MeshRenderer>(),
                Type = TilePropType.GlobalProp,
                Parent = tileObj.transform
            };
        }).ToList();

        var localProps = new List<TileProp>();
        foreach (var localPropSet in tileObj.GetComponentsInChildren<LocalPropSet>())
        {
            localProps.AddRange(
                localPropSet.Props.Weights
                    .Where(propObj => propObj.Value)
                    .Select(propObj => new TileProp
                    {
                        Object = propObj.Value,
                        Colliders = propObj.Value.gameObject.GetComponentsInChildren<Collider>().ToHashSet(),
                        MeshRenderers = propObj.Value.gameObject.GetComponentsInChildren<MeshRenderer>(),
                        Type = TilePropType.LocalProp,
                        Parent = localPropSet.transform
                    })
            );
        }

        return new PlacedTile
        {
            Blueprint = tile.Tile,
            TileProps = scrapSpawns.Concat(aiNodes).Concat(globalProps).Concat(localProps).ToList()
        };
    }

    private void RegisterDoors(bool clear = false)
    {
        if (clear)
        {
            foreach (var marker in registeredMarkers) Destroy(marker.gameObject);
            markerPositions.Clear();
            registeredMarkers.Clear();
        }

        var registeredPositions = new List<Vector3>();

        foreach (var doorway in FindObjectsOfType<Doorway>())
        {
            var markerExists = false;
            foreach (var markerPosition in markerPositions)
            {
                if (Vector3.Distance(markerPosition, doorway.transform.position) < 0.1f)
                {
                    markerExists = true;
                    break;
                }
            }

            for (var i = 0; i < registeredPositions.Count; i++)
            {
                if (Vector3.Distance(registeredPositions[i], doorway.transform.position) < 0.1f)
                {
                    registeredMarkers[i].IsConnected = true;
                    break;
                }
            }

            if (markerExists) continue;

            var markerObj = Instantiate(ImpAssets.DoorMarkerObject, doorway.transform);
            var marker = markerObj.AddComponent<DoorMarker>();
            marker.Init(doorway);

            registeredMarkers.Add(marker);
            markerPositions.Add(doorway.transform.position);
            registeredPositions.Add(doorway.transform.position);
        }
    }

    private DoorMarker highlightedMarker;
    private readonly RaycastHit[] rayHits = new RaycastHit[5];

    private DoorMarker currentPreviewMarker;

    private bool componentPickedThisFrame;

    private void Update()
    {
        // var camera = Imperium.Freecam.IsFreecamEnabled.Value
        // ? Imperium.Freecam.FreecamCamera
        // : Imperium.Player.gameplayCamera;

        var camera = Imperium.Player.gameplayCamera;

        var intersects = Physics.RaycastNonAlloc(
            camera.transform.position,
            camera.transform.forward,
            rayHits,
            10
        );

        var foundMarker = false;

        for (var i = 0; i < intersects; i++)
        {
            if (rayHits[i].collider.transform.parent &&
                rayHits[i].collider.transform.parent.TryGetComponent<DoorMarker>(out var marker))
            {
                foundMarker = true;

                if (highlightedMarker == marker) break;

                if (highlightedMarker) highlightedMarker.Unhighlight();

                highlightedMarker = marker;
                highlightedMarker.Highlight();

                break;
            }
        }

        // No marker found, unhighlight last looked at marker
        if (!foundMarker)
        {
            if (highlightedMarker)
            {
                highlightedMarker.Unhighlight();
                highlightedMarker = null;
            }

            selectedTile?.PreviewPrefab.SetActive(false);
            selectedBlocker?.PreviewPrefab.SetActive(false);
            selectedConnector?.PreviewPrefab.SetActive(false);
        }
        else
        {
            if (selectedTile != null)
            {
                selectedTile.PreviewPrefab.SetActive(true);

                if (currentPreviewMarker != highlightedMarker || componentPickedThisFrame)
                {
                    var doorPivot = highlightedMarker.DoorwayOrigin;
                    selectedTile.PreviewPrefab.transform.position =
                        doorPivot - selectedTile.Tile.DoorwayOrigins[selectedTile.CurrentDoorIndex];
                    selectedTile.PreviewPrefab.transform.rotation = selectedTile.Tile.OriginalRotation;
                    selectedTile.PreviewPrefab.transform.RotateAround(
                        doorPivot,
                        Vector3.up,
                        highlightedMarker.DoorwayRotation.eulerAngles.y - 180 -
                        selectedTile.Tile.DoorwayYRotations[selectedTile.CurrentDoorIndex]
                    );

                    if (highlightedMarker.IsConnected)
                    {
                        previewValid = false;
                        SetPreviewError(selectedTile.PreviewPrefab);
                    }
                    else
                    {
                        previewValid = true;
                        SetPreviewOkay(selectedTile.PreviewPrefab);
                    }
                }
            }
            else if (selectedBlocker != null)
            {
                selectedBlocker.PreviewPrefab.SetActive(true);

                if (currentPreviewMarker != highlightedMarker || componentPickedThisFrame)
                {
                    selectedBlocker.PreviewPrefab.transform.position = highlightedMarker.DoorwayOrigin;
                    selectedBlocker.PreviewPrefab.transform.rotation = highlightedMarker.DoorwayRotation;
                }

                if (highlightedMarker.IsConnected)
                {
                    previewValid = false;
                    SetPreviewError(selectedBlocker.PreviewPrefab);
                }
                else
                {
                    previewValid = true;
                    SetPreviewOkay(selectedBlocker.PreviewPrefab);
                }
            }
            else if (selectedConnector != null)
            {
                selectedConnector.PreviewPrefab.SetActive(true);

                if (currentPreviewMarker != highlightedMarker || componentPickedThisFrame)
                {
                    selectedConnector.PreviewPrefab.transform.position = highlightedMarker.DoorwayOrigin;
                    selectedConnector.PreviewPrefab.transform.rotation = highlightedMarker.DoorwayRotation;
                }

                if (highlightedMarker.HasDoorway || !highlightedMarker.IsConnected)
                {
                    previewValid = false;
                    SetPreviewError(selectedConnector.PreviewPrefab);
                }
                else
                {
                    previewValid = true;
                    SetPreviewOkay(selectedConnector.PreviewPrefab);
                }
            }

            currentPreviewMarker = highlightedMarker;
            componentPickedThisFrame = false;
        }

        var scrollValue = Mathf.RoundToInt(Imperium.IngamePlayerSettings.playerInput.actions
            .FindAction("SwitchItem")
            .ReadValue<float>());

        timeSinceCycling += Time.deltaTime;
        if (timeSinceCycling > 0.15f)
        {
            if (scrollValue != 0)
            {
                OnMouseCycle(scrollValue > 0);
                timeSinceCycling = 0;
            }
        }
    }

    internal static class Utils
    {
        internal static void SpawnNetworkChildren(GameObject obj)
        {
            foreach (var syncedObject in obj.GetComponentsInChildren<SpawnSyncedObject>())
            {
                Instantiate(
                    syncedObject.spawnPrefab,
                    syncedObject.transform.position,
                    syncedObject.transform.rotation,
                    obj.transform
                );
            }
        }
    }
}

internal record PlacedDungeon
{
    internal List<PlacedTile> Tiles { get; init; } = [];
}

internal record TileProp
{
    internal GameObject Object { get; init; }
    internal TilePropType Type { get; init; }
    internal Transform Parent { get; init; }

    internal HashSet<Collider> Colliders { get; init; }
    internal MeshRenderer[] MeshRenderers { get; init; }

    internal Vector3 OriginalPosition { get; init; }
    internal Quaternion OriginalRotatioon { get; init; }
    internal Vector3 PositionOffset { get; init; }
    internal Quaternion RotationOffset { get; init; }
}

internal readonly struct PlacedTile
{
    internal Tile Blueprint { get; init; }
    internal List<TileProp> TileProps { get; init; }
}

internal readonly struct Tile
{
    internal string Name { get; init; }
    internal GameObject Prefab { get; init; }
    internal Quaternion OriginalRotation { get; init; }
    internal Doorway[] Doorways { get; init; }
    internal Vector3[] DoorwayOrigins { get; init; }
    internal float[] DoorwayYRotations { get; init; }
    internal float ScrapSpawns { get; init; }
    internal float AINodesCount { get; init; }
}

internal enum TilePropType
{
    GlobalProp,
    LocalProp,
    AINode,
    ScrapSpawn
}

internal record PreviewTile
{
    internal Tile Tile { get; init; }
    internal GameObject PreviewPrefab { get; init; }
    internal int CurrentDoorIndex { get; set; }
}

internal struct Blocker
{
    internal string Name { get; init; }
    internal DoorwaySocket Socket { get; init; }
    internal GameObject Prefab { get; init; }
    internal Quaternion OriginalRotation { get; init; }
}

internal record PreviewBlocker
{
    internal Blocker Blocker { get; init; }
    internal GameObject PreviewPrefab { get; init; }
}

internal struct Connector
{
    internal string Name { get; init; }
    internal DoorwaySocket Socket { get; init; }
    internal GameObject Prefab { get; init; }
    internal Quaternion OriginalRotation { get; init; }
}

internal record PreviewConnector
{
    internal Connector Connector { get; init; }
    internal GameObject PreviewPrefab { get; init; }
}
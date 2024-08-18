using System.Collections.Generic;
using System.Linq;
using DunGen;
using Imperium.Interface.TilePicker;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Imperium.Core.LevelEditor;

internal class ImpLevelEditor : MonoBehaviour
{
    private readonly HashSet<DoorMarker> registeredMarkers = [];
    private readonly HashSet<Vector3> markerPositions = [];

    private readonly ImpBinding<List<Tile>> Tiles = new([]);

    private PreviewTile selectedTile;

    internal static ImpLevelEditor Create() => new GameObject("Imp_LevelEditor").AddComponent<ImpLevelEditor>();

    private TilePicker tilePicker;

    private void Awake()
    {
        Imperium.IsSceneLoaded.onUpdate += OnSceneChange;

        Imperium.IngamePlayerSettings.playerInput.actions["ActivateItem"].performed += OnLeftClick;
        Imperium.IngamePlayerSettings.playerInput.actions["PingScan"].performed += OnRightClick;

        tilePicker = Imperium.Interface.Get<TilePicker>();
        tilePicker.InitUI(Imperium.Interface.Theme);
        tilePicker.BindUI(PickTile, Tiles);

        foreach (var tileSet in Resources.FindObjectsOfTypeAll<TileSet>())
        {
            foreach (var tile in tileSet.TileWeights.Weights)
            {
                Tiles.Value.Add(new Tile
                {
                    Name = tile.Value.name,
                    Prefab = tile.Value,
                    OriginalRotation = tile.Value.transform.rotation,
                    DoorwayPositions = tile.Value.GetComponentsInChildren<Doorway>()
                        .Select(doorway => doorway.transform.localPosition)
                        .ToArray(),
                    DoorwayRotationsY = tile.Value.GetComponentsInChildren<Doorway>()
                        .Select(doorway => doorway.transform.localRotation.y)
                        .ToArray()
                });
            }
        }

        Tiles.Refresh();
    }

    private void OnCyclePreviewDoor(bool forward)
    {
        if (selectedTile == null) return;

        if (forward)
        {
            selectedTile.CurrentDoorIndex = (selectedTile.CurrentDoorIndex + 1) % selectedTile.Tile.DoorwayPositions.Length;
        }
        else
        {
            if (selectedTile.CurrentDoorIndex == 0)
            {
                selectedTile.CurrentDoorIndex = selectedTile.Tile.DoorwayPositions.Length - 1;
            }
            else
            {
                selectedTile.CurrentDoorIndex--;
            }
        }

        if (highlightedMarker)
        {
            var doorPivot = highlightedMarker.DoorwayOrigin;
            selectedTile.PreviewPrefab.transform.position =
                doorPivot - selectedTile.Tile.DoorwayPositions[selectedTile.CurrentDoorIndex];
            var rotationOffset = Quaternion.AngleAxis(
                selectedTile.Tile.DoorwayRotationsY[selectedTile.CurrentDoorIndex],
                Vector3.up
            );
            transform.position = rotationOffset * (selectedTile.PreviewPrefab.transform.position - doorPivot) + doorPivot;
            transform.rotation = rotationOffset * selectedTile.Tile.OriginalRotation;
        }
    }

    private void OnSceneChange(bool isLoaded)
    {
        foreach (var registeredMarker in registeredMarkers) Destroy(registeredMarker);
        registeredMarkers.Clear();

        RegisterDoors();
    }

    private void OnLeftClick(InputAction.CallbackContext _)
    {
        if (!highlightedMarker ||
            Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat ||
            Imperium.ShipBuildModeManager.InBuildMode) return;

        if (!highlightedMarker) return;

        if (selectedTile != null)
        {
            var newTile = Instantiate(selectedTile.Tile.Prefab);
            newTile.transform.position = selectedTile.PreviewPrefab.transform.position;
            newTile.transform.rotation = selectedTile.PreviewPrefab.transform.rotation;
            selectedTile.PreviewPrefab.SetActive(false);
            selectedTile = null;

            highlightedMarker.DisableCollider();

            RegisterDoors();
        }
        else
        {
            tilePicker.Open();
        }
    }

    private void OnRightClick(InputAction.CallbackContext _)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat ||
            Imperium.ShipBuildModeManager.InBuildMode) return;

        selectedTile.PreviewPrefab.SetActive(false);
        selectedTile = null;
    }

    private readonly Dictionary<string, GameObject> prefabPreviewCache = [];

    private void PickTile(Tile tile)
    {
        if (!prefabPreviewCache.TryGetValue(tile.Name, out var previewPrefab))
        {
            previewPrefab = Instantiate(tile.Prefab);
            prefabPreviewCache[tile.Name] = previewPrefab;
        }

        foreach (var component in previewPrefab.GetComponentsInChildren<Component>())
        {
            switch (component)
            {
                case MeshRenderer renderer:
                    renderer.material = ImpAssets.HologramOkay;
                    renderer.materials = Enumerable.Repeat(ImpAssets.HologramOkay, renderer.materials.Length).ToArray();

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

        selectedTile = new PreviewTile
        {
            Tile = tile,
            PreviewPrefab = previewPrefab,
            CurrentDoorIndex = 0
        };

        tilePicker.Close();
    }

    private void RegisterDoors()
    {
        foreach (var doorway in FindObjectsOfType<Doorway>())
        {
            if (markerPositions.Contains(doorway.transform.position)) continue;

            var markerObj = Instantiate(ImpAssets.DoorMarkerObject, doorway.transform);
            var marker = markerObj.AddComponent<DoorMarker>();
            marker.Init(doorway);

            registeredMarkers.Add(marker);
            markerPositions.Add(doorway.transform.position);
        }
    }

    private DoorMarker highlightedMarker;
    private readonly RaycastHit[] rayHits = new RaycastHit[5];

    private DoorMarker currentPreviewMarker;

    private void Update()
    {
        var intersects = Physics.RaycastNonAlloc(
            Imperium.Player.gameplayCamera.transform.position,
            Imperium.Player.gameplayCamera.transform.forward,
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
        }
        else
        {
            if (selectedTile != null)
            {
                selectedTile.PreviewPrefab.SetActive(true);

                if (currentPreviewMarker != highlightedMarker)
                {
                    var doorPivot = highlightedMarker.DoorwayOrigin;
                    selectedTile.PreviewPrefab.transform.position =
                        doorPivot - selectedTile.Tile.DoorwayPositions[selectedTile.CurrentDoorIndex];
                    var rotationOffset = Quaternion.AngleAxis(
                        selectedTile.Tile.DoorwayRotationsY[selectedTile.CurrentDoorIndex],
                        Vector3.up
                    );
                    transform.position = rotationOffset * (selectedTile.PreviewPrefab.transform.position - doorPivot) + doorPivot;
                    transform.rotation = rotationOffset * selectedTile.Tile.OriginalRotation;

                    currentPreviewMarker = highlightedMarker;
                }
            }
        }

        var scrollValue = Mathf.RoundToInt(Imperium.IngamePlayerSettings.playerInput.actions
            .FindAction("SwitchItem")
            .ReadValue<float>());


        timeSinceCycling += Time.deltaTime;
        if (timeSinceCycling > 0.15f)
        {
            if (scrollValue != 0)
            {
                OnCyclePreviewDoor(scrollValue > 0);
                timeSinceCycling = 0;
            }
        }
    }

    private float timeSinceCycling;
}

internal struct Tile
{
    internal string Name { get; init; }
    internal GameObject Prefab { get; init; }
    internal Quaternion OriginalRotation { get; init; }
    internal Vector3[] DoorwayPositions { get; init; }
    internal float[] DoorwayRotationsY { get; init; }
}

internal record PreviewTile
{
    internal Tile Tile { get; init; }
    internal GameObject PreviewPrefab { get; init; }
    internal int CurrentDoorIndex { get; set; } = 0;
}
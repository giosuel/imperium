using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using Imperium.Core.LevelEditor;
using Imperium.Interface.ComponentManager.Widgets;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Tile = Imperium.Core.LevelEditor.Tile;

namespace Imperium.Interface.ComponentManager;

public class ComponentManager : BaseUI
{
    private Transform content;

    private Transform tileList;
    private Transform connectorList;
    private Transform blockerList;

    private GameObject buttonTemplate;

    private RawImage cameraCanvas;

    private ComponentPreview preview;

    private readonly List<(Tile, ComponentButton)> tileButtons = [];
    private readonly List<(Blocker, ComponentButton)> blockerButtons = [];
    private readonly List<(Connector, ComponentButton)> connectorButtons = [];

    private readonly ImpBinding<GameObject> selectedButton = new();

    private Action<Tile> onPickTile;
    private Action<Blocker> onPickBlocker;
    private Action<Connector> onPickConnector;

    protected override void InitUI()
    {
        content = container.Find("Content");

        tileList = content.Find("Components/Tiles/ScrollView/Viewport/Content");
        connectorList = content.Find("Components/Connectors/ScrollView/Viewport/Content");
        blockerList = content.Find("Components/Blockers/ScrollView/Viewport/Content");

        preview = content.gameObject.AddComponent<ComponentPreview>();
        preview.Init();

        buttonTemplate = tileList.Find("Template").gameObject;
        buttonTemplate.SetActive(false);
    }

    internal void BindUI(
        Action<Tile> onPickTileCallback,
        Action<Blocker> onPickBlockerCallback,
        Action<Connector> onPickConnectorCallback,
        IBinding<List<Tile>> tiles,
        IBinding<List<Blocker>> blockers,
        IBinding<List<Connector>> connectors
    )
    {
        onPickTile = onPickTileCallback;
        onPickBlocker = onPickBlockerCallback;
        onPickConnector = onPickConnectorCallback;

        tiles.onUpdate += OnTilesUpdate;
        blockers.onUpdate += OnBlockersUpdate;
        connectors.onUpdate += OnConnectorsUpdate;
        OnTilesUpdate(tiles.Value);
        OnBlockersUpdate(blockers.Value);
        OnConnectorsUpdate(connectors.Value);
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            content,
            new StyleOverride("Preview/Content", Variant.DARKER),
            new StyleOverride("Components/Tiles", Variant.DARKER),
            new StyleOverride("Components/Blockers", Variant.DARKER),
            new StyleOverride("Components/Connectors", Variant.DARKER)
        );
    }

    internal void OpenForPrimary(DoorwaySocket socket)
    {
        foreach (var (_, button) in connectorButtons) button.SetInteractable(false);

        foreach (var (tile, button) in tileButtons)
        {
            button.SetInteractable(tile.Doorways.Any(doorway => doorway.socket.IsCompatible(socket)));
        }

        foreach (var (blocker, button) in blockerButtons)
        {
            button.SetInteractable(blocker.Socket.IsCompatible(socket));
        }

        Open();
    }

    internal void OpenForSecondary(DoorwaySocket socket)
    {
        foreach (var (_, button) in tileButtons) button.SetInteractable(false);
        foreach (var (_, button) in blockerButtons) button.SetInteractable(false);

        foreach (var (blocker, button) in connectorButtons)
        {
            button.SetInteractable(blocker.Socket.IsCompatible(socket));
        }

        Open();
    }

    private void OnTilesUpdate(List<Tile> tiles)
    {
        foreach (var (_, button) in tileButtons) Destroy(button);
        tileButtons.Clear();

        foreach (var tile in tiles) RegisterTileButton(tile);
    }

    private void OnBlockersUpdate(List<Blocker> blockers)
    {
        foreach (var (_, button) in blockerButtons) Destroy(button);
        blockerButtons.Clear();

        foreach (var blocker in blockers) RegisterBlockerButton(blocker);
    }

    private void OnConnectorsUpdate(List<Connector> connectors)
    {
        foreach (var (_, button) in connectorButtons) Destroy(button);
        connectorButtons.Clear();

        foreach (var connector in connectors) RegisterConnectorButton(connector);
    }

    private void OnSelectTile(Tile tile)
    {
        preview.PreviewTile(tile);
    }

    private void OnSelectBlocker(Blocker blocker)
    {
        preview.PreviewBlocker(blocker);
    }

    private void OnSelectConnector(Connector connector)
    {
        preview.PreviewConnector(connector);
    }

    private void RegisterTileButton(Tile tile)
    {
        var buttonObj = Instantiate(buttonTemplate, tileList);
        buttonObj.SetActive(true);
        buttonObj.transform.Find("Text").GetComponent<TMP_Text>().text = tile.Name;
        var button = buttonObj.AddComponent<ComponentButton>();
        button.Init(
            tile.Name,
            () => OnSelectTile(tile),
            () => onPickTile?.Invoke(tile),
            selectedButton,
            theme
        );
        tileButtons.Add((tile, button));
    }

    private void RegisterBlockerButton(Blocker blocker)
    {
        var buttonObj = Instantiate(buttonTemplate, blockerList);
        buttonObj.SetActive(true);
        buttonObj.transform.Find("Text").GetComponent<TMP_Text>().text = blocker.Name;
        var button = buttonObj.AddComponent<ComponentButton>();
        button.Init(
            blocker.Name,
            () => OnSelectBlocker(blocker),
            () => onPickBlocker?.Invoke(blocker),
            selectedButton,
            theme
        );
        blockerButtons.Add((blocker, button));
    }

    private void RegisterConnectorButton(Connector connector)
    {
        var buttonObj = Instantiate(buttonTemplate, connectorList);
        buttonObj.SetActive(true);
        buttonObj.transform.Find("Text").GetComponent<TMP_Text>().text = connector.Name;
        var button = buttonObj.AddComponent<ComponentButton>();
        button.Init(
            connector.Name,
            () => OnSelectConnector(connector),
            () => onPickConnector?.Invoke(connector),
            selectedButton,
            theme
        );
        connectorButtons.Add((connector, button));
    }

    protected override void OnClose()
    {
        selectedButton.Set(null);
        preview.OnClose();
    }
}
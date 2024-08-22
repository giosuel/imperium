using System;
using System.Collections.Generic;
using System.Linq;
using DunGen;
using Imperium.Core.LevelEditor;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Tile = Imperium.Core.LevelEditor.Tile;

namespace Imperium.Interface.TilePicker;

public class TilePicker : BaseUI
{
    private Transform content;

    private Transform buttonList;
    private GameObject buttonTemplate;

    private readonly List<(Tile, Button)> tileButtons = [];
    private readonly List<(Blocker, Button)> blockerButtons = [];
    private readonly List<(Connector, Button)> connectorButtons = [];

    private Action<Tile> onPickTile;
    private Action<Blocker> onPickBlocker;
    private Action<Connector> onPickConnector;

    protected override void InitUI()
    {
        content = container.Find("Content");

        buttonList = content.Find("List/ScrollView/Viewport/Content");
        buttonTemplate = content.Find("List/ScrollView/Viewport/Content/Template").gameObject;
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
            new StyleOverride("List", Variant.DARKER),
            new StyleOverride("List/ScrollView/Scrollbar", Variant.DARKEST),
            new StyleOverride("List/ScrollView/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );

        ImpThemeManager.Style(
            themeUpdate,
            buttonTemplate.transform,
            new StyleOverride("", Variant.DARKER)
        );


        foreach (var button in tileButtons)
        {
            ImpThemeManager.Style(
                themeUpdate,
                button.Item2.transform,
                new StyleOverride("", Variant.DARKER)
            );
        }

        foreach (var button in blockerButtons)
        {
            ImpThemeManager.Style(
                themeUpdate,
                button.Item2.transform,
                new StyleOverride("", Variant.DARKER)
            );
        }

        foreach (var button in connectorButtons)
        {
            ImpThemeManager.Style(
                themeUpdate,
                button.Item2.transform,
                new StyleOverride("", Variant.DARKER)
            );
        }
    }

    internal void OpenForPrimary(DoorwaySocket socket)
    {
        ShowPrimaryForSocket(socket);
        Open();
    }

    internal void OpenForSecondary(DoorwaySocket socket)
    {
        ShowSecondaryForSocket(socket);
        Open();
    }

    private void ShowPrimaryForSocket(DoorwaySocket socket)
    {
        foreach (var (_, button) in connectorButtons) button.gameObject.SetActive(false);

        foreach (var (tile, button) in tileButtons)
        {
            button.gameObject.SetActive(true);
            button.interactable = tile.Doorways.Any(doorway => doorway.socket == socket);
        }

        foreach (var (blocker, button) in blockerButtons)
        {
            button.gameObject.SetActive(true);
            button.interactable = blocker.Socket == socket;
        }
    }

    private void ShowSecondaryForSocket(DoorwaySocket socket)
    {
        foreach (var (_, button) in tileButtons) button.gameObject.SetActive(false);
        foreach (var (_, button) in blockerButtons) button.gameObject.SetActive(false);

        foreach (var (blocker, button) in connectorButtons)
        {
            button.gameObject.SetActive(true);
            button.interactable = blocker.Socket == socket;
        }
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

    private void RegisterTileButton(Tile tile)
    {
        var buttonObj = Instantiate(buttonTemplate, buttonList);
        buttonObj.SetActive(true);
        buttonObj.transform.Find("Text").GetComponent<TMP_Text>().text = tile.Name;
        var button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => onPickTile?.Invoke(tile));
        tileButtons.Add((tile, button));
    }

    private void RegisterBlockerButton(Blocker blocker)
    {
        var buttonObj = Instantiate(buttonTemplate, buttonList);
        buttonObj.SetActive(true);
        buttonObj.transform.Find("Text").GetComponent<TMP_Text>().text = blocker.Name;
        var button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => onPickBlocker?.Invoke(blocker));
        blockerButtons.Add((blocker, button));
    }

    private void RegisterConnectorButton(Connector connector)
    {
        var buttonObj = Instantiate(buttonTemplate, buttonList);
        buttonObj.SetActive(true);
        buttonObj.transform.Find("Text").GetComponent<TMP_Text>().text = connector.Name;
        var button = buttonObj.GetComponent<Button>();
        button.onClick.AddListener(() => onPickConnector?.Invoke(connector));
        connectorButtons.Add((connector, button));
    }
}
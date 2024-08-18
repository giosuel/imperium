using System;
using System.Collections.Generic;
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

    private Transform tileList;
    private GameObject tileTemplate;

    private readonly List<GameObject> tileButtons = [];

    private Action<Tile> pickCallback;

    protected override void InitUI()
    {
        content = container.Find("Content");

        tileList = content.Find("List/ScrollView/Viewport/Content");
        tileTemplate = content.Find("List/ScrollView/Viewport/Content/Template").gameObject;
        tileTemplate.SetActive(false);
    }

    internal void BindUI(Action<Tile> onPickCallback, IBinding<List<Tile>> tiles)
    {
        pickCallback = onPickCallback;

        tiles.onUpdate += OnTilesUpdate;
        OnTilesUpdate(tiles.Value);
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            content,
            new StyleOverride("List", Variant.DARKER),
            new StyleOverride("List/ScrollView/Scrollbar", Variant.DARKEST),
            new StyleOverride("List/ScrollView/Scrollbar/SlidingArea/Handle", Variant.FOREGROUND)
        );

        ImpThemeManager.Style(
            themeUpdate,
            tileTemplate.transform,
            new StyleOverride("", Variant.DARKER)
        );


        foreach (var button in tileButtons)
        {
            ImpThemeManager.Style(
                themeUpdate,
                button.transform,
                new StyleOverride("", Variant.DARKER)
            );
        }
    }

    private void OnTilesUpdate(List<Tile> tiles)
    {
        foreach (var tileButton in tileButtons) Destroy(tileButton);
        tileButtons.Clear();

        foreach (var tile in tiles) RegisterButton(tile);
    }

    private void RegisterButton(Tile tile)
    {
        var tileButtonObj = Instantiate(tileTemplate, tileList);
        tileButtonObj.SetActive(true);
        tileButtonObj.GetComponent<Button>().onClick.AddListener(() => pickCallback?.Invoke(tile));
        tileButtonObj.transform.Find("Text").GetComponent<TMP_Text>().text = tile.Name;
        tileButtons.Add(tileButtonObj);
    }
}
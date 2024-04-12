#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.Core;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SpawningUI;

internal class SpawningUI : BaseUI
{
    private TMP_Dropdown results;
    private TMP_InputField input;
    private TMP_Text modeText;
    private Button modeButton;
    private TMP_Text placeholder;

    private KeyboardShortcut switchModeShortcut = new(KeyCode.Tab);
    private KeyboardShortcut downArrow = new(KeyCode.DownArrow);
    private SpawningMode spawningMode;
    private List<string> dropdownItems;

    private readonly Dictionary<SpawningMode, (string, int, int)> previouslySpawnedObjects = new()
    {
        { SpawningMode.ENTITY, ("", 1, -1) },
        { SpawningMode.ITEM, ("", 1, -1) },
        { SpawningMode.MAP_HAZARD, ("", 1, -1) },
    };

    public override void Awake() => InitializeUI(false, true);

    protected override void InitUI()
    {
        spawningMode = SpawningMode.ENTITY;

        input = container.Find("Window/Input").GetComponent<TMP_InputField>();
        results = container.Find("Window/Input/Dropdown").GetComponent<TMP_Dropdown>();
        modeButton = container.Find("Window/Input/Mode").GetComponent<Button>();
        modeText = container.Find("Window/Input/Mode/Text").GetComponent<TMP_Text>();
        placeholder = container.Find("Window/Input/TextArea/Placeholder").GetComponent<TMP_Text>();

        modeButton.onClick.AddListener(OnModeRotate);
        input.onValueChanged.AddListener(_ => OnInput(input.text));
        input.onSubmit.AddListener(_ => OnSubmit());
        results.onValueChanged.AddListener(OnDropdownSelect);

        // Can't update dropdown yet since there is still some internal stuff going on at this point (apparently)
        SetMode(SpawningMode.ENTITY, false);
    }

    private void OnModeRotate()
    {
        SetMode(spawningMode switch
        {
            SpawningMode.ENTITY => SpawningMode.ITEM,
            SpawningMode.ITEM => SpawningMode.MAP_HAZARD,
            SpawningMode.MAP_HAZARD => SpawningMode.ENTITY,
            _ => spawningMode
        }, true);

        // The switch button (Tab) is also used to open the quick menu, this fix makes it so the game thinks the quick
        // menu is still open after "closing" it internally so the menu doesn't open after pressing Tab more than
        // two times
        ImpUtils.Interface.ToggleCursorState(true);
    }

    private void SetMode(SpawningMode mode, bool updateDropdown)
    {
        spawningMode = mode;
        modeText.text = ModeNameMap[spawningMode];

        dropdownItems = spawningMode switch
        {
            SpawningMode.ENTITY => Imperium.ObjectManager.AllEntities.Value.Keys.ToList(),
            SpawningMode.ITEM => Imperium.ObjectManager.AllItems.Value.Keys.ToList(),
            SpawningMode.MAP_HAZARD => Imperium.ObjectManager.AllMapHazards.Value.Keys.ToList(),
            _ => []
        };

        SetPlaceholder();
        if (updateDropdown) OnInput(input.text);
    }

    private void SetPlaceholder()
    {
        placeholder.text = spawningMode switch
        {
            SpawningMode.ENTITY => "e.g. Jester",
            SpawningMode.ITEM => "e.g. Flashlight",
            SpawningMode.MAP_HAZARD => "e.g. Turret",
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private void OnDropdownSelect(int value) => Spawn(results.options[value].text, 1, -1);

    private void OnSubmit()
    {
        if (results.options.Count > 0)
        {
            var (_, amount, value) = GetInputParameters(input.text.Trim());
            previouslySpawnedObjects[spawningMode] = (results.options[0].text, Math.Min(amount, 100), value);
            Spawn(results.options[0].text, amount, value);
        }
        else if (!string.IsNullOrEmpty(input.text))
        {
            ImpOutput.Send(
                $"Failed to find object '{input.text}'",
                isWarning: true, notificationType: NotificationType.Other
            );
        }
        else if (!string.IsNullOrEmpty(previouslySpawnedObjects[spawningMode].Item1))
        {
            var (objectName, amount, value) = previouslySpawnedObjects[spawningMode];
            Spawn(objectName, amount, value);
        }

        input.ActivateInputField();
    }

    private static (string, int, int) GetInputParameters(string text)
    {
        var amount = 1;
        var value = -1;

        var split = text.Split(" ").ToList();
        switch (split.Count)
        {
            case > 2:
                int.TryParse(split[^1], out value);
                int.TryParse(split[^2], out amount);
                break;
            case > 1:
                int.TryParse(split[1], out amount);
                break;
        }

        return (split[0], amount, value);
    }

    private void OnInput(string text)
    {
        results.Hide();

        var (inputText, _, _) = GetInputParameters(text.Trim());

        if (text.Length != 0)
        {
            results.options = dropdownItems
                .Where(key => ItemNameMatchesInput(key, inputText))
                .Select(key => new TMP_Dropdown.OptionData(key)).ToList();
        }
        else
        {
            results.options = [];
        }

        if (results.options.Count > 0) results.Show();

        input.ActivateInputField();
        input.caretPosition = input.text.Length;
    }

    private static bool ItemNameMatchesInput(string objectName, string inputText)
    {
        var inputNormalized = inputText.Trim().ToLower();

        return objectName.ToLower().Contains(inputNormalized) ||
               Imperium.ObjectManager.GetDisplayName(objectName).ToLower().Contains(inputNormalized);
    }

    private void Spawn(string objectName, int amount, int value)
    {
        switch (spawningMode)
        {
            case SpawningMode.ENTITY:
                ObjectManager.SpawnEntity(objectName, amount: amount, health: value);
                break;
            case SpawningMode.ITEM:
                ObjectManager.SpawnItem(objectName, PlayerManager.LocalPlayerId, amount: amount, value: value);
                break;
            case SpawningMode.MAP_HAZARD:
                ObjectManager.SpawnMapHazard(objectName, amount: amount);
                break;
            default:
                return;
        }

        Imperium.Interface.Close();
    }

    private void Update()
    {
        if (IsOpen && switchModeShortcut.IsDown()) OnModeRotate();
        if (downArrow.IsDown())
        {
            results.Select();
        }
    }

    protected override void OnOpen()
    {
        input.text = "";
        input.ActivateInputField();
        //input.Select();
    }

    private enum SpawningMode
    {
        ENTITY,
        ITEM,
        MAP_HAZARD
    }

    private static readonly Dictionary<SpawningMode, string> ModeNameMap = new()
    {
        { SpawningMode.ENTITY, "Entities" },
        { SpawningMode.ITEM, "Items" },
        { SpawningMode.MAP_HAZARD, "Map Hazards" },
    };
}
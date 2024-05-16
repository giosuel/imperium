#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SpawningUI;

internal class SpawningUI : SingleplexUI
{
    private TMP_InputField input;

    private GameObject moreItems;
    private TMP_Text moreItemsText;

    private Transform entryContainer;
    private GameObject template;

    private readonly List<SpawningObjectEntry> entries = [];
    private SpawningObjectEntry previouslySpawnedObject;
    private int previouslySpawnedAmount;
    private int previouslySpawnedValue;

    private int selectedIndex = -1;

    protected override void InitUI()
    {
        entryContainer = container.Find("Window/Results");
        template = container.Find("Window/Results/Template").gameObject;
        template.SetActive(false);
        input = container.Find("Window/Input").GetComponent<TMP_InputField>();

        moreItems = container.Find("Window/Results/MoreItems").gameObject;
        moreItemsText = container.Find("Window/Results/MoreItems/Label").GetComponent<TMP_Text>();
        moreItems.SetActive(false);

        input.onValueChanged.AddListener(_ => OnInput(input.text));
        input.onSubmit.AddListener(_ => OnSubmit());

        Imperium.InputBindings.SpawningMap["ArrowUp"].performed += OnSelectPrevious;
        Imperium.InputBindings.SpawningMap["ArrowDown"].performed += OnSelectNext;
        Imperium.InputBindings.SpawningMap["Submit"].performed += OnSelectNext;

        onOpen += Imperium.InputBindings.SpawningMap.Enable;
        onClose += Imperium.InputBindings.SpawningMap.Disable;

        GenerateItems();
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container.Find("Window"),
            new StyleOverride("Input", Variant.BACKGROUND),
            new StyleOverride("Input/Border", Variant.DARKER),
            new StyleOverride("Results/MoreItems", Variant.DARKEST),
            new StyleOverride("Results/Template", Variant.DARKER)
        );

        base.OnThemeUpdate(themeUpdate);
    }

    private void GenerateItems()
    {
        foreach (var entity in Imperium.ObjectManager.AllEntities.Value)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawningObjectEntry.SpawnObjectType.ENTITY,
                entity.enemyName,
                entity.enemyPrefab?.name,
                () => Spawn(spawningEntry, 1, -1),
                () => SelectItemAndDeselectOthers(currentIndex),
                theme
            );
            entries.Add(spawningEntry);
        }

        foreach (var item in Imperium.ObjectManager.AllItems.Value)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawningObjectEntry.SpawnObjectType.ITEM,
                item.itemName,
                item.spawnPrefab?.name ?? Imperium.ObjectManager.GetStaticPrefabName(item.itemName),
                () => Spawn(spawningEntry, 1, -1),
                () => SelectItemAndDeselectOthers(currentIndex),
                theme
            );
            entries.Add(spawningEntry);
        }

        foreach (var (hazardName, hazard) in Imperium.ObjectManager.AllMapHazards.Value)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawningObjectEntry.SpawnObjectType.MAP_HAZARD,
                hazardName,
                hazard.name,
                () => Spawn(spawningEntry, 1, -1),
                () => SelectItemAndDeselectOthers(currentIndex),
                theme
            );
            entries.Add(spawningEntry);
        }
    }

    private void SelectItemAndDeselectOthers(int index)
    {
        selectedIndex = index;
        SelectItemAndDeselectOthers();
    }

    private void SelectItemAndDeselectOthers()
    {
        for (var i = 0; i < entries.Count; i++) entries[i].SetSelected(i == selectedIndex);
    }

    private void SelectFirst()
    {
        selectedIndex = 0;
        while (!entries[selectedIndex].gameObject.activeSelf)
        {
            selectedIndex++;

            if (selectedIndex == entries.Count)
            {
                selectedIndex = -1;
                break;
            }
        }

        if (selectedIndex > -1) SelectItemAndDeselectOthers();
    }

    private void OnSelectNext(InputAction.CallbackContext callbackContext)
    {
        var traverseCounter = 0;
        do
        {
            if (selectedIndex == entries.Count - 1)
            {
                selectedIndex = 0;
            }
            else
            {
                selectedIndex++;
            }

            traverseCounter++;
            if (traverseCounter == entries.Count)
            {
                selectedIndex = -1;
                break;
            }
        } while (!entries[selectedIndex].gameObject.activeSelf);

        if (selectedIndex > -1) SelectItemAndDeselectOthers();

        // Put caret at the end since arrow keys move it
        if (input.text.Length > 0)
        {
            input.caretPosition = input.text.Length;
        }
    }

    private void OnSelectPrevious(InputAction.CallbackContext callbackContext)
    {
        var traverseCounter = 0;
        do
        {
            if (selectedIndex == 0)
            {
                selectedIndex = entries.Count - 1;
            }
            else
            {
                selectedIndex--;
            }

            traverseCounter++;
            if (traverseCounter == entries.Count)
            {
                selectedIndex = -1;
                break;
            }
        } while (!entries[selectedIndex].gameObject.activeSelf);

        if (selectedIndex > -1) SelectItemAndDeselectOthers();

        // Put caret at the end since arrow keys move it
        if (input.text.Length > 0)
        {
            input.caretPosition = input.text.Length;
        }
    }

    private void SetMoreItemsText(int amount)
    {
        if (amount < 1)
        {
            moreItems.gameObject.SetActive(false);
            return;
        }

        moreItems.SetActive(true);
        moreItemsText.text = $"{amount} more results...";
        moreItems.transform.SetAsLastSibling();
    }

    private void Spawn(SpawningObjectEntry spawningObjectEntry, int amount, int value)
    {
        if (Imperium.Freecam.IsFreecamEnabled.Value)
        {
            Imperium.ImpPositionIndicator.Activate(
                position => spawningObjectEntry.Spawn(position, amount, value, false),
                Imperium.Freecam.transform
            );
        }
        else
        {
            var playerTransform = Imperium.Player.gameplayCamera.transform;
            var spawnPosition = playerTransform.position + playerTransform.forward * 3f;

            // Note: This layer mask was copied from GrabbableObject.FallToGround()
            var hasFloorBeneath = Physics.Raycast(
                spawnPosition,
                Vector3.down,
                out var hitInfo,
                80f,
                268437760,
                QueryTriggerInteraction.Ignore
            );

            if (hasFloorBeneath) spawnPosition = hitInfo.point;
            spawningObjectEntry.Spawn(spawnPosition, amount, value);
        }

        CloseUI();
    }

    private void OnInput(string text)
    {
        var (inputText, _, _) = GetInputParameters(text.Trim());
        var resultCount = entries.Count(entry => entry.OnInput(inputText));

        SelectFirst();

        var shownItems = 0;
        var hiddenItems = 0;
        foreach (var entry in entries.Where(entry => entry.gameObject.activeSelf))
        {
            if (shownItems < 6)
            {
                shownItems++;
            }
            else
            {
                hiddenItems++;
                entry.gameObject.SetActive(false);
            }
        }

        SetMoreItemsText(hiddenItems);
    }

    private void OnSubmit()
    {
        var (_, amount, value) = GetInputParameters(input.text.Trim());

        if (selectedIndex > -1)
        {
            Spawn(entries[selectedIndex], amount, value);

            previouslySpawnedObject = entries[selectedIndex];
            previouslySpawnedAmount = amount;
            previouslySpawnedValue = value;

            CloseUI();
        }
        else if (previouslySpawnedObject)
        {
            Spawn(previouslySpawnedObject, previouslySpawnedAmount, previouslySpawnedValue);
            CloseUI();
        }
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

    protected override void OnOpen()
    {
        input.text = "";
        input.ActivateInputField();
    }
}
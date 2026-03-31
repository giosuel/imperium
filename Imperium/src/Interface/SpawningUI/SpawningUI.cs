#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Types;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Interface.SpawningUI;

internal class SpawningUI : BaseUI
{
    private const int MaxItemsShown = 8;

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

    private readonly Dictionary<SpawnObjectType, string> typeDisplayNames = new()
    {
        { SpawnObjectType.Entity, "Entity" },
        { SpawnObjectType.Item, "Item" },
        { SpawnObjectType.MapHazard, "Map Hazard" },
        { SpawnObjectType.StaticPrefab, "Static Prefab" },
        { SpawnObjectType.LocalStaticPrefab, "Static Prefab (Local)" },
        { SpawnObjectType.OutsideObject, "Outdoor Object" },
        { SpawnObjectType.Vehicle, "Vehicle" }
    };

    protected override void InitUI()
    {
        entryContainer = container.Find("Results");
        template = container.Find("Results/Template").gameObject;
        template.SetActive(false);
        input = container.Find("Input").GetComponent<TMP_InputField>();

        moreItems = container.Find("Results/MoreItems").gameObject;
        moreItemsText = container.Find("Results/MoreItems/Label").GetComponent<TMP_Text>();
        moreItems.SetActive(false);

        input.onValueChanged.AddListener(_ => OnInput(input.text));
        input.onSubmit.AddListener(_ => OnSubmit());

        Imperium.InputBindings.BaseMap.PreviousItem.performed += OnSelectPrevious;
        Imperium.InputBindings.BaseMap.NextItem.performed += OnSelectNext;
        Imperium.InputBindings.BaseMap.SelectItem.performed += OnSelectNext;

        GenerateItems();
    }

    private void GenerateItems()
    {
        foreach (var entity in Imperium.ObjectManager.LoadedEntities.Value)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawnObjectType.Entity,
                entity.enemyName,
                () => Spawn(spawningEntry, 1, -1),
                _ => SelectItemAndDeselectOthers(currentIndex),
                typeDisplayNames,
                theme
            );
            entries.Add(spawningEntry);
        }

        foreach (var item in Imperium.ObjectManager.LoadedItems.Value)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawnObjectType.Item,
                item.itemName,
                () => Spawn(spawningEntry, 1, -1),
                _ => SelectItemAndDeselectOthers(currentIndex),
                typeDisplayNames,
                theme
            );
            entries.Add(spawningEntry);
        }

        foreach (var hazardName in Imperium.ObjectManager.LoadedMapHazards.Value.Keys)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawnObjectType.MapHazard,
                hazardName,
                () => Spawn(spawningEntry, 1, -1),
                _ => SelectItemAndDeselectOthers(currentIndex),
                typeDisplayNames,
                theme
            );
            entries.Add(spawningEntry);
        }

        foreach (var vehicleName in Imperium.ObjectManager.LoadedVehicles.Value.Keys)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawnObjectType.Vehicle,
                vehicleName,
                () => Spawn(spawningEntry, 1, -1),
                _ => SelectItemAndDeselectOthers(currentIndex),
                typeDisplayNames,
                theme
            );
            entries.Add(spawningEntry);
        }

        foreach (var prefabName in Imperium.ObjectManager.LoadedStaticPrefabs.Value.Keys)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawnObjectType.StaticPrefab,
                prefabName,
                () => Spawn(spawningEntry, 1, -1),
                _ => SelectItemAndDeselectOthers(currentIndex),
                typeDisplayNames,
                theme
            );
            entries.Add(spawningEntry);
        }

        foreach (var prefabName in Imperium.ObjectManager.LoadedLocalStaticPrefabs.Value.Keys)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawnObjectType.LocalStaticPrefab,
                prefabName,
                () => Spawn(spawningEntry, 1, -1),
                _ => SelectItemAndDeselectOthers(currentIndex),
                typeDisplayNames,
                theme
            );
            entries.Add(spawningEntry);
        }

        foreach (var prefabName in Imperium.ObjectManager.LoadedOutsideObjects.Value.Keys)
        {
            var currentIndex = entries.Count;
            var spawningEntryObject = Instantiate(template, entryContainer);
            var spawningEntry = spawningEntryObject.AddComponent<SpawningObjectEntry>();
            spawningEntry.Init(
                SpawnObjectType.OutsideObject,
                prefabName,
                () => Spawn(spawningEntry, 1, -1),
                _ => SelectItemAndDeselectOthers(currentIndex),
                typeDisplayNames,
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

    private void OnSelectNext(InputAction.CallbackContext callbackContext)
    {
        if (!IsOpen) return;

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
        if (!IsOpen) return;

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
        var useIndicator = spawningObjectEntry.SpawnType is
            SpawnObjectType.MapHazard or
            SpawnObjectType.StaticPrefab or
            SpawnObjectType.Vehicle or
            SpawnObjectType.LocalStaticPrefab or
            SpawnObjectType.OutsideObject;

        if (Imperium.Freecam.IsFreecamEnabled.Value || Imperium.PlayerManager.IsFlying.Value || useIndicator)
        {
            var originTransform = Imperium.Freecam.IsFreecamEnabled.Value
                ? Imperium.Freecam.transform
                : Imperium.Player.gameplayCamera.transform;

            Imperium.ImpPositionIndicator.Activate(
                position => spawningObjectEntry.Spawn(position, amount, value, false),
                originTransform,
                castGround: false
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
            spawningObjectEntry.Spawn(spawnPosition, amount, value, true);
        }

        Close();
    }

    private void OnInput(string text)
    {
        var inputParams = GetInputParameters(text.Trim());

        foreach (var entry in entries) entry.OnInput(inputParams.Text);

        var hasSelected = false;

        var shownItems = 0;
        var hiddenItems = 0;
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (shownItems > MaxItemsShown)
            {
                hiddenItems++;
                entry.SetShown(false);
            }
            else
            {
                var isShown = entry.OnInput(inputParams.Text);
                if (isShown)
                {
                    shownItems++;
                    if (!hasSelected)
                    {
                        entry.SetSelected(true);
                        hasSelected = true;
                        selectedIndex = i;
                    }
                    else
                    {
                        entry.SetSelected(false);
                    }
                }
            }
        }

        SetMoreItemsText(hiddenItems);
    }

    private void OnSubmit()
    {
        var inputParams = GetInputParameters(input.text.Trim());

        if (selectedIndex > -1)
        {
            Spawn(entries[selectedIndex], inputParams.Amount, inputParams.Value);

            previouslySpawnedObject = entries[selectedIndex];
            previouslySpawnedAmount = inputParams.Amount;
            previouslySpawnedValue = inputParams.Value;

            Close();
        }
        else if (previouslySpawnedObject)
        {
            Spawn(previouslySpawnedObject, previouslySpawnedAmount, previouslySpawnedValue);
            Close();
        }
    }

    private static SpawnInputParameters GetInputParameters(string text)
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

        return new SpawnInputParameters
        {
            Text = split[0],
            Amount = amount,
            Value = value
        };
    }

    protected override void OnOpen()
    {
        input.text = "";
        input.ActivateInputField();
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("Input", Variant.BACKGROUND),
            new StyleOverride("Input/Border", Variant.DARKER),
            new StyleOverride("Results/MoreItems", Variant.DARKEST),
            new StyleOverride("Results/Template", Variant.DARKER)
        );
    }
}

internal enum SpawnObjectType
{
    Entity,
    Item,
    MapHazard,
    StaticPrefab,
    LocalStaticPrefab,
    OutsideObject,
    Vehicle
}

internal readonly struct SpawnInputParameters
{
    internal string Text { get; init; }
    internal int Amount { get; init; }
    internal int Value { get; init; }
}
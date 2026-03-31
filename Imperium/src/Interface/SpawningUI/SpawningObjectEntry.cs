#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.SpawningUI;

public class SpawningObjectEntry : MonoBehaviour
{
    private GameObject selectedCover;

    internal SpawnObjectType SpawnType { get; private set; }
    private string displayName;
    private string spawnObjectName;

    private string displayNameNormalized;
    private string objectNameNormalized;

    internal void Init(
        SpawnObjectType type,
        string objectName,
        Action onClick,
        Action<Vector2> onHover,
        Dictionary<SpawnObjectType, string> typeDisplayNameMap,
        ImpBinding<ImpTheme> themeBinding
    )
    {
        SpawnType = type;
        spawnObjectName = objectName;

        var overrideName = Imperium.ObjectManager.GetOverrideDisplayName(objectName);

        /*
         * Primary name is the user defined display name from the display name dictionary
         * Secondary name is the raw object name from the game, if they are equal, only the primary name is displayed.
         *
         * If an override name is defined, the override name will be used as primary name and the secondary name
         * will be ignored.
         *
         * Normalized primary and secondary names will be used for indexing.
         */
        string primaryName;
        var secondaryName = "";

        if (overrideName != null)
        {
            displayName = overrideName;
            primaryName = overrideName;
        }
        else
        {
            displayName = Imperium.ObjectManager.GetDisplayName(objectName);
            primaryName = displayName;
            if (displayName != objectName) secondaryName = $"({objectName})";
        }

        displayNameNormalized = NormalizeName(displayName);
        objectNameNormalized = NormalizeName(objectName);

        ImpButton.Bind("", transform, () => onClick?.Invoke(), themeBinding);

        selectedCover = transform.Find("Selected").gameObject;
        selectedCover.SetActive(false);

        transform.Find("Name/Primary").GetComponent<TMP_Text>().text = primaryName;
        transform.Find("Name/Secondary").GetComponent<TMP_Text>().text = secondaryName;
        transform.Find("Type").GetComponent<TMP_Text>().text = typeDisplayNameMap.GetValueOrDefault(type, "");

        gameObject.AddComponent<ImpInteractable>().onOver += onHover;
        themeBinding.onUpdate += OnThemeUpdate;
    }

    private void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("", Variant.FOREGROUND),
            new StyleOverride("Selected", Variant.FADED)
        );
    }

    internal void Spawn(Vector3 position, int amount, int value, bool spawnInInventory)
    {
        switch (SpawnType)
        {
            case SpawnObjectType.Entity:
                Imperium.ObjectManager.SpawnEntity(new EntitySpawnRequest
                {
                    Name = spawnObjectName,
                    SpawnPosition = position,
                    Amount = amount,
                    Health = value,
                    SendNotification = true
                });
                break;
            case SpawnObjectType.Item:
                Imperium.ObjectManager.SpawnItem(new ItemSpawnRequest
                {
                    Name = spawnObjectName,
                    SpawnPosition = position,
                    Amount = amount,
                    Value = value,
                    SpawnInInventory = spawnInInventory,
                    SendNotification = true
                });
                break;
            case SpawnObjectType.MapHazard:
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = spawnObjectName,
                    SpawnPosition = position,
                    Amount = amount,
                    SendNotification = true
                });
                break;
            case SpawnObjectType.Vehicle:
                Imperium.ObjectManager.SpawnVehicle(new VehicleSpawnRequest
                {
                    Name = spawnObjectName,
                    SpawnPosition = position,
                    SendNotification = true
                });
                break;
            case SpawnObjectType.StaticPrefab:
                Imperium.ObjectManager.SpawnStaticPrefab(new StaticPrefabSpawnRequest
                {
                    Name = spawnObjectName,
                    SpawnPosition = position,
                    Amount = amount,
                    SendNotification = true
                });
                break;
            case SpawnObjectType.LocalStaticPrefab:
                Imperium.ObjectManager.SpawnLocalStaticPrefab(new StaticPrefabSpawnRequest
                {
                    Name = spawnObjectName,
                    SpawnPosition = position,
                    Amount = amount,
                    SendNotification = true
                });
                break;
            case SpawnObjectType.OutsideObject:
                Imperium.ObjectManager.SpawnOutsideObject(new StaticPrefabSpawnRequest
                {
                    Name = spawnObjectName,
                    SpawnPosition = position,
                    Amount = amount,
                    SendNotification = true
                });
                break;
            default:
                return;
        }
    }

    internal void SetShown(bool isShown)
    {
        gameObject.SetActive(isShown);
        SetSelected(false);
    }

    internal void SetSelected(bool isSelected)
    {
        selectedCover.SetActive(isSelected);
    }

    internal bool OnInput(string inputText)
    {
        inputText = NormalizeName(inputText);

        var isShown = !string.IsNullOrEmpty(inputText) && (
            objectNameNormalized.Contains(inputText) || displayNameNormalized.Contains(inputText)
        );

        gameObject.SetActive(isShown);
        if (!isShown) SetSelected(false);

        return isShown;
    }

    private static string NormalizeName(string input)
    {
        return new string(input.Trim().ToLower().Where(char.IsLetterOrDigit).ToArray());
    }
}
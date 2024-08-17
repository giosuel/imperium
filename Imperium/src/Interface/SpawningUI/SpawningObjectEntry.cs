#region

using System;
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
        Action onHover,
        ImpBinding<ImpTheme> themeBinding
    )
    {
        SpawnType = type;
        spawnObjectName = objectName;

        var overrideName = Imperium.ObjectManager.GetOverrideDisplayName(objectName);
        string labelText;

        // If override name exists, use that for full label
        if (overrideName != null)
        {
            displayName = overrideName;
            labelText = overrideName;
        }
        else
        {
            displayName = Imperium.ObjectManager.GetDisplayName(objectName);
            labelText = $"{displayName} ({objectName})";
        }

        displayNameNormalized = NormalizeName(displayName);
        objectNameNormalized = NormalizeName(objectName);

        ImpButton.Bind("", transform, () => onClick?.Invoke(), themeBinding);

        selectedCover = transform.Find("Selected").gameObject;
        selectedCover.SetActive(false);
        transform.Find("Label").GetComponent<TMP_Text>().text = labelText;

        gameObject.AddComponent<ImpInteractable>().onEnter += onHover;
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
            case SpawnObjectType.CompanyCruiser:
                Imperium.ObjectManager.SpawnCompanyCruiser(new CompanyCruiserSpawnRequest
                {
                    SpawnPosition = position + Vector3.up * 5f,
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

    internal void SetSelected(bool isSelected)
    {
        selectedCover.SetActive(isSelected);
    }

    internal bool OnInput(string inputText)
    {
        inputText = NormalizeName(inputText);

        var isInList = !string.IsNullOrEmpty(inputText) && (
            objectNameNormalized.Contains(inputText) || displayNameNormalized.Contains(inputText)
        );

        gameObject.SetActive(isInList);
        return isInList;
    }

    private static string NormalizeName(string input)
    {
        return new string(input.Trim().ToLower().Where(char.IsLetterOrDigit).ToArray());
    }

    internal enum SpawnObjectType
    {
        Entity,
        Item,
        MapHazard,
        StaticPrefab,
        LocalStaticPrefab,
        OutsideObject,
        CompanyCruiser
    }
}
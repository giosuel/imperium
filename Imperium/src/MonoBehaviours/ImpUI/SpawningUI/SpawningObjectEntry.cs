#region

using System;
using System.Linq;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SpawningUI;

public class SpawningObjectEntry : MonoBehaviour
{
    private GameObject selectedCover;

    private SpawnObjectType spawnType;
    private string displayName;
    private string spawnObjectName;
    private string spawnObjectPrefabName;

    private string displayNameNormalized;
    private string spawnObjectNamNormalized;
    private string spawnObjectPrefabNameNormalized;

    internal void Init(
        SpawnObjectType type,
        string objectName,
        string prefabName,
        Action onClick,
        Action onHover,
        ImpBinding<ImpTheme> themeBinding
    )
    {
        spawnType = type;
        displayName = Imperium.ObjectManager.GetDisplayName(objectName);
        spawnObjectName = objectName ?? "";
        spawnObjectPrefabName = prefabName ?? "";

        displayNameNormalized = NormalizeName(displayName);
        spawnObjectNamNormalized = NormalizeName(spawnObjectName);
        spawnObjectPrefabNameNormalized = NormalizeName(spawnObjectPrefabName);

        ImpButton.Bind("", transform, () => onClick?.Invoke(), themeBinding);

        selectedCover = transform.Find("Selected").gameObject;
        selectedCover.SetActive(false);
        transform.Find("Label").GetComponent<TMP_Text>().text = $"{displayName} ({objectName})";

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

    internal void Spawn(Vector3 position, int amount, int value, bool spawnInInventory = true)
    {
        switch (spawnType)
        {
            case SpawnObjectType.ENTITY:
                ObjectManager.SpawnEntity(
                    spawnObjectName,
                    spawnObjectPrefabName,
                    position,
                    amount,
                    value,
                    true
                );
                break;
            case SpawnObjectType.ITEM:
                ObjectManager.SpawnItem(
                    spawnObjectName,
                    spawnObjectPrefabName,
                    spawnInInventory ? PlayerManager.LocalPlayerId : -1,
                    position,
                    amount,
                    value
                );
                break;
            case SpawnObjectType.MAP_HAZARD:
                ObjectManager.SpawnMapHazard(spawnObjectName, position, amount);
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
            spawnObjectNamNormalized.Contains(inputText)
            || spawnObjectPrefabNameNormalized.Contains(inputText)
            || displayNameNormalized.Contains(inputText)
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
        ENTITY,
        ITEM,
        MAP_HAZARD
    }
}
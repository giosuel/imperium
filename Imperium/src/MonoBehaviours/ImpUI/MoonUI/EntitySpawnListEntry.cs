#region

using System;
using System.Collections.Generic;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Netcode;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI;

public class EntitySpawnListEntry : MonoBehaviour
{
    private string spawnObjectName;
    private TMP_Text spawnChanceText;

    private EntityListType entityListListType;
    private SpawnableEnemyWithRarity entity;

    internal ImpBinding<int> Rarity { get; private set; }
    internal ImpBinding<bool> IsSpawning { get; private set; }

    internal Action onExclusive;

    public void Init(bool isNative, SpawnableEnemyWithRarity entityWithRarity, EntityListType listType)
    {
        entity = entityWithRarity;
        spawnObjectName = entity.enemyType.enemyName;
        entityListListType = listType;

        Rarity = new ImpBinding<int>(
            entity.rarity,
            GetOriginalRarity(),
            OnUpdateRarity,
            _ => ImpNetSpawning.Instance.OnSpawningChangedServerRpc()
        );
        IsSpawning = new ImpBinding<bool>(
            entity.rarity > 0,
            OnUpdateIsSpawning,
            _ => ImpNetSpawning.Instance.OnSpawningChangedServerRpc()
        );

        var content = transform;

        content.Find("Background/Text/Name").GetComponent<TMP_Text>().text = spawnObjectName;
        content.Find("Background/Text/Native").gameObject.SetActive(isNative);

        spawnChanceText = content.Find("Chance").GetComponent<TMP_Text>();

        ImpButton.Bind("Exclusive", content, OnExclusive);
        ImpToggle.Bind("Active", content, IsSpawning);
        ImpInput.Bind("Rarity", content, Rarity, min: 0, max: int.MaxValue, interactableBindings: IsSpawning);
    }

    internal void UpdateSpawnChance(int total)
    {
        spawnChanceText.text = total == 0 ? "0%" : Formatting.FormatChance(Rarity.Value / (float)total);
    }

    private void OnExclusive()
    {
        onExclusive?.Invoke();
        Rarity.Set(100, true);

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    private void OnUpdateRarity(int value) => entity.rarity = value;

    private void OnUpdateIsSpawning(bool isSpawning)
    {
        if (isSpawning)
        {
            Rarity.Reset(true);
        }
        else
        {
            Rarity.Set(0, true);
        }
    }

    private int GetOriginalRarity() =>
        entityListListType switch
        {
            EntityListType.IndoorEntity =>
                MoonManager.Current.OriginalMoonData.IndoorEntityRarities.GetValueOrDefault(entity.enemyType),
            EntityListType.OutdoorEntity =>
                MoonManager.Current.OriginalMoonData.OutdoorEntityRarities.GetValueOrDefault(entity.enemyType),
            EntityListType.DaytimeEntity =>
                MoonManager.Current.OriginalMoonData.DaytimeEntityRarities.GetValueOrDefault(entity.enemyType),
            _ => throw new ArgumentOutOfRangeException()
        };

    public enum EntityListType
    {
        IndoorEntity,
        OutdoorEntity,
        DaytimeEntity
    }
}
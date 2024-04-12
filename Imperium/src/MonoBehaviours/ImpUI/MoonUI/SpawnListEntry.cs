#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Netcode;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI;

public class SpawnListEntry : MonoBehaviour
{
    private object spawnObject;
    private string spawnObjectName;
    private TMP_Text spawnChanceText;

    private EntryType entryType;

    private Dictionary<string, SpawnListEntry> otherEntries;

    internal ImpBinding<int> Rarity { get; private set; }
    internal ImpBinding<bool> IsSpawning { get; private set; }

    public void Init(
        bool isNative,
        bool isSpawning,
        string entryName,
        object obj,
        Dictionary<string, SpawnListEntry> entries,
        EntryType type
    )
    {
        spawnObject = obj;
        spawnObjectName = entryName;
        otherEntries = entries;
        entryType = type;

        var rarity = spawnObject switch
        {
            SpawnableEnemyWithRarity entityObject => entityObject.rarity,
            SpawnableItemWithRarity scrapObject => scrapObject.rarity,
            _ => throw new ArgumentOutOfRangeException()
        };

        Rarity = new ImpBinding<int>(
            rarity,
            GetOriginalRarity(spawnObjectName),
            OnUpdateRarity,
            _ => ImpNetSpawning.Instance.OnSpawningChangedServerRpc()
        );
        IsSpawning = new ImpBinding<bool>(
            isSpawning,
            OnUpdateIsSpawning,
            _ => ImpNetSpawning.Instance.OnSpawningChangedServerRpc()
        );

        var content = transform;

        content.Find("Background/Text/Name").GetComponent<TMP_Text>().text = spawnObjectName;
        content.Find("Background/Text/Native").gameObject.SetActive(isNative);

        spawnChanceText = content.Find("Chance").GetComponent<TMP_Text>();

        ImpButton.Bind("Exclusive", content, OnUpdateExclusive);
        ImpToggle.Bind("Active", content, IsSpawning);
        ImpInput.Bind("Rarity", content, Rarity, min: 0, max: int.MaxValue, interactableBindings: IsSpawning);
    }

    /// <summary>
    ///     Sync changes with other entries in the same list for chance calculation and sends update to other clients
    /// </summary>
    public void SyncUpdate()
    {
        var totalRarity = otherEntries.Values.Select(entry => entry.Rarity.Value).Sum();
        otherEntries.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalRarity));

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    internal void UpdateSpawnChance(int total)
    {
        spawnChanceText.text = total == 0 ? "0%" : ImpUtils.Math.FormatChance(Rarity.Value / (float)total);
    }

    private void OnUpdateExclusive()
    {
        foreach (var entry in otherEntries.Values) entry.Rarity.Set(0, skipSync: true);
        Rarity.Set(100, true);

        ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
    }

    private void OnUpdateRarity(int value)
    {
        switch (spawnObject)
        {
            case SpawnableEnemyWithRarity entityObject:
                entityObject.rarity = value;
                break;
            case SpawnableItemWithRarity scrapObject:
                scrapObject.rarity = value;
                break;
        }
    }

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

    private int GetOriginalRarity(string objectName) =>
        entryType switch
        {
            EntryType.IndoorEntity =>
                MoonManager.Current.OriginalMoonData.IndoorEntityRarities.GetValueOrDefault(objectName),
            EntryType.OutdoorEntity =>
                MoonManager.Current.OriginalMoonData.OutdoorEntityRarities.GetValueOrDefault(objectName),
            EntryType.DaytimeEntity =>
                MoonManager.Current.OriginalMoonData.DaytimeEntityRarities.GetValueOrDefault(objectName),
            EntryType.Scrap =>
                MoonManager.Current.OriginalMoonData.ScrapRarities.GetValueOrDefault(objectName),
            _ => throw new ArgumentOutOfRangeException()
        };

    public enum EntryType
    {
        IndoorEntity,
        OutdoorEntity,
        DaytimeEntity,
        Scrap
    }
}
#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Netcode;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI.Windows;

internal class SpawnListsWindow : BaseWindow
{
    private GameObject entityListTemplate;

    private Transform indoorEntityList;
    private Transform outdoorEntityList;
    private Transform daytimeEntityList;

    private GameObject scrapListTemplate;
    private GameObject scrapList;

    private readonly Dictionary<string, SpawnListEntry> indoorEntitySpawnEntries = [];
    private readonly Dictionary<string, SpawnListEntry> outdoorEntitySpawnEntries = [];
    private readonly Dictionary<string, SpawnListEntry> daytimeEntitySpawnEntries = [];
    private readonly Dictionary<string, SpawnListEntry> scrapSpawnEntries = [];

    protected override void RegisterWindow()
    {
        ImpButton.Bind(
            "EntitySpawnListTitle/Reset", content, OnEntitySpawnsReset,
            interactableBindings: ImpNetworkManager.IsHost
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Equal", content, OnEntitySpawnsEqual,
            interactableBindings: ImpNetworkManager.IsHost
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Reset", content, MoonManager.Current.ResetScrap,
            interactableBindings: ImpNetworkManager.IsHost
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Equal", content, MoonManager.Current.EqualScrap,
            interactableBindings: ImpNetworkManager.IsHost
        );

        // Indoor category
        indoorEntityList = content.Find("EntitySpawnList/Viewport/Content/IndoorList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/IndoorTitle/Arrow", content, indoorEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/IndoorTitle/Reset",
            content,
            MoonManager.Current.ResetIndoorEntities,
            interactableBindings: ImpNetworkManager.IsHost
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/IndoorTitle/Equal",
            content,
            MoonManager.Current.EqualIndoorEntities,
            interactableBindings: ImpNetworkManager.IsHost
        );

        outdoorEntityList = content.Find("EntitySpawnList/Viewport/Content/OutdoorList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/OutdoorTitle/Arrow", content, outdoorEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/OutdoorTitle/Reset",
            content,
            MoonManager.Current.ResetOutdoorEntities,
            interactableBindings: ImpNetworkManager.IsHost
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/OutdoorTitle/Equal",
            content,
            MoonManager.Current.EqualOutdoorEntities,
            interactableBindings: ImpNetworkManager.IsHost
        );

        daytimeEntityList = content.Find("EntitySpawnList/Viewport/Content/DaytimeList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/DaytimeTitle/Arrow", content, daytimeEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/DaytimeTitle/Reset",
            content,
            MoonManager.Current.ResetDaytimeEntities,
            interactableBindings: ImpNetworkManager.IsHost
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/DaytimeTitle/Equal",
            content,
            MoonManager.Current.EqualDaytimeEntities,
            interactableBindings: ImpNetworkManager.IsHost
        );

        // Scrap category
        scrapList = content.Find("ScrapSpawnList/Viewport/Content").gameObject;

        // Item templates
        scrapListTemplate = scrapList.transform.Find("Item").gameObject;
        entityListTemplate = indoorEntityList.transform.Find("Item").gameObject;

        // Deactivate templates
        entityListTemplate.SetActive(false);
        scrapListTemplate.SetActive(false);

        Imperium.IsSceneLoaded.onUpdate += _ => Refresh();
    }

    private static void OnEntitySpawnsReset()
    {
        MoonManager.Current.ResetIndoorEntities();
        MoonManager.Current.ResetOutdoorEntities();
        MoonManager.Current.ResetDaytimeEntities();
    }

    private static void OnEntitySpawnsEqual()
    {
        MoonManager.Current.EqualIndoorEntities();
        MoonManager.Current.EqualOutdoorEntities();
        MoonManager.Current.EqualDaytimeEntities();
    }

    public void Refresh()
    {
        RefreshEntitySpawnLists();
        RefreshScrapSpawnList();
    }

    private void RefreshEntitySpawnLists()
    {
        var objectList = Imperium.ObjectManager.AllEntities.Value.OrderByDescending(
            entry => MoonManager.Current.IsEntityNative(entry.Key));

        foreach (var entry in objectList)
        {
            var entityName = entry.Key;
            var isNative = false;
            var isSpawning = false;
            Transform listParent = null;
            Dictionary<string, SpawnListEntry> spawnMap = null;
            SpawnableEnemyWithRarity entityObject = null;
            SpawnListEntry.EntryType entryType = default;

            if (Imperium.ObjectManager.AllIndoorEntities.Value.ContainsKey(entityName))
            {
                listParent = indoorEntityList;
                isNative = MoonManager.Current.IsEntityNative(entityName);
                isSpawning = MoonManager.Current.IndoorEntities[entityName].rarity > 0;
                entityObject = MoonManager.Current.IndoorEntities[entityName];
                spawnMap = indoorEntitySpawnEntries;
                entryType = SpawnListEntry.EntryType.IndoorEntity;
            }
            else if (Imperium.ObjectManager.AllOutdoorEntities.Value.ContainsKey(entityName))
            {
                listParent = outdoorEntityList;
                isNative = MoonManager.Current.IsEntityNative(entityName);
                isSpawning = MoonManager.Current.OutdoorEntities[entityName].rarity > 0;
                entityObject = MoonManager.Current.OutdoorEntities[entityName];
                spawnMap = outdoorEntitySpawnEntries;
                entryType = SpawnListEntry.EntryType.OutdoorEntity;
            }
            else if (Imperium.ObjectManager.AllDaytimeEntities.Value.ContainsKey(entityName))
            {
                listParent = daytimeEntityList;
                isNative = MoonManager.Current.IsEntityNative(entityName);
                isSpawning = MoonManager.Current.DaytimeEntities[entityName].rarity > 0;
                entityObject = MoonManager.Current.DaytimeEntities[entityName];
                spawnMap = daytimeEntitySpawnEntries;
                entryType = SpawnListEntry.EntryType.DaytimeEntity;
            }

            if (spawnMap == null)
            {
                Imperium.Output.Error($"Failed to find entity {entityName} in any spawn list!");
                return;
            }

            if (spawnMap.TryGetValue(entityName, out var existingEntry))
            {
                // Skip syncing for every entry, sync once at the end
                existingEntry.Rarity.Set(entityObject.rarity, true);
                existingEntry.IsSpawning.Set(isSpawning, true);
            }
            else
            {
                var listItem = Instantiate(entityListTemplate, listParent);
                var spawnListEntry = listItem.AddComponent<SpawnListEntry>();
                listItem.SetActive(true);
                spawnListEntry.Init(isNative, isSpawning, entityName, entityObject, spawnMap, entryType);
                spawnMap[entityName] = spawnListEntry;
            }
        }

        var totalIndoorRarity = indoorEntitySpawnEntries.Values.Select(entry => entry.Rarity.Value).Sum();
        indoorEntitySpawnEntries.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalIndoorRarity));

        var totalOutdoorRarity = outdoorEntitySpawnEntries.Values.Select(entry => entry.Rarity.Value).Sum();
        outdoorEntitySpawnEntries.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalOutdoorRarity));

        var totalDaytimeRarity = daytimeEntitySpawnEntries.Values.Select(entry => entry.Rarity.Value).Sum();
        daytimeEntitySpawnEntries.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalDaytimeRarity));
    }

    protected override void OnOpen() => Refresh();

    private void RefreshScrapSpawnList()
    {
        var objectList = Imperium.ObjectManager.AllScrap.Value.OrderByDescending(
            entry => MoonManager.Current.IsEntityNative(entry.Key));
        foreach (var entry in objectList)
        {
            var scrapName = entry.Key;
            var isNative = MoonManager.Current.IsScrapNative(scrapName);
            var scrapObject = MoonManager.Current.Scrap[scrapName];
            var isSpawning = scrapObject.rarity > 0;

            if (scrapSpawnEntries.TryGetValue(scrapName, out var existingEntry))
            {
                // Skip syncing for every entry, sync once at the end
                existingEntry.Rarity.Set(scrapObject.rarity, true);
                existingEntry.IsSpawning.Set(isSpawning, true);
            }
            else
            {
                var listItem = Instantiate(scrapListTemplate, scrapList.transform);
                var spawnListEntry = listItem.AddComponent<SpawnListEntry>();
                spawnListEntry.Init(
                    isNative, isSpawning, scrapName, scrapObject, scrapSpawnEntries, SpawnListEntry.EntryType.Scrap);
                listItem.SetActive(true);

                scrapSpawnEntries[scrapName] = spawnListEntry;
            }
        }

        var totalRarity = daytimeEntitySpawnEntries.Values.Select(entry => entry.Rarity.Value).Sum();
        scrapSpawnEntries.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalRarity));
    }
}
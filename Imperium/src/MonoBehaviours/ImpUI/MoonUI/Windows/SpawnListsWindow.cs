#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Netcode;
using Imperium.Util.Binding;
using Unity.Netcode;
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

    private readonly Dictionary<EnemyType, EntitySpawnListEntry> indoorEntitySpawnEntries = [];
    private readonly Dictionary<EnemyType, EntitySpawnListEntry> outdoorEntitySpawnEntries = [];
    private readonly Dictionary<EnemyType, EntitySpawnListEntry> daytimeEntitySpawnEntries = [];
    private readonly Dictionary<Item, ScrapSpawnListEntry> scrapSpawnEntries = [];

    protected override void RegisterWindow()
    {
        ImpButton.Bind(
            "EntitySpawnListTitle/Reset", content, OnEntitySpawnsReset,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Equal", content, OnEntitySpawnsEqual,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Reset", content, MoonManager.Current.ResetScrap,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Equal", content, MoonManager.Current.EqualScrap,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );

        // Indoor category
        indoorEntityList = content.Find("EntitySpawnList/Viewport/Content/IndoorList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/IndoorTitle/Arrow", content, indoorEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/IndoorTitle/Reset",
            content,
            MoonManager.Current.ResetIndoorEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/IndoorTitle/Equal",
            content,
            MoonManager.Current.EqualIndoorEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );

        outdoorEntityList = content.Find("EntitySpawnList/Viewport/Content/OutdoorList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/OutdoorTitle/Arrow", content, outdoorEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/OutdoorTitle/Reset",
            content,
            MoonManager.Current.ResetOutdoorEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/OutdoorTitle/Equal",
            content,
            MoonManager.Current.EqualOutdoorEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );

        daytimeEntityList = content.Find("EntitySpawnList/Viewport/Content/DaytimeList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/DaytimeTitle/Arrow", content, daytimeEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/DaytimeTitle/Reset",
            content,
            MoonManager.Current.ResetDaytimeEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/DaytimeTitle/Equal",
            content,
            MoonManager.Current.EqualDaytimeEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: themeBinding
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
        // TODO(giosuel): Activate this when implementing spawn list synchronization
        RefreshEntitySpawnLists();
        RefreshScrapSpawnList();
    }

    private void RefreshEntitySpawnLists()
    {
        var objectList = Imperium.ObjectManager.AllEntities.Value
            .OrderByDescending(entry => MoonManager.Current.IsEntityNative(entry));

        foreach (var entity in objectList)
        {
            var isNative = false;
            Transform listParent = null;
            Dictionary<EnemyType, EntitySpawnListEntry> spawnMap = null;
            SpawnableEnemyWithRarity entityWithRarity = null;
            EntitySpawnListEntry.EntityListType entityListType = default;

            if (Imperium.ObjectManager.AllIndoorEntities.Value.Contains(entity))
            {
                listParent = indoorEntityList;
                isNative = MoonManager.Current.IsEntityNative(entity);
                entityWithRarity = MoonManager.Current.Level.Enemies.Find(entry => entry.enemyType == entity);
                spawnMap = indoorEntitySpawnEntries;
                entityListType = EntitySpawnListEntry.EntityListType.IndoorEntity;
            }
            else if (Imperium.ObjectManager.AllOutdoorEntities.Value.Contains(entity))
            {
                listParent = outdoorEntityList;
                isNative = MoonManager.Current.IsEntityNative(entity);
                entityWithRarity = MoonManager.Current.Level.OutsideEnemies.Find(entry => entry.enemyType == entity);
                spawnMap = outdoorEntitySpawnEntries;
                entityListType = EntitySpawnListEntry.EntityListType.OutdoorEntity;
            }
            else if (Imperium.ObjectManager.AllDaytimeEntities.Value.Contains(entity))
            {
                listParent = daytimeEntityList;
                isNative = MoonManager.Current.IsEntityNative(entity);
                entityWithRarity = MoonManager.Current.Level.DaytimeEnemies.Find(entry => entry.enemyType == entity);
                spawnMap = daytimeEntitySpawnEntries;
                entityListType = EntitySpawnListEntry.EntityListType.DaytimeEntity;
            }

            if (spawnMap == null)
            {
                Imperium.Log.LogError($"Failed to find entity {entity.enemyName} in any spawn list!");
                return;
            }

            if (spawnMap.TryGetValue(entity, out var existingEntry))
            {
                // Skip syncing for every entry, sync once at the end
                //existingEntry.Rarity.Set(entityWithRarity.rarity, true);
                // existingEntry.IsSpawning.Set(entityWithRarity.rarity > 0, true);
            }
            else
            {
                var listItem = Instantiate(entityListTemplate, listParent);
                var spawnListEntry = listItem.AddComponent<EntitySpawnListEntry>();
                listItem.SetActive(true);
                spawnListEntry.Init(isNative, entityWithRarity, entityListType);
                spawnListEntry.Rarity.onTrigger += () =>
                {
                    var totalRarity = spawnMap.Values.Select(entry => entry.Rarity.Value).Sum();
                    spawnMap.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalRarity));

                    ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
                };
                spawnListEntry.onExclusive += () =>
                {
                    foreach (var entry in spawnMap.Values) entry.Rarity.Set(0, skipSync: true);
                };
                spawnMap[entity] = spawnListEntry;
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
        var objectList = Imperium.ObjectManager.AllScrap.Value
            .OrderByDescending(scrap => MoonManager.Current.IsScrapNative(scrap));

        foreach (var scrap in objectList)
        {
            var isNative = MoonManager.Current.IsScrapNative(scrap);
            var scrapObject = MoonManager.Current.Level.spawnableScrap.Find(entry => entry.spawnableItem = scrap);
            var isSpawning = scrapObject.rarity > 0;

            if (scrapSpawnEntries.TryGetValue(scrap, out var existingEntry))
            {
                // Skip syncing for every entry, sync once at the end
                existingEntry.Rarity.Set(scrapObject.rarity, true);
                existingEntry.IsSpawning.Set(isSpawning, true);
            }
            else
            {
                var listItem = Instantiate(scrapListTemplate, scrapList.transform);
                var spawnListEntry = listItem.AddComponent<ScrapSpawnListEntry>();
                listItem.SetActive(true);
                spawnListEntry.Init(isNative, scrapObject);
                spawnListEntry.Rarity.onTrigger += () =>
                {
                    var totalRarity = scrapSpawnEntries.Values.Select(entry => entry.Rarity.Value).Sum();
                    scrapSpawnEntries.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalRarity));

                    ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
                };
                spawnListEntry.onExclusive += () =>
                {
                    foreach (var entry in scrapSpawnEntries.Values) entry.Rarity.Set(0, skipSync: true);
                };
                scrapSpawnEntries[scrapObject.spawnableItem] = spawnListEntry;
            }
        }

        var totalRarity = daytimeEntitySpawnEntries.Values.Select(entry => entry.Rarity.Value).Sum();
        scrapSpawnEntries.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalRarity));
    }
}
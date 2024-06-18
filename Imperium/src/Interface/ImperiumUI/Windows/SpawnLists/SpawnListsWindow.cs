#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI.Windows;

internal class SpawnListsWindow : ImperiumWindow
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

    protected override void InitWindow()
    {
        ImpButton.Bind(
            "EntitySpawnListTitle/Reset", transform, OnEntitySpawnsReset,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Equal", transform, OnEntitySpawnsEqual,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Reset", transform, MoonContainer.Current.ResetScrap,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );
        ImpButton.Bind(
            "ScrapSpawnListTitle/Equal", transform, MoonContainer.Current.EqualScrap,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );

        // Indoor category
        indoorEntityList = transform.Find("EntitySpawnList/Viewport/Content/IndoorList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/IndoorTitle/Arrow", transform, indoorEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/IndoorTitle/Reset",
            transform,
            MoonContainer.Current.ResetIndoorEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/IndoorTitle/Equal",
            transform,
            MoonContainer.Current.EqualIndoorEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );

        outdoorEntityList = transform.Find("EntitySpawnList/Viewport/Content/OutdoorList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/OutdoorTitle/Arrow", transform, outdoorEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/OutdoorTitle/Reset",
            transform,
            MoonContainer.Current.ResetOutdoorEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/OutdoorTitle/Equal",
            transform,
            MoonContainer.Current.EqualOutdoorEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );

        daytimeEntityList = transform.Find("EntitySpawnList/Viewport/Content/DaytimeList");
        ImpButton.CreateCollapse("EntitySpawnList/Viewport/Content/DaytimeTitle/Arrow", transform, daytimeEntityList);
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/DaytimeTitle/Reset",
            transform,
            MoonContainer.Current.ResetDaytimeEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );
        ImpButton.Bind(
            "EntitySpawnList/Viewport/Content/DaytimeTitle/Equal",
            transform,
            MoonContainer.Current.EqualDaytimeEntities,
            interactableBindings: new ImpBinding<bool>(NetworkManager.Singleton.IsHost),
            theme: theme
        );

        // Scrap category
        scrapList = transform.Find("ScrapSpawnList/Viewport/Content").gameObject;

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
        MoonContainer.Current.ResetIndoorEntities();
        MoonContainer.Current.ResetOutdoorEntities();
        MoonContainer.Current.ResetDaytimeEntities();
    }

    private static void OnEntitySpawnsEqual()
    {
        MoonContainer.Current.EqualIndoorEntities();
        MoonContainer.Current.EqualOutdoorEntities();
        MoonContainer.Current.EqualDaytimeEntities();
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
            .OrderByDescending(entry => MoonContainer.Current.IsEntityNative(entry));

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
                isNative = MoonContainer.Current.IsEntityNative(entity);
                entityWithRarity = MoonContainer.Current.Level.Enemies.Find(entry => entry.enemyType == entity);
                spawnMap = indoorEntitySpawnEntries;
                entityListType = EntitySpawnListEntry.EntityListType.IndoorEntity;
            }
            else if (Imperium.ObjectManager.AllOutdoorEntities.Value.Contains(entity))
            {
                listParent = outdoorEntityList;
                isNative = MoonContainer.Current.IsEntityNative(entity);
                entityWithRarity = MoonContainer.Current.Level.OutsideEnemies.Find(entry => entry.enemyType == entity);
                spawnMap = outdoorEntitySpawnEntries;
                entityListType = EntitySpawnListEntry.EntityListType.OutdoorEntity;
            }
            else if (Imperium.ObjectManager.AllDaytimeEntities.Value.Contains(entity))
            {
                listParent = daytimeEntityList;
                isNative = MoonContainer.Current.IsEntityNative(entity);
                entityWithRarity = MoonContainer.Current.Level.DaytimeEnemies.Find(entry => entry.enemyType == entity);
                spawnMap = daytimeEntitySpawnEntries;
                entityListType = EntitySpawnListEntry.EntityListType.DaytimeEntity;
            }

            if (spawnMap == null)
            {
                Imperium.IO.LogError($"Failed to find entity {entity.enemyName} in any spawn list!");
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

                    // ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
                };
                spawnListEntry.onExclusive += () =>
                {
                    // foreach (var entry in spawnMap.Values) entry.Rarity.Set(0, skipSync: true);
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
            .OrderByDescending(scrap => MoonContainer.Current.IsScrapNative(scrap));

        foreach (var scrap in objectList)
        {
            var isNative = MoonContainer.Current.IsScrapNative(scrap);
            var scrapObject = MoonContainer.Current.Level.spawnableScrap.Find(entry => entry.spawnableItem = scrap);
            var isSpawning = scrapObject.rarity > 0;

            if (scrapSpawnEntries.TryGetValue(scrap, out var existingEntry))
            {
                // Skip syncing for every entry, sync once at the end
                existingEntry.Rarity.Set(scrapObject.rarity);
                existingEntry.IsSpawning.Set(isSpawning);
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

                    // ImpNetSpawning.Instance.OnSpawningChangedServerRpc();
                };
                spawnListEntry.onExclusive += () =>
                {
                    // foreach (var entry in scrapSpawnEntries.Values) entry.Rarity.Set(0, skipSync: true);
                };
                scrapSpawnEntries[scrapObject.spawnableItem] = spawnListEntry;
            }
        }

        var totalRarity = daytimeEntitySpawnEntries.Values.Select(entry => entry.Rarity.Value).Sum();
        scrapSpawnEntries.Values.ToList().ForEach(entry => entry.UpdateSpawnChance(totalRarity));
    }
}
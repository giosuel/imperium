#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.MonoBehaviours.ImpUI.VisualizationUI.ObjectVisualizerEntries;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Types;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.VisualizationUI.Windows;

internal class ObjectVisualizersWindow : BaseWindow
{
    private GameObject playerTemplate;
    private GameObject entityTemplate;

    private Transform playerList;
    private Transform entityList;

    private readonly Dictionary<int, ObjectVisualizerPlayerEntry> playerEntries = [];
    private readonly Dictionary<int, ObjectVisualizerEntityEntry> entityEntries = [];

    protected override void RegisterWindow()
    {
        playerList = content.Find("Players/Content/Viewport/Content");
        entityList = content.Find("Entities/Content/Viewport/Content");

        playerTemplate = playerList.Find("Item").gameObject;
        playerTemplate.SetActive(false);

        entityTemplate = entityList.Find("Item").gameObject;
        entityTemplate.SetActive(false);

        ImpButton.Bind(
            "PlayersHeader/Icons/Infos",
            content,
            () => TogglePlayerConfig(config => config.Info),
            theme: themeBinding,
            isIconButton: true
        );

        ImpButton.Bind(
            "EntitiesHeader/Icons/Infos",
            content,
            () => ToggleEntityConfig(config => config.Info),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/Pathfinding",
            content,
            () => ToggleEntityConfig(config => config.Pathfinding),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/Targeting",
            content,
            () => ToggleEntityConfig(config => config.Targeting),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/LineOfSight",
            content,
            () => ToggleEntityConfig(config => config.LineOfSight),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/Hearing",
            content,
            () => ToggleEntityConfig(config => config.Hearing),
            theme: themeBinding,
            isIconButton: true
        );

        Imperium.ObjectManager.CurrentPlayers.onTrigger += Refresh;
    }

    private static void TogglePlayerConfig(Func<PlayerInfoConfig, ImpBinding<bool>> configGetter)
    {
        var total = Imperium.Visualization.PlayerInfos.PlayerInfoConfigs.Count;
        var activated = Imperium.Visualization.PlayerInfos.PlayerInfoConfigs.Values.Count(
            config => configGetter(config).Value
        );

        // Set all active if at least half are inactive and vice-versa
        var setActive = activated < total / 2;
        foreach (var playerInfoConfig in Imperium.Visualization.PlayerInfos.PlayerInfoConfigs.Values)
        {
            configGetter(playerInfoConfig).Set(setActive, skipSync: true);
        }
    }

    private static void ToggleEntityConfig(Func<EntityInfoConfig, ImpBinding<bool>> configGetter)
    {
        var total = Imperium.Visualization.EntityInfos.EntityInfoConfigs.Count;
        var activated = Imperium.Visualization.EntityInfos.EntityInfoConfigs.Values.Count(
            config => configGetter(config).Value
        );

        // Set all active if at least half are inactive and vice-versa
        var setActive = activated < total / 2;
        foreach (var entityInfoConfig in Imperium.Visualization.EntityInfos.EntityInfoConfigs.Values)
        {
            configGetter(entityInfoConfig).Set(setActive, skipSync: true);
        }
    }

    protected override void OnThemeUpdate(ImpTheme theme)
    {
        ImpThemeManager.Style(
            theme,
            content,
            new StyleOverride("Players", Variant.DARKER),
            new StyleOverride("Players/Content/Scrollbar", Variant.DARKEST),
            new StyleOverride("Players/Content/Scrollbar/SlidingArea/Handle", Variant.LIGHTER),
            new StyleOverride("Entities", Variant.DARKER),
            new StyleOverride("Entities/Content/Scrollbar", Variant.DARKEST),
            new StyleOverride("Entities/Content/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );
    }

    public void Refresh()
    {
        foreach (var playerEntry in playerEntries.Values) Destroy(playerEntry.gameObject);
        playerEntries.Clear();

        foreach (var entityEntry in entityEntries.Values) Destroy(entityEntry.gameObject);
        entityEntries.Clear();

        foreach (var player in Imperium.ObjectManager.CurrentPlayers.Value)
        {
            if (playerEntries.ContainsKey(player.GetInstanceID())) continue;

            var playerEntryObject = Instantiate(playerTemplate, playerList, true);
            playerEntryObject.SetActive(true);

            var playerEntry = playerEntryObject.AddComponent<ObjectVisualizerPlayerEntry>();
            playerEntry.Init(Imperium.Visualization.PlayerInfos.PlayerInfoConfigs[player], themeBinding);

            playerEntries[player.GetInstanceID()] = playerEntry;
        }

        foreach (var entity in Resources.FindObjectsOfTypeAll<EnemyType>())
        {
            if (entityEntries.ContainsKey(entity.GetInstanceID())) continue;

            var entityEntryObject = Instantiate(entityTemplate, entityList, true);
            entityEntryObject.SetActive(true);

            var entityEntry = entityEntryObject.AddComponent<ObjectVisualizerEntityEntry>();
            entityEntry.Init(Imperium.Visualization.EntityInfos.EntityInfoConfigs[entity], themeBinding);

            entityEntries[entity.GetInstanceID()] = entityEntry;
        }
    }
}
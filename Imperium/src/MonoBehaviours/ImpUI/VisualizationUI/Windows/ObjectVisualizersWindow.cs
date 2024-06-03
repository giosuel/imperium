#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.MonoBehaviours.ImpUI.VisualizationUI.ObjectVisualizerEntries;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.VisualizationUI.Windows;

internal class ObjectVisualizersWindow : BaseWindow
{
    private GameObject objectTemplate;
    private GameObject playerTemplate;
    private GameObject entityTemplate;

    private Transform objectList;
    private Transform playerList;
    private Transform entityList;

    private readonly Dictionary<string, ObjectVisualizerObjectEntry> objectEntries = [];
    private readonly Dictionary<int, ObjectVisualizerPlayerEntry> playerEntries = [];
    private readonly Dictionary<int, ObjectVisualizerEntityEntry> entityEntries = [];

    protected override void RegisterWindow()
    {
        objectList = content.Find("Objects/Content/Viewport/Content");
        playerList = content.Find("Players/Content/Viewport/Content");
        entityList = content.Find("Entities/Content/Viewport/Content");

        objectTemplate = objectList.Find("Item").gameObject;
        objectTemplate.SetActive(false);

        playerTemplate = playerList.Find("Item").gameObject;
        playerTemplate.SetActive(false);

        entityTemplate = entityList.Find("Item").gameObject;
        entityTemplate.SetActive(false);

        ImpButton.Bind(
            "ObjectsHeader/Icons/Infos",
            content,
            ToggleObjectConfigs,
            theme: themeBinding,
            isIconButton: true
        );

        ImpButton.Bind(
            "PlayersHeader/Icons/Infos",
            content,
            () => TogglePlayerConfigs(config => config.Info),
            theme: themeBinding,
            isIconButton: true
        );

        ImpButton.Bind(
            "EntitiesHeader/Icons/Infos",
            content,
            () => ToggleEntityConfigs(config => config.Info),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/Pathfinding",
            content,
            () => ToggleEntityConfigs(config => config.Pathfinding),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/Targeting",
            content,
            () => ToggleEntityConfigs(config => config.Targeting),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/LineOfSight",
            content,
            () => ToggleEntityConfigs(config => config.LineOfSight),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/Hearing",
            content,
            () => ToggleEntityConfigs(config => config.Hearing),
            theme: themeBinding,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntitiesHeader/Icons/Custom",
            content,
            () => ToggleEntityConfigs(config => config.Custom),
            theme: themeBinding,
            isIconButton: true
        );

        Imperium.Visualization.ObjectInsights.InsightVisibilityBindings.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentPlayers.onTrigger += Refresh;
    }

    protected override void OnThemeUpdate(ImpTheme theme)
    {
        ImpThemeManager.Style(
            theme,
            content,
            new StyleOverride("Objects", Variant.DARKER),
            new StyleOverride("Objects/Content/Scrollbar", Variant.DARKEST),
            new StyleOverride("Objects/Content/Scrollbar/SlidingArea/Handle", Variant.LIGHTER),
            new StyleOverride("Players", Variant.DARKER),
            new StyleOverride("Players/Content/Scrollbar", Variant.DARKEST),
            new StyleOverride("Players/Content/Scrollbar/SlidingArea/Handle", Variant.LIGHTER),
            new StyleOverride("Entities", Variant.DARKER),
            new StyleOverride("Entities/Content/Scrollbar", Variant.DARKEST),
            new StyleOverride("Entities/Content/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );
    }

    private static void ToggleObjectConfigs()
    {
        var total = Imperium.Visualization.ObjectInsights.InsightVisibilityBindings.Value.Count;
        var activated = Imperium.Visualization.ObjectInsights.InsightVisibilityBindings.Value.Count(
            entry => entry.Value.Value
        );

        // Set all active if at least half are inactive and vice-versa
        var setActive = activated < total / 2;
        foreach (var (_, insightConfig) in Imperium.Visualization.ObjectInsights.InsightVisibilityBindings.Value)
        {
            insightConfig.Set(setActive, skipSync: true);
        }
    }

    private static void TogglePlayerConfigs(Func<PlayerInfoConfig, ImpBinding<bool>> configGetter)
    {
        var total = Imperium.Visualization.PlayerGizmos.PlayerInfoConfigs.Count;
        var activated = Imperium.Visualization.PlayerGizmos.PlayerInfoConfigs.Values.Count(
            config => configGetter(config).Value
        );

        // Set all active if at least half are inactive and vice-versa
        var setActive = activated < total / 2;
        foreach (var playerInfoConfig in Imperium.Visualization.PlayerGizmos.PlayerInfoConfigs.Values)
        {
            configGetter(playerInfoConfig).Set(setActive, skipSync: true);
        }
    }

    private static void ToggleEntityConfigs(Func<EntityInfoConfig, ImpBinding<bool>> configGetter)
    {
        var total = Imperium.Visualization.EntityGizmos.EntityInfoConfigs.Count;
        var activated = Imperium.Visualization.EntityGizmos.EntityInfoConfigs.Values.Count(
            config => configGetter(config).Value
        );

        // Set all active if at least half are inactive and vice-versa
        var setActive = activated < total / 2;
        foreach (var entityInfoConfig in Imperium.Visualization.EntityGizmos.EntityInfoConfigs.Values)
        {
            configGetter(entityInfoConfig).Set(setActive, skipSync: true);
        }
    }

    public void Refresh()
    {
        foreach (var playerEntry in playerEntries.Values) Destroy(playerEntry.gameObject);
        playerEntries.Clear();

        foreach (var entityEntry in entityEntries.Values) Destroy(entityEntry.gameObject);
        entityEntries.Clear();

        foreach (var objectEntry in objectEntries.Values) Destroy(objectEntry.gameObject);
        objectEntries.Clear();

        foreach (var player in Imperium.ObjectManager.CurrentPlayers.Value)
        {
            if (playerEntries.ContainsKey(player.GetInstanceID())) continue;

            var playerEntryObject = Instantiate(playerTemplate, playerList);
            playerEntryObject.SetActive(true);

            var playerEntry = playerEntryObject.AddComponent<ObjectVisualizerPlayerEntry>();
            playerEntry.Init(Imperium.Visualization.PlayerGizmos.PlayerInfoConfigs[player], themeBinding);

            playerEntries[player.GetInstanceID()] = playerEntry;
        }

        foreach (var entity in Resources.FindObjectsOfTypeAll<EnemyType>())
        {
            if (entityEntries.ContainsKey(entity.GetInstanceID())) continue;

            var entityEntryObject = Instantiate(entityTemplate, entityList);
            entityEntryObject.SetActive(true);

            var entityEntry = entityEntryObject.AddComponent<ObjectVisualizerEntityEntry>();
            entityEntry.Init(Imperium.Visualization.EntityGizmos.EntityInfoConfigs[entity], themeBinding);

            entityEntries[entity.GetInstanceID()] = entityEntry;
        }

        foreach (var (objectType, objectConfig) in Imperium.Visualization.ObjectInsights.InsightVisibilityBindings.Value)
        {
            var objectEntryObject = Instantiate(objectTemplate, objectList);
            objectEntryObject.SetActive(true);

            var objectEntry = objectEntryObject.AddComponent<ObjectVisualizerObjectEntry>();
            objectEntry.Init(objectType.Name, objectConfig, themeBinding);

            objectEntries[objectType.FullName ?? objectType.Name] = objectEntry;
        }

        // Register custom object entry
        var customObjectEntryObject = Instantiate(objectTemplate, entityList);
        customObjectEntryObject.SetActive(true);

        var customObjectEntry = customObjectEntryObject.AddComponent<ObjectVisualizerObjectEntry>();
        customObjectEntry.Init("Custom", Imperium.Visualization.ObjectInsights.CustomInsights, themeBinding);

        objectEntries["Special.CustomType"] = customObjectEntry;
    }
}
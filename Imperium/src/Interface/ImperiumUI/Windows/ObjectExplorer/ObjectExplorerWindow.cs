#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer;

internal class ObjectExplorerWindow : ImperiumWindow
{
    private RectTransform playerTitle;
    private TMP_Text playerCount;

    private RectTransform entityTitle;
    private TMP_Text entityCount;

    private RectTransform cruiserTitle;
    private TMP_Text cruiserCount;

    private RectTransform itemTitle;
    private TMP_Text itemCount;

    private RectTransform hazardTitle;
    private TMP_Text hazardCount;

    private RectTransform ventTitle;
    private TMP_Text ventCount;

    private RectTransform otherTitle;
    private TMP_Text otherCount;

    private RectTransform moldSporeTitle;
    private TMP_Text moldSporeCount;

    private GameObject entryTemplate;

    private ScrollRect scrollRect;
    private RectTransform contentRect;

    private readonly ImpBinding<bool> PlayersCollapsed = new(false);
    private readonly ImpBinding<bool> EntitiesCollapsed = new(false);
    private readonly ImpBinding<bool> HazardsCollapsed = new(false);
    private readonly ImpBinding<bool> VentsCollapsed = new(false);
    private readonly ImpBinding<bool> ItemsCollapsed = new(false);
    private readonly ImpBinding<bool> MoldSporesCollapsed = new(false);
    private readonly ImpBinding<bool> CompanyCruisersCollapsed = new(false);
    private readonly ImpBinding<bool> OtherCollapsed = new(false);

    private float viewHeight;
    private float contentHeight;
    private int entryCount;
    private const float entryHeight = 19;
    private const float titleCount = 8;
    private float originalViewHeight;

    private readonly List<DynamicObjectEntry> entryInstances = [];

    protected override void InitWindow()
    {
        contentRect = transform.Find("Content/Viewport/Content").GetComponent<RectTransform>();
        scrollRect = transform.Find("Content").GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);

        entryTemplate = contentRect.Find("Template").gameObject;
        entryTemplate.SetActive(false);

        playerTitle = contentRect.Find("PlayerListTitle").GetComponent<RectTransform>();
        playerCount = contentRect.Find("PlayerListTitle/Count").GetComponent<TMP_Text>();
        entityTitle = contentRect.Find("EntityListTitle").GetComponent<RectTransform>();
        entityCount = contentRect.Find("EntityListTitle/Count").GetComponent<TMP_Text>();
        cruiserTitle = contentRect.Find("CruiserListTitle").GetComponent<RectTransform>();
        cruiserCount = contentRect.Find("CruiserListTitle/Count").GetComponent<TMP_Text>();
        itemTitle = contentRect.Find("ItemListTitle").GetComponent<RectTransform>();
        itemCount = contentRect.Find("ItemListTitle/Count").GetComponent<TMP_Text>();
        hazardTitle = contentRect.Find("HazardListTitle").GetComponent<RectTransform>();
        hazardCount = contentRect.Find("HazardListTitle/Count").GetComponent<TMP_Text>();
        ventTitle = contentRect.Find("VentListTitle").GetComponent<RectTransform>();
        ventCount = contentRect.Find("VentListTitle/Count").GetComponent<TMP_Text>();
        otherTitle = contentRect.Find("OtherListTitle").GetComponent<RectTransform>();
        otherCount = contentRect.Find("OtherListTitle/Count").GetComponent<TMP_Text>();
        moldSporeTitle = contentRect.Find("VainShroudListTitle").GetComponent<RectTransform>();
        moldSporeCount = contentRect.Find("VainShroudListTitle/Count").GetComponent<TMP_Text>();

        PlayersCollapsed.onTrigger += RefreshEntries;
        EntitiesCollapsed.onTrigger += RefreshEntries;
        HazardsCollapsed.onTrigger += RefreshEntries;
        VentsCollapsed.onTrigger += RefreshEntries;
        ItemsCollapsed.onTrigger += RefreshEntries;
        MoldSporesCollapsed.onTrigger += RefreshEntries;
        CompanyCruisersCollapsed.onTrigger += RefreshEntries;
        OtherCollapsed.onTrigger += RefreshEntries;

        ImpButton.Bind(
            "PlayerListTitle/Arrow",
            contentRect,
            PlayersCollapsed,
            theme: theme,
            isIconButton: true
        );
        ImpButton.Bind(
            "EntityListTitle/Arrow",
            contentRect,
            EntitiesCollapsed,
            theme: theme,
            isIconButton: true
        );
        ImpButton.Bind(
            "CruiserListTitle/Arrow",
            contentRect,
            CompanyCruisersCollapsed,
            theme: theme,
            isIconButton: true
        );
        ImpButton.Bind(
            "ItemListTitle/Arrow",
            contentRect,
            ItemsCollapsed,
            theme: theme,
            isIconButton: true
        );
        ImpButton.Bind(
            "HazardListTitle/Arrow",
            contentRect,
            HazardsCollapsed,
            theme: theme,
            isIconButton: true
        );
        ImpButton.Bind(
            "VentListTitle/Arrow",
            contentRect,
            VentsCollapsed,
            theme: theme,
            isIconButton: true
        );
        ImpButton.Bind(
            "OtherListTitle/Arrow",
            contentRect,
            OtherCollapsed,
            theme: theme,
            isIconButton: true
        );
        ImpButton.Bind(
            "VainShroudListTitle/Arrow",
            contentRect,
            MoldSporesCollapsed,
            theme: theme,
            isIconButton: true
        );

        Imperium.ObjectManager.CurrentLevelDoors.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelSecurityDoors.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelTurrets.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelLandmines.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelSpikeTraps.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelBreakerBoxes.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelVents.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelSteamValves.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelSpiderWebs.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelEntities.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelItems.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentPlayers.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelMoldSpores.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelCompanyCruisers.onTrigger += RefreshEntries;

        InitLogList();
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            contentRect,
            new StyleOverride("PlayerListTitle", Variant.DARKER),
            new StyleOverride("EntityListTitle", Variant.DARKER),
            new StyleOverride("CruiserListTitle", Variant.DARKER),
            new StyleOverride("HazardListTitle", Variant.DARKER),
            new StyleOverride("ItemListTitle", Variant.DARKER),
            new StyleOverride("VentListTitle", Variant.DARKER),
            new StyleOverride("OtherListTitle", Variant.DARKER),
            new StyleOverride("VainShroudListTitle", Variant.DARKER)
        );

        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Content/Scrollbar", Variant.DARKEST),
            new StyleOverride("Content/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );

        ImpThemeManager.StyleText(
            themeUpdate,
            contentRect,
            new StyleOverride("PlayerListTitle/Count", Variant.FADED_TEXT),
            new StyleOverride("EntityListTitle/Count", Variant.FADED_TEXT),
            new StyleOverride("CruiserListTitle/Count", Variant.FADED_TEXT),
            new StyleOverride("HazardListTitle/Count", Variant.FADED_TEXT),
            new StyleOverride("ItemListTitle/Count", Variant.FADED_TEXT),
            new StyleOverride("VentListTitle/Count", Variant.FADED_TEXT),
            new StyleOverride("OtherListTitle/Count", Variant.FADED_TEXT),
            new StyleOverride("VainShroudListTitle/Count", Variant.FADED_TEXT)
        );
    }

    protected override void OnOpen() => RefreshEntries();

    private List<(ObjectEntryType, RectTransform)> categories;

    private List<KeyValuePair<ObjectEntryType, Component>> GetVisibleObjects() =>
        Imperium.ObjectManager.CurrentPlayers.Value
            .Where(obj => obj && !PlayersCollapsed.Value)
            .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.Player, entry))
            .Concat(Imperium.ObjectManager.CurrentLevelEntities.Value
                .Where(obj => obj && !EntitiesCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.Entity, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelCompanyCruisers.Value
                .Where(obj => obj && !CompanyCruisersCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.CompanyCruiser, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelTurrets.Value
                .Where(obj => obj && !HazardsCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.Turret, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelLandmines.Value
                .Where(obj => obj && !HazardsCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.Landmine, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelSpikeTraps.Value
                .Where(obj => obj && !HazardsCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.SpikeTrap, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelSpiderWebs.Value
                .Where(obj => obj && !HazardsCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.SpiderWeb, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelItems.Value
                .Where(obj => obj && !ItemsCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.Item, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelVents.Value
                .Where(obj => obj && !VentsCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.Vent, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelBreakerBoxes.Value
                .Where(obj => obj && !OtherCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.BreakerBox, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelSteamValves.Value
                .Where(obj => obj && !OtherCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.SteamValve, entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelMoldSpores.Value
                .Where(obj => obj && !MoldSporesCollapsed.Value)
                .Select(entry => new KeyValuePair<ObjectEntryType, Component>(ObjectEntryType.MoldSpore, entry.transform)))
            .ToList();

    private void InitLogList()
    {
        originalViewHeight = Math.Abs(scrollRect.GetComponent<RectTransform>().sizeDelta.y);
        entryCount = Mathf.CeilToInt(originalViewHeight / entryHeight) + 2;
        viewHeight = entryHeight * entryCount;

        for (var i = 0; i < entryCount; i++)
        {
            var obj = Instantiate(entryTemplate, contentRect);
            obj.gameObject.SetActive(true);
            var entry = obj.AddComponent<DynamicObjectEntry>();
            entry.InitItem(theme);
            entryInstances.Add(entry);
        }

        categories =
        [
            (ObjectEntryType.Player, playerTitle),
            (ObjectEntryType.Entity, entityTitle),
            (ObjectEntryType.CompanyCruiser, cruiserTitle),
            (ObjectEntryType.Turret, hazardTitle),
            (ObjectEntryType.Landmine, hazardTitle),
            (ObjectEntryType.SpikeTrap, hazardTitle),
            (ObjectEntryType.SpiderWeb, hazardTitle),
            (ObjectEntryType.Item, itemTitle),
            (ObjectEntryType.Vent, ventTitle),
            (ObjectEntryType.BreakerBox, otherTitle),
            (ObjectEntryType.SteamValve, otherTitle),
            (ObjectEntryType.MoldSpore, moldSporeTitle)
        ];

        RefreshEntries();
    }

    public void RefreshEntries()
    {
        var visibleObjects = GetVisibleObjects();
        var categoryEntryCounts = new Dictionary<ObjectEntryType, int>();
        var titlePositions = new Dictionary<RectTransform, float>();

        // Count the items in each category
        foreach (var (type, _) in visibleObjects)
        {
            if (!categoryEntryCounts.TryAdd(type, 1)) categoryEntryCounts[type]++;
        }

        // Position the titles based to the items present in the list
        var titleOffset = 0f;
        for (var i = 0; i < categories.Count; i++)
        {
            if (titlePositions.TryAdd(categories[i].Item2, titleOffset))
            {
                categories[i].Item2.anchoredPosition = new Vector2(0, -titleOffset);
                titleOffset += entryHeight;
            }

            var categoryEntryCount = categoryEntryCounts.GetValueOrDefault(categories[i].Item1, 0);
            titleOffset += categoryEntryCount * entryHeight;
        }

        var titlePositionValues = titlePositions.Values.ToList();

        contentHeight = entryHeight * visibleObjects.Count + titleCount * entryHeight;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);

        var position = Math.Max((1 - scrollRect.verticalNormalizedPosition) * (contentHeight - originalViewHeight), 0);

        for (var i = 0; i < entryInstances.Count; i++)
        {
            var pageOffset = i * entryHeight;
            var titlesBeforePage = titlePositionValues.Count(titlePosition => titlePosition <= pageOffset + position);

            var page = (int)(
                (position + (entryInstances.Count - i - 1) * entryHeight - titlesBeforePage * entryHeight) / viewHeight
            );
            var entryPosition = page * viewHeight + pageOffset;

            var currentPosition = entryPosition;
            var titlesBefore = 0;
            foreach (var titlePosition in titlePositionValues)
            {
                if (titlePosition < currentPosition || Mathf.Approximately(titlePosition, currentPosition))
                {
                    titlesBefore++;
                    currentPosition += entryHeight;
                }
            }

            var entryPositionAbsolute = entryPosition + titlesBefore * entryHeight;

            var index = (int)(entryPosition / entryHeight);

            if (index >= visibleObjects.Count)
            {
                entryInstances[i].ClearItem(index, entryPositionAbsolute);
            }
            else
            {
                var entryObject = visibleObjects[index];
                entryInstances[i].SetItem(entryObject.Value, entryObject.Key, tooltip, null, entryPositionAbsolute);
            }
        }
    }

    private void OnScroll(Vector2 _) => RefreshEntries();
}
#region

using System;
using System.Collections;
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
    private RectTransform playersTitle;
    private TMP_Text playersCount;

    private RectTransform entitiesTitle;
    private TMP_Text entitiesCount;

    private RectTransform cruisersTitle;
    private TMP_Text cruisersCount;

    private RectTransform itemsTitle;
    private TMP_Text itemsCount;

    private RectTransform hazardsTitle;
    private TMP_Text hazardsCount;

    private RectTransform ventsTitle;
    private TMP_Text ventsCount;

    private RectTransform othersTitle;
    private TMP_Text othersCount;

    private RectTransform vainsTitle;
    private TMP_Text vainShroudCount;

    private GameObject entryTemplate;

    private ScrollRect scrollRect;
    private RectTransform contentRect;

    private readonly ImpBinding<bool> PlayersCollapsed = new(false);
    private readonly ImpBinding<bool> EntitiesCollapsed = new(false);
    private readonly ImpBinding<bool> HazardsCollapsed = new(false);
    private readonly ImpBinding<bool> VentsCollapsed = new(false);
    private readonly ImpBinding<bool> ItemsCollapsed = new(false);
    private readonly ImpBinding<bool> VainsCollapsed = new(false);
    private readonly ImpBinding<bool> CruisersCollapsed = new(false);
    private readonly ImpBinding<bool> OtherCollapsed = new(false);

    private float viewHeight;
    private float contentHeight;
    private int entryCount;
    private const float entryHeight = 19;
    private const float titleCount = 8;
    private float originalViewHeight;

    private ObjectEntryEngine objectEntryEngine;
    private readonly List<ObjectEntry> entryInstances = [];

    private List<ObjectCategory> categoryOrder;
    private Dictionary<ObjectCategory, CategoryDefinition> objectCategories;

    private bool performRefreshNextFrame = true;
    private float previousScrollValue;

    protected override void InitWindow()
    {
        contentRect = transform.Find("Content/Viewport/Content").GetComponent<RectTransform>();
        scrollRect = transform.Find("Content").GetComponent<ScrollRect>();
        scrollRect.onValueChanged.AddListener(OnScroll);

        entryTemplate = contentRect.Find("Template").gameObject;
        entryTemplate.SetActive(false);

        playersTitle = contentRect.Find("PlayerListTitle").GetComponent<RectTransform>();
        playersCount = contentRect.Find("PlayerListTitle/Count").GetComponent<TMP_Text>();
        entitiesTitle = contentRect.Find("EntityListTitle").GetComponent<RectTransform>();
        entitiesCount = contentRect.Find("EntityListTitle/Count").GetComponent<TMP_Text>();
        cruisersTitle = contentRect.Find("CruiserListTitle").GetComponent<RectTransform>();
        cruisersCount = contentRect.Find("CruiserListTitle/Count").GetComponent<TMP_Text>();
        itemsTitle = contentRect.Find("ItemListTitle").GetComponent<RectTransform>();
        itemsCount = contentRect.Find("ItemListTitle/Count").GetComponent<TMP_Text>();
        hazardsTitle = contentRect.Find("HazardListTitle").GetComponent<RectTransform>();
        hazardsCount = contentRect.Find("HazardListTitle/Count").GetComponent<TMP_Text>();
        ventsTitle = contentRect.Find("VentListTitle").GetComponent<RectTransform>();
        ventsCount = contentRect.Find("VentListTitle/Count").GetComponent<TMP_Text>();
        othersTitle = contentRect.Find("OtherListTitle").GetComponent<RectTransform>();
        othersCount = contentRect.Find("OtherListTitle/Count").GetComponent<TMP_Text>();
        vainsTitle = contentRect.Find("VainShroudListTitle").GetComponent<RectTransform>();
        vainShroudCount = contentRect.Find("VainShroudListTitle/Count").GetComponent<TMP_Text>();

        ImpButton.CreateCollapse("PlayerListTitle/Arrow", contentRect, stateBinding: PlayersCollapsed);
        ImpButton.CreateCollapse("EntityListTitle/Arrow", contentRect, stateBinding: EntitiesCollapsed);
        ImpButton.CreateCollapse("CruiserListTitle/Arrow", contentRect, stateBinding: CruisersCollapsed);
        ImpButton.CreateCollapse("ItemListTitle/Arrow", contentRect, stateBinding: ItemsCollapsed);
        ImpButton.CreateCollapse("HazardListTitle/Arrow", contentRect, stateBinding: HazardsCollapsed);
        ImpButton.CreateCollapse("VentListTitle/Arrow", contentRect, stateBinding: VentsCollapsed);
        ImpButton.CreateCollapse("OtherListTitle/Arrow", contentRect, stateBinding: OtherCollapsed);
        ImpButton.CreateCollapse("VainShroudListTitle/Arrow", contentRect, stateBinding: VainsCollapsed);

        PlayersCollapsed.onTrigger += RefreshEntries;
        EntitiesCollapsed.onTrigger += RefreshEntries;
        HazardsCollapsed.onTrigger += RefreshEntries;
        VentsCollapsed.onTrigger += RefreshEntries;
        ItemsCollapsed.onTrigger += RefreshEntries;
        VainsCollapsed.onTrigger += RefreshEntries;
        CruisersCollapsed.onTrigger += RefreshEntries;
        OtherCollapsed.onTrigger += RefreshEntries;

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
        Imperium.ObjectManager.CurrentLevelVainShrouds.onTrigger += RefreshEntries;
        Imperium.ObjectManager.CurrentLevelCruisers.onTrigger += RefreshEntries;

        objectCategories = new Dictionary<ObjectCategory, CategoryDefinition>
        {
            { ObjectCategory.Players, new CategoryDefinition { TitleRect = playersTitle, Binding = PlayersCollapsed } },
            { ObjectCategory.Entities, new CategoryDefinition { TitleRect = entitiesTitle, Binding = EntitiesCollapsed } },
            { ObjectCategory.Cruisers, new CategoryDefinition { TitleRect = cruisersTitle, Binding = CruisersCollapsed } },
            { ObjectCategory.Hazards, new CategoryDefinition { TitleRect = hazardsTitle, Binding = HazardsCollapsed } },
            { ObjectCategory.Items, new CategoryDefinition { TitleRect = itemsTitle, Binding = ItemsCollapsed } },
            { ObjectCategory.Vents, new CategoryDefinition { TitleRect = ventsTitle, Binding = VentsCollapsed } },
            { ObjectCategory.Other, new CategoryDefinition { TitleRect = othersTitle, Binding = OtherCollapsed } },
            { ObjectCategory.Vains, new CategoryDefinition { TitleRect = vainsTitle, Binding = VainsCollapsed } },
        };

        categoryOrder =
        [
            ObjectCategory.Players,
            ObjectCategory.Entities,
            ObjectCategory.Cruisers,
            ObjectCategory.Hazards,
            ObjectCategory.Items,
            ObjectCategory.Vents,
            ObjectCategory.Other,
            ObjectCategory.Vains,
        ];

        InitEntryEngine();
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

    private void InitEntryEngine()
    {
        originalViewHeight = Math.Abs(scrollRect.GetComponent<RectTransform>().sizeDelta.y);
        entryCount = Mathf.CeilToInt(originalViewHeight / entryHeight) + 2;
        viewHeight = entryHeight * entryCount;

        for (var i = 0; i < entryCount; i++)
        {
            var obj = Instantiate(entryTemplate, contentRect);
            obj.gameObject.SetActive(true);
            var entry = obj.AddComponent<ObjectEntry>();
            entry.InitItem(theme, OnDestroyObject);
            entryInstances.Add(entry);
        }

        objectEntryEngine = new ObjectEntryEngine(objectCategories, categoryOrder);

        RefreshEntries();
    }

    public void RefreshEntries(bool useCache)
    {
        // Skip element calculation when the scroll value remains the same and cached values are used
        var currentScrollValue = scrollRect.verticalNormalizedPosition;
        if (useCache && Mathf.Approximately(currentScrollValue, previousScrollValue)) return;
        previousScrollValue = currentScrollValue;

        var (objects, categoryCounts, incrementalCategoryCounts) = objectEntryEngine.Generate(useCache);

        // Calculate title positions based on the amount of entries in each category
        var titlePositions = new List<float>();
        for (var i = 0; i < categoryOrder.Count; i++)
        {
            var titlePosition = incrementalCategoryCounts[i] * entryHeight + i * entryHeight;
            objectCategories[categoryOrder[i]].TitleRect.anchoredPosition = new Vector2(0, -titlePosition);

            titlePositions.Add(titlePosition);
        }

        // Calculate the total content height
        contentHeight = entryHeight * objects.Count + titleCount * entryHeight;
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, contentHeight);

        // Current absolute scroll value
        var position = Math.Max((1 - currentScrollValue) * (contentHeight - originalViewHeight), 0);

        for (var i = 0; i < entryInstances.Count; i++)
        {
            // Calculate instance position based on scroll value and titles above the current entry
            var pageOffset = i * entryHeight;
            var titlesBeforePage = titlePositions.Count(titlePosition =>
                titlePosition <= pageOffset + position
            );

            var page = (int)(
                (position + (entryInstances.Count - i - 1) * entryHeight - titlesBeforePage * entryHeight) / viewHeight
            );
            var entryPosition = page * viewHeight + pageOffset;

            // Calculate offset from titles above the current entry
            var currentPosition = entryPosition;
            var titlesBefore = 0;
            foreach (var titlePosition in titlePositions)
            {
                if (titlePosition <= currentPosition)
                {
                    titlesBefore++;
                    currentPosition += entryHeight;
                }
            }

            var entryPositionAbsolute = entryPosition + titlesBefore * entryHeight;

            var index = (int)(entryPosition / entryHeight);

            if (index >= objects.Count)
            {
                entryInstances[i].ClearItem(index, entryPositionAbsolute);
            }
            else
            {
                var entryObject = objects[index];
                entryInstances[i].SetItem(entryObject.Component, entryObject.Type, tooltip, entryPositionAbsolute);
            }
        }

        playersCount.text = $"({categoryCounts.GetValueOrDefault(ObjectCategory.Players, 0).ToString()})";
        entitiesCount.text = $"({categoryCounts.GetValueOrDefault(ObjectCategory.Entities, 0).ToString()})";
        cruisersCount.text = $"({categoryCounts.GetValueOrDefault(ObjectCategory.Cruisers, 0).ToString()})";
        hazardsCount.text = $"({categoryCounts.GetValueOrDefault(ObjectCategory.Hazards, 0).ToString()})";
        itemsCount.text = $"({categoryCounts.GetValueOrDefault(ObjectCategory.Items, 0).ToString()})";
        ventsCount.text = $"({categoryCounts.GetValueOrDefault(ObjectCategory.Vents, 0).ToString()})";
        othersCount.text = $"({categoryCounts.GetValueOrDefault(ObjectCategory.Other, 0).ToString()})";
        vainShroudCount.text = $"({categoryCounts.GetValueOrDefault(ObjectCategory.Vains, 0).ToString()})";
    }

    private IEnumerator waitForRefresh()
    {
        yield return 0;
        RefreshEntries();
    }

    private void Update()
    {
        if (performRefreshNextFrame)
        {
            StartCoroutine(waitForRefresh());
            performRefreshNextFrame = false;
        }
    }

    private void OnDestroyObject() => performRefreshNextFrame = true;
    public void RefreshEntries() => RefreshEntries(false);
    private void OnScroll(Vector2 _) => RefreshEntries(useCache: true);
}
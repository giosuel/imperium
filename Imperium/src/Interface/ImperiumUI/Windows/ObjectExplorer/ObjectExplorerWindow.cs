#region

using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;
using Imperium.Interface.ImperiumUI.Windows.ObjectSettings;
using Imperium.Types;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer;

internal class ObjectExplorerWindow : ImperiumWindow
{
    private RectTransform explorerTransformRect;
    private GameObject listTemplate;

    private Transform playerList;
    private TMP_Text playerCount;
    private Transform entityList;
    private TMP_Text entityCount;
    private Transform cruiserList;
    private TMP_Text cruiserCount;
    private Transform itemList;
    private TMP_Text itemCount;
    private Transform hazardList;
    private TMP_Text hazardCount;
    private Transform ventList;
    private TMP_Text ventCount;
    private Transform otherList;
    private TMP_Text otherCount;
    private Transform moldSporeList;
    private TMP_Text moldSporeCount;

    private readonly ImpTimer refreshTimer = ImpTimer.ForInterval(0.08f);

    private Dictionary<int, ObjectEntry> objectEntries = [];

    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content/Viewport/Content");

        explorerTransformRect = content.GetComponent<RectTransform>();
        listTemplate = content.Find("EntityList/Item").gameObject;
        listTemplate.SetActive(false);

        playerList = content.Find("PlayerList");
        playerCount = content.Find("PlayerListTitle/Count").GetComponent<TMP_Text>();
        entityList = content.Find("EntityList");
        entityCount = content.Find("EntityListTitle/Count").GetComponent<TMP_Text>();
        cruiserList = content.Find("CruiserList");
        cruiserCount = content.Find("CruiserListTitle/Count").GetComponent<TMP_Text>();
        itemList = content.Find("ItemList");
        itemCount = content.Find("ItemListTitle/Count").GetComponent<TMP_Text>();
        hazardList = content.Find("HazardList");
        hazardCount = content.Find("HazardListTitle/Count").GetComponent<TMP_Text>();
        ventList = content.Find("VentList");
        ventCount = content.Find("VentListTitle/Count").GetComponent<TMP_Text>();
        otherList = content.Find("OtherList");
        otherCount = content.Find("OtherListTitle/Count").GetComponent<TMP_Text>();
        moldSporeList = content.Find("VainShroudList");
        moldSporeCount = content.Find("VainShroudListTitle/Count").GetComponent<TMP_Text>();

        ImpButton.CreateCollapse("PlayerListTitle/Arrow", content, playerList);
        ImpButton.CreateCollapse("EntityListTitle/Arrow", content, entityList);
        ImpButton.CreateCollapse("ItemListTitle/Arrow", content, itemList);
        ImpButton.CreateCollapse("HazardListTitle/Arrow", content, hazardList);
        ImpButton.CreateCollapse("VentListTitle/Arrow", content, ventList);
        ImpButton.CreateCollapse("OtherListTitle/Arrow", content, otherList);
        ImpButton.CreateCollapse("VainShroudListTitle/Arrow", content, moldSporeList);

        Imperium.ObjectManager.CurrentLevelDoors.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelSecurityDoors.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelTurrets.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelLandmines.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelSpikeTraps.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelBreakerBoxes.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelVents.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelSteamValves.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelSpiderWebs.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelEntities.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelItems.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentPlayers.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelMoldSpores.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelCompanyCruisers.onTrigger += Refresh;

        if (Random.Range(0, 100) >= 99) titleBox.Find("Title").GetComponent<TMP_Text>().text = "Emporium Control Panel";
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            content,
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
            content,
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

    protected override void OnOpen() => Refresh();

    private void Update()
    {
        if (refreshTimer.Tick()) LayoutRebuilder.ForceRebuildLayoutImmediate(explorerTransformRect);
    }

    private static IEnumerable<KeyValuePair<Type, Component>> Objects =>
        Imperium.ObjectManager.CurrentLevelEntities.Value
            .Where(obj => obj != null)
            .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntryEntity), entry))
            .Concat(Imperium.ObjectManager.CurrentLevelTurrets.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntryTurret), entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelBreakerBoxes.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntryBreakerBox), entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelVents.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntryVent), entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelSteamValves.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntrySteamValve), entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelLandmines.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntryLandmine), entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelSpikeTraps.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntrySpikeTrap), entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelSpiderWebs.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntrySpiderWeb), entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelItems.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntryItem), entry)))
            .Concat(Imperium.ObjectManager.CurrentLevelMoldSpores.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntryMoldSpore), entry.transform)))
            .Concat(Imperium.ObjectManager.CurrentLevelCompanyCruisers.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectCompanyCruiser), entry)))
            .Concat(Imperium.ObjectManager.CurrentPlayers.Value
                .Where(obj => obj != null)
                .Select(entry => new KeyValuePair<Type, Component>(typeof(ObjectEntryPlayer), entry)));

    public void Refresh()
    {
        // Create a list of key value pairs where the key is the type of object entry that will be added to the
        // instantiated list entry of each object (ObjectEntryXXX, Component)
        var instanceIdSet = new HashSet<int>();

        foreach (var (type, component) in Objects)
        {
            if (!objectEntries.TryGetValue(component.GetInstanceID(), out var objectEntry))
            {
                var entryList = component switch
                {
                    EnemyAI => entityList,
                    GrabbableObject => itemList,
                    PlayerControllerB => playerList,
                    EnemyVent => ventList,
                    // Make this more clear, MoldSpores is the only thing using transform as component right now
                    Transform => moldSporeList,
                    VehicleController => cruiserList,
                    Turret or Landmine or SpikeRoofTrap or SandSpiderWebTrap or SteamValveHazard => hazardList,
                    _ => otherList
                };

                var entryObj = Instantiate(listTemplate, entryList.transform);
                entryObj.SetActive(true);
                objectEntry = (ObjectEntry)entryObj.AddComponent(type);
                objectEntry.Init(component, theme, tooltip);

                objectEntries[component.GetInstanceID()] = objectEntry;
            }
            else
            {
                objectEntry.UpdateEntry();
                instanceIdSet.Add(component.GetInstanceID());
            }

            instanceIdSet.Add(component.GetInstanceID());
        }

        // Generate new object map based on what objects were missing in the last check
        objectEntries = objectEntries
            .Where(entry => entry.Value)
            .Where(entry =>
            {
                if (!instanceIdSet.Contains(entry.Key))
                {
                    Destroy(entry.Value.gameObject);
                    return false;
                }

                return true;
            })
            .ToDictionary(entry => entry.Key, entry => entry.Value);

        var players = Imperium.StartOfRound.allPlayerScripts;
        playerCount.text = Formatting.FormatFraction(players.Count(p => !p.isPlayerDead), players.Length);

        var entities = Imperium.ObjectManager.CurrentLevelEntities.Value.Where(obj => obj != null).ToList();
        entityCount.text = Formatting.FormatFraction(
            entities.Count(p => p.gameObject.activeSelf),
            entities.Count
        );

        var companyCruisers = Objects
            .Where(entry => entry.Key == typeof(ObjectCompanyCruiser))
            .Select(entry => entry.Value)
            .ToList();
        cruiserCount.text = Formatting.FormatFraction(
            companyCruisers.Count(p => p.gameObject.activeInHierarchy),
            companyCruisers.Count
        );

        var items = Imperium.ObjectManager.CurrentLevelItems.Value.Where(obj => obj != null).ToList();
        itemCount.text = Formatting.FormatFraction(
            items.Count(p => p.gameObject.activeSelf),
            items.Count
        );
        var hazards = Objects
            .Where(entry =>
                entry.Key == typeof(ObjectEntryTurret)
                || entry.Key == typeof(ObjectEntryLandmine)
                || entry.Key == typeof(ObjectEntrySpiderWeb)
                || entry.Key == typeof(ObjectEntrySpikeTrap)
            )
            .Select(entry => entry.Value).ToList();
        hazardCount.text = Formatting.FormatFraction(
            hazards.Count(p => p.gameObject.activeInHierarchy),
            hazards.Count
        );

        var vents = Imperium.ObjectManager.CurrentLevelVents.Value.Where(obj => obj != null).ToList();
        ventCount.text = Formatting.FormatFraction(
            vents.Count(p => p.gameObject.activeInHierarchy),
            vents.Count
        );

        var other = Objects
            .Where(entry => entry.Key == typeof(ObjectEntryBreakerBox))
            .Select(entry => entry.Value)
            .ToList();
        otherCount.text = Formatting.FormatFraction(
            other.Count(p => p.gameObject.activeInHierarchy),
            other.Count
        );

        var moldSpores = Objects
            .Where(entry => entry.Key == typeof(ObjectEntryMoldSpore))
            .Select(entry => entry.Value)
            .ToList();
        moldSporeCount.text = Formatting.FormatFraction(
            moldSpores.Count(p => p.gameObject.activeInHierarchy),
            moldSpores.Count
        );
    }

    private static void OpenObjectsUI() => Imperium.Interface.Open<ObjectSettingsWindow>();
}
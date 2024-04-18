#region

using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.Windows;

internal class ObjectExplorerWindow : BaseWindow
{
    private RectTransform explorerContentRect;
    private GameObject listTemplate;

    private Transform playerList;
    private TMP_Text playerCount;
    private Transform entityList;
    private TMP_Text entityCount;
    private Transform itemList;
    private TMP_Text itemCount;
    private Transform hazardList;
    private TMP_Text hazardCount;
    private Transform ventList;
    private TMP_Text ventCount;
    private Transform otherList;
    private TMP_Text otherCount;

    private readonly ImpTimer refreshTimer = ImpTimer.ForInterval(0.2f);

    private Dictionary<int, ObjectEntry> objectEntries = [];

    protected override void RegisterWindow()
    {
        titleBox.Find("Objects").GetComponent<Button>().onClick.AddListener(OpenObjectsUI);

        explorerContentRect = content.GetComponent<RectTransform>();
        listTemplate = content.Find("EntityList/Item").gameObject;
        listTemplate.SetActive(false);

        playerList = content.Find("PlayerList");
        playerCount = content.Find("PlayerListTitle/Count").GetComponent<TMP_Text>();
        entityList = content.Find("EntityList");
        entityCount = content.Find("EntityListTitle/Count").GetComponent<TMP_Text>();
        itemList = content.Find("ItemList");
        itemCount = content.Find("ItemListTitle/Count").GetComponent<TMP_Text>();
        hazardList = content.Find("HazardList");
        hazardCount = content.Find("HazardListTitle/Count").GetComponent<TMP_Text>();
        ventList = content.Find("VentList");
        ventCount = content.Find("VentListTitle/Count").GetComponent<TMP_Text>();
        otherList = content.Find("OtherList");
        otherCount = content.Find("OtherListTitle/Count").GetComponent<TMP_Text>();

        ImpButton.CreateCollapse("PlayerListTitle/Arrow", content, playerList);
        ImpButton.CreateCollapse("EntityListTitle/Arrow", content, entityList);
        ImpButton.CreateCollapse("ItemListTitle/Arrow", content, itemList);
        ImpButton.CreateCollapse("HazardListTitle/Arrow", content, hazardList);
        ImpButton.CreateCollapse("OtherListTitle/Arrow", content, otherList);

        Imperium.ObjectManager.CurrentLevelDoors.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelSecurityDoors.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelTurrets.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelLandmines.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelSpikeTraps.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelBreakerBoxes.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelSteamleaks.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelVents.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelSpiderWebs.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelEntities.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentLevelItems.onTrigger += Refresh;
        Imperium.ObjectManager.CurrentPlayers.onTrigger += Refresh;
    }

    protected override void OnOpen() => Refresh();

    private void Update()
    {
        if (refreshTimer.Tick()) Refresh();
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
                    Turret or Landmine or SpikeRoofTrap or SandSpiderWebTrap => hazardList,
                    _ => otherList
                };

                var entryObj = Instantiate(listTemplate, entryList.transform);
                entryObj.SetActive(true);
                objectEntry = (ObjectEntry)entryObj.AddComponent(type);
                objectEntry.Init(component);

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
        playerCount.text = ImpUtils.FormatFraction(players.Count(p => !p.isPlayerDead), players.Length);

        var entities = Imperium.ObjectManager.CurrentLevelEntities.Value.Where(obj => obj != null).ToList();
        entityCount.text = ImpUtils.FormatFraction(
            entities.Count(p => p.gameObject.activeSelf),
            entities.Count
        );

        var items = Imperium.ObjectManager.CurrentLevelItems.Value.Where(obj => obj != null).ToList();
        itemCount.text = ImpUtils.FormatFraction(
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
        hazardCount.text = ImpUtils.FormatFraction(
            hazards.Count(p => p.gameObject.activeInHierarchy),
            hazards.Count
        );

        var vents = Imperium.ObjectManager.CurrentLevelVents.Value.Where(obj => obj != null).ToList();
        ventCount.text = ImpUtils.FormatFraction(
            vents.Count(p => p.gameObject.activeInHierarchy),
            vents.Count
        );

        var other = Objects
            .Where(entry => entry.Key == typeof(ObjectEntryBreakerBox))
            .Select(entry => entry.Value)
            .ToList();
        otherCount.text = ImpUtils.FormatFraction(
            other.Count(p => p.gameObject.activeInHierarchy),
            other.Count
        );

        LayoutRebuilder.ForceRebuildLayoutImmediate(explorerContentRect);
    }

    private static void OpenObjectsUI() => Imperium.Interface.Open<ObjectsUI.ObjectsUI>();
}
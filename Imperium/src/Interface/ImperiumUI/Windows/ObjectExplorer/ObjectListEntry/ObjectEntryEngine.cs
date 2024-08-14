using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Util.Binding;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntryEngine
{
    private List<int> incrementalCategoryCounts = [];
    private Dictionary<ObjectCategory, int> categoryCounts = [];
    private Dictionary<ObjectCategory, int> categoryCountsVisible = [];

    private List<ObjectEntryDefinition> entries = [];

    private readonly Dictionary<ObjectCategory, CategoryDefinition> categories;
    private readonly List<ObjectCategory> categoryOrder;

    internal ObjectEntryEngine(Dictionary<ObjectCategory, CategoryDefinition> categories,
        List<ObjectCategory> categoryOrder)
    {
        this.categories = categories;
        this.categoryOrder = categoryOrder;
    }

    internal (List<ObjectEntryDefinition>, Dictionary<ObjectCategory, int>, List<int>) Generate(bool useCache)
    {
        if (useCache && entries != null && categoryCounts != null && categoryCountsVisible != null)
        {
            return (entries, categoryCounts, incrementalCategoryCounts);
        }

        entries = [];
        categoryCounts = [];
        categoryCountsVisible = [];

        AddType(Imperium.ObjectManager.CurrentPlayers, ObjectType.Player, ObjectCategory.Players);
        AddType(Imperium.ObjectManager.CurrentLevelEntities, ObjectType.Entity, ObjectCategory.Entities);
        AddType(Imperium.ObjectManager.CurrentLevelCruisers, ObjectType.Cruiser, ObjectCategory.Cruisers);
        AddType(Imperium.ObjectManager.CurrentLevelTurrets, ObjectType.Turret, ObjectCategory.Hazards);
        AddType(Imperium.ObjectManager.CurrentLevelLandmines, ObjectType.Landmine, ObjectCategory.Hazards);
        AddType(Imperium.ObjectManager.CurrentLevelSpikeTraps, ObjectType.SpikeTrap, ObjectCategory.Hazards);
        AddType(Imperium.ObjectManager.CurrentLevelSpiderWebs, ObjectType.SpiderWeb, ObjectCategory.Hazards);
        AddType(Imperium.ObjectManager.CurrentLevelItems, ObjectType.Item, ObjectCategory.Items);
        AddType(Imperium.ObjectManager.CurrentLevelVents, ObjectType.Vent, ObjectCategory.Vents);
        AddType(Imperium.ObjectManager.CurrentLevelBreakerBoxes, ObjectType.BreakerBox, ObjectCategory.Other);
        AddType(Imperium.ObjectManager.CurrentLevelSteamValves, ObjectType.SteamValve, ObjectCategory.Other);
        AddType(
            Imperium.ObjectManager.CurrentLevelVainShrouds,
            ObjectType.VainShroud,
            ObjectCategory.Vains,
            objectGetter: obj => obj.transform
        );

        incrementalCategoryCounts = GetIncrementalCategoryCounts();
        return (entries, categoryCounts, incrementalCategoryCounts);
    }

    private List<int> GetIncrementalCategoryCounts()
    {
        var counts = new List<int>();
        var currentSum = 0;

        foreach (var objectCategory in categoryOrder)
        {
            counts.Add(currentSum);
            currentSum += categoryCountsVisible[objectCategory];
        }

        return counts;
    }

    private void AddType<T>(
        ImpBinding<IReadOnlyCollection<T>> list,
        ObjectType type,
        ObjectCategory category,
        Func<T, Component> objectGetter = null
    ) where T : Object
    {
        var typeCount = list.Value.Count;
        if (!categoryCounts.TryAdd(category, typeCount)) categoryCounts[category] += typeCount;

        var categoryHidden = categories[category].Binding.Value;
        var typeCountVisible = categoryHidden ? 0 : typeCount;
        if (!categoryCountsVisible.TryAdd(category, typeCountVisible))
        {
            categoryCountsVisible[category] += typeCountVisible;
        }

        if (!categoryHidden)
        {
            foreach (var entry in list.Value.Where(obj => obj))
            {
                entries.Add(new ObjectEntryDefinition
                {
                    Type = type,
                    Component = objectGetter?.Invoke(entry) ?? entry as Component
                });
            }
        }
    }
}

internal struct ObjectEntryDefinition
{
    internal ObjectType Type { get; init; }
    internal Component Component { get; init; }
}

internal struct CategoryDefinition
{
    internal RectTransform TitleRect { get; init; }
    internal IBinding<bool> Binding { get; init; }
}
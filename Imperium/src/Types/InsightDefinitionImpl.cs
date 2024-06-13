#region

using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Imperium.API.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Types;

internal class InsightDefinitionImpl<T> : InsightDefinition<T> where T : Component
{
    private readonly ConfigFile config;

    private readonly Dictionary<Type, InsightDefinition<Component>> globalInsights;
    private readonly ImpBinding<Dictionary<Type, ImpBinding<bool>>> insightVisibilityBindings;

    internal InsightDefinitionImpl(
        Dictionary<Type, InsightDefinition<Component>> globalInsights,
        ImpBinding<Dictionary<Type, ImpBinding<bool>>> insightVisibilityBindings,
        ConfigFile config
    )
    {
        this.config = config;
        this.globalInsights = globalInsights;
        this.insightVisibilityBindings = insightVisibilityBindings;

        InheritPropertiesFromParents();
    }

    public ImpBinding<Dictionary<string, Func<Component, string>>> Insights { get; } = new([]);
    public Func<Component, string> NameGenerator { get; private set; }
    public Func<Component, bool> IsDeadGenerator { get; private set; }
    public Func<Component, Vector3> PositionOverride { get; private set; }
    public ImpBinding<bool> VisibilityBinding { get; private set; }


    public InsightDefinition<T> SetNameGenerator(Func<T, string> generator)
    {
        NameGenerator = obj => generator((T)obj);
        PropagateNameGenerator(obj => generator((T)obj));
        return this;
    }

    public InsightDefinition<T> SetIsDeadGenerator(Func<T, bool> generator)
    {
        IsDeadGenerator = obj => generator((T)obj);
        PropagateIsDeadGenerator(obj => generator((T)obj));
        return this;
    }

    public InsightDefinition<T> SetPositionOverride(Func<T, Vector3> @override)
    {
        PositionOverride = obj => @override((T)obj);
        PropagatePositionOverride(obj => @override((T)obj));
        return this;
    }

    public InsightDefinition<T> SetConfigKey(string configKey)
    {
        VisibilityBinding = new ImpConfig<bool>(config,"Visualization.Insights", configKey, false);

        // Register possibly new binding in visibility binding list
        insightVisibilityBindings.Value[typeof(T)] = VisibilityBinding;
        insightVisibilityBindings.Refresh();
        return this;
    }

    public InsightDefinition<T> RegisterInsight(string name, Func<T, string> generator)
    {
        Insights.Value[name] = obj => generator((T)obj);

        // Refresh insights to signalize the users that an insight was added / changed
        Insights.Refresh();

        PropagateInsightRegister(name, obj => generator((T)obj));

        return this;
    }

    public InsightDefinition<T> UnregisterInsight(string name)
    {
        if (!Insights.Value.ContainsKey(name)) return this;

        Insights.Value.Remove(name);

        // Refresh insights to signalize the users that an insight was added / changed
        Insights.Refresh();

        PropagateInsightUnregister(name);

        return this;
    }

    /// <summary>
    ///     Inherits the properties from a possible parent InsightDefinition. Insights are merged and the closest generators
    ///     and overrides are picked.
    ///     Only inherits properties that aren't defined yet. Does not override insights.
    /// </summary>
    private void InheritPropertiesFromParents()
    {
        var parentTypes = Debugging.GetParentTypes<T>();
        foreach (var type in parentTypes)
        {
            if (globalInsights.TryGetValue(type, out var parentDefinition))
            {
                // Inherit parent insights that are not yet present in the dictionary
                foreach (var (parentInsightName, parentInsightGenerator) in parentDefinition.Insights.Value)
                {
                    Insights.Value.TryAdd(parentInsightName, parentInsightGenerator);
                }

                NameGenerator ??= parentDefinition.NameGenerator;
                IsDeadGenerator ??= parentDefinition.IsDeadGenerator;
                PositionOverride ??= parentDefinition.PositionOverride;

                VisibilityBinding ??= parentDefinition.VisibilityBinding;
            }
        }
    }

    /// <summary>
    ///     Propagates the removal of an insight to child types.
    /// </summary>
    private void PropagateInsightUnregister(string insightName)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                typeInsights.RemoveInsightFromParent(insightName);
            }
        }
    }

    /// <summary>
    ///     Propagates the addition of an insight to child types.
    /// </summary>
    private void PropagateInsightRegister(string insightName, Func<Component, string> insightGenerator)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                typeInsights.AddInsightFromParent(insightName, insightGenerator);
            }
        }
    }

    /// <summary>
    ///     Propagates the name generator to child types that don't have their own.
    /// </summary>
    private void PropagateNameGenerator(Func<Component, string> generator)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                typeInsights.SetNameGeneratorFromParent(generator);
            }
        }
    }

    /// <summary>
    ///     Propagates the is dead generator to child types that don't have their own.
    /// </summary>
    private void PropagateIsDeadGenerator(Func<Component, bool> generator)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                typeInsights.SetIsDeadGeneratorFromParent(generator);
            }
        }
    }

    /// <summary>
    ///     Propagates the position override to child types that don't have their own.
    /// </summary>
    private void PropagatePositionOverride(Func<Component, Vector3> @override)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                typeInsights.SetPositionOverrideFromParent(@override);
            }
        }
    }

    public void RemoveInsightFromParent(string insightName)
    {
        if (!Insights.Value.ContainsKey(insightName)) return;

        Insights.Value.Remove(insightName);
        Insights.Refresh();
    }

    public void AddInsightFromParent(string insightName, Func<Component, string> insightGenerator)
    {
        Insights.Value[insightName] = insightGenerator;
        Insights.Refresh();
    }

    public void SetNameGeneratorFromParent(Func<T, string> generator) => NameGenerator ??= obj => generator((T)obj);
    public void SetIsDeadGeneratorFromParent(Func<T, bool> generator) => IsDeadGenerator ??= obj => generator((T)obj);
    public void SetPositionOverrideFromParent(Func<T, Vector3> @override) => PositionOverride ??= obj => @override((T)obj);
}
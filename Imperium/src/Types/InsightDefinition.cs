using System;
using System.Collections.Generic;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

namespace Imperium.Types;

/// <summary>
/// Definition of an object insight.
///
/// Holds insight generators for a specific type of object alongside a few more items.
///  - Name Generator -> Function to get the name of the object
///  - IsDead Generator -> Function to get the alive status of the object
///  - Position Override -> Function to transform the object's target's position to the desired insight panel position.
///
/// </summary>
/// <typeparam name="T"></typeparam>
public interface InsightDefinition<out T> where T : Component
{
    public ImpBinding<Dictionary<string, Func<Component, string>>> Insights { get; }
    public Func<Component, string> NameGenerator { get; }
    public Func<Component, bool> IsDeadGenerator { get; }
    public Func<Component, Vector3> PositionOverride { get; }
    public ImpBinding<bool> VisibilityBinding { get; }

    public InsightDefinition<T> SetNameGenerator(Func<T, string> generator);

    public InsightDefinition<T> SetIsDeadGenerator(Func<T, bool> generator);

    public InsightDefinition<T> SetPositionOverride(Func<T, Vector3> @override);

    public InsightDefinition<T> SetConfigKey(string configKey);
    public InsightDefinition<T> RegisterInsight(string name, Func<T, string> generator);
}

public class InsightDefinitionImpl<T> : InsightDefinition<T> where T : Component
{
    private readonly Dictionary<Type, InsightDefinition<Component>> globalInsights;
    private readonly ImpBinding<Dictionary<Type, ImpBinding<bool>>> insightVisibilityBindings;

    internal InsightDefinitionImpl(
        Dictionary<Type, InsightDefinition<Component>> globalInsights,
        ImpBinding<Dictionary<Type, ImpBinding<bool>>> insightVisibilityBindings
    )
    {
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
        PropagateNameGenerator(generator);
        return this;
    }

    public InsightDefinition<T> SetIsDeadGenerator(Func<T, bool> generator)
    {
        IsDeadGenerator = obj => generator((T)obj);
        PropagateIsDeadGenerator(generator);
        return this;
    }

    public InsightDefinition<T> SetPositionOverride(Func<T, Vector3> @override)
    {
        PositionOverride = obj => @override((T)obj);
        PropagatePositionOverride(@override);
        return this;
    }

    public InsightDefinition<T> SetConfigKey(string configKey)
    {
        VisibilityBinding = new ImpConfig<bool>("Visualizers.Insights", configKey, false);

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

        PropagateInsight(name, obj => generator((T)obj));

        return this;
    }

    /// <summary>
    /// Inherits the properties from a possible parent InsightDefinition. Insights are merged and the closest generators
    /// and overrides are picked.
    ///
    /// Only inherits properties that aren't defined yet. Does not override insights.
    /// </summary>
    private void InheritPropertiesFromParents()
    {
        var parentTypes = ImpUtils.GetParentTypes<T>();
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
    /// Propagates a new insight to child types.
    /// </summary>
    private void PropagateInsight(string insightName, Func<Component, string> insightGenerator)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                ((InsightDefinitionImpl<T>)typeInsights).AddInsightFromParent(insightName, insightGenerator);
            }
        }
    }

    /// <summary>
    /// Propagates the name generator to child types that don't have their own.
    /// </summary>
    private void PropagateNameGenerator(Func<T, string> generator)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                ((InsightDefinitionImpl<T>)typeInsights).SetNameGeneratorFromParent(generator);
            }
        }
    }

    /// <summary>
    /// Propagates the is dead generator to child types that don't have their own.
    /// </summary>
    private void PropagateIsDeadGenerator(Func<T, bool> generator)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                ((InsightDefinitionImpl<T>)typeInsights).SetIsDeadGeneratorFromParent(generator);
            }
        }
    }

    /// <summary>
    /// Propagates the position override to child types that don't have their own.
    /// </summary>
    private void PropagatePositionOverride(Func<T, Vector3> @override)
    {
        foreach (var (type, typeInsights) in globalInsights)
        {
            if (type.IsSubclassOf(typeof(T)))
            {
                ((InsightDefinitionImpl<T>)typeInsights).SetPositionOverrideFromParent(@override);
            }
        }
    }

    private void AddInsightFromParent(string insightName, Func<Component, string> insightGenerator)
    {
        Insights.Value[insightName] = insightGenerator;
        Insights.Refresh();
    }

    private void SetNameGeneratorFromParent(Func<T, string> generator) => NameGenerator ??= obj => generator((T)obj);
    private void SetIsDeadGeneratorFromParent(Func<T, bool> generator) => IsDeadGenerator ??= obj => generator((T)obj);
    private void SetPositionOverrideFromParent(Func<T, Vector3> @override) => PositionOverride ??= obj => @override((T)obj);
}
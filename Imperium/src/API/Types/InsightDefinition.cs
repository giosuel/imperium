#region

using System;
using System.Collections.Generic;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.API.Types;

/// <summary>
///     Definition of an object insight.
///     Holds insight generators for a specific type of object alongside a few more specifiers.
///     - Name Generator -> Function to get the name of the object
///     - IsDead Generator -> Function to get the alive status of the object
///     - Position Override -> Function to transform the object's target's position to the desired insight panel position.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface InsightDefinition<out T> where T : Component
{
    public ImpBinding<Dictionary<string, Func<Component, string>>> Insights { get; }
    public Func<Component, string> NameGenerator { get; }
    public Func<Component, string> PersonalNameGenerator { get; }
    public Func<Component, bool> IsDeadGenerator { get; }
    public Func<Component, Vector3> PositionOverride { get; }
    public ImpBinding<bool> VisibilityBinding { get; }

    public InsightDefinition<T> RegisterInsight(string name, Func<T, string> generator);
    public InsightDefinition<T> UnregisterInsight(string name);

    public InsightDefinition<T> SetNameGenerator(Func<T, string> generator);
    public InsightDefinition<T> SetPersonalNameGenerator(Func<T, string> generator);
    public InsightDefinition<T> SetIsDeadGenerator(Func<T, bool> generator);
    public InsightDefinition<T> SetPositionOverride(Func<T, Vector3> @override);
    public InsightDefinition<T> SetConfigKey(string configKey);

    /*
     * Internal propagation functions
     */
    public void RemoveInsightFromParent(string insightName);
    public void AddInsightFromParent(string insightName, Func<Component, string> insightGenerator);
    public void SetNameGeneratorFromParent(Func<T, string> generator);
    public void SetPersonalNameGeneratorFromParent(Func<T, string> generator);
    public void SetIsDeadGeneratorFromParent(Func<T, bool> generator);
    public void SetPositionOverrideFromParent(Func<T, Vector3> @override);
}
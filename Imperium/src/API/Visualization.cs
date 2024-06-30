#region

using Imperium.API.Types;
using UnityEngine;

#endregion

namespace Imperium.API;

public static class Visualization
{
    /// <summary>
    ///     Retrieve or create the insight definition for a specific component type.
    ///     Insights are visualizations of instance fields of components in Lethal Company. They are displayed in the insight
    ///     panels that can be toggled per component type. The insight definition holds all information about the insight
    ///     panel.
    ///     Insight definitions support inheritance, meaning components that inherit from another component that already has
    ///     an insight defintions, will inherit from that.
    /// </summary>
    /// <typeparam name="T">The type of component your insight is meant for</typeparam>
    /// <returns>The existing or newly created insight definition for the given type</returns>
    public static InsightDefinition<T> InsightsFor<T>() where T : Component
    {
        APIHelpers.AssertImperiumReady();

        return Imperium.Visualization.ObjectInsights.InsightsFor<T>();
    }
}
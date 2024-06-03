using Imperium.Types;
using UnityEngine;

namespace Imperium.API.Visualization;

public static class Insights
{
    public static InsightDefinition<T> For<T>() where T : Component
    {
        if (!Imperium.IsImperiumLaunched)
        {
            throw new ImperiumAPIException(
                "Failed to execute API call. Imperium has not yet been initialized."
            );
        }

        return Imperium.Visualization.ObjectInsights.InsightsFor<T>();
    }
}


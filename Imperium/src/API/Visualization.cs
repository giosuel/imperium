using Imperium.API.Types;
using UnityEngine;

namespace Imperium.API;

public static class Visualization
{
    public static InsightDefinition<T> InsightFor<T>() where T : Component
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


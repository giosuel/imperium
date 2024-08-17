#region

using System;
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

    public static void DrawNoiseLine(EnemyAI instance, Vector3 origin)
    {
        Imperium.Visualization.EntityGizmos.NoiseVisualizerUpdate(instance, origin);
    }

    public static void DrawCone(
        EnemyAI instance,
        Transform eye,
        float coneAngle,
        float coneLength,
        Material material,
        GizmoType gizmoType = GizmoType.Custom,
        Func<Vector3> relativepositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            instance, eye, coneAngle, coneLength, material,
            gizmoType == GizmoType.Custom,
            relativepositionOverride, absolutePositionOverride
        );
    }

    public static void DrawSphere(
        EnemyAI instance,
        Transform eye,
        float radius,
        Material material,
        GizmoType gizmoType = GizmoType.Custom,
        Func<Vector3> relativepositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            instance, eye,
            radius,
            material,
            gizmoType == GizmoType.Custom,
            relativepositionOverride,
            absolutePositionOverride
        );
    }

    public enum GizmoType
    {
        LineOfSight,
        Custom
    }
}
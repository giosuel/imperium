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

    /// <summary>
    /// Draws a dynamic cone gizmo for an entity. This gizmo is positioned relative to the entity transform.
    ///
    /// This can be used to visualize line of sight for example.
    /// </summary>
    /// <param name="instance">The entity that owns the gizmo.</param>
    /// <param name="coneAngle">The angle of the cone.</param>
    /// <param name="coneLength">The length of the cone.</param>
    /// <param name="material">The material of the cone.</param>
    /// <param name="eye">The entity's eye to use as origin for the gizmo. The entity's transform is used if set to null.</param>
    /// <param name="gizmoType">The type of the gizmo. Used to get the config that is used to determine visibility.</param>
    /// <param name="gizmoDuration">The display duration of the gizmo.</param>
    /// <param name="id">The unique ID of the visualizer. If not set, the radius is used as ID.</param>
    /// <param name="relativePositionOverride">A function that overrides the relative placement of the gizmo.</param>
    /// <param name="absolutePositionOverride">A function that overrides the absolute placement of the gizmo.</param>
    public static void DrawCone(
        EnemyAI instance,
        float coneAngle,
        float coneLength,
        Material material,
        Transform eye = null,
        GizmoType gizmoType = GizmoType.LineOfSight,
        GizmoDuration gizmoDuration = GizmoDuration.AIInterval,
        int id = -1,
        Func<Vector3> relativePositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            instance, eye, coneAngle, coneLength, material, gizmoType, gizmoDuration,
            id, relativePositionOverride, absolutePositionOverride
        );
    }

    /// <summary>
    /// Draws a dynamic sphere gizmo for an entity. This gizmo is positioned relative to the entity transform.
    ///
    /// This can be used to visualize scan ranges or proximity awareness, for example.
    /// </summary>
    /// <param name="instance">The entity that owns the gizmo.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="material">The material of the sphere.</param>
    /// <param name="eye">The entity's eye to use as origin for the gizmo. The entity's transform is used if set to null.</param>
    /// <param name="gizmoType">The type of the gizmo. Used to get the config that is used to determine visibility.</param>
    /// <param name="gizmoDuration">The display duration of the gizmo.</param>
    /// <param name="id">The unique ID of the visualizer. If not set, the radius is used as ID.</param>
    /// <param name="relativePositionOverride">A function that overrides the relative placement of the gizmo.</param>
    /// <param name="absolutePositionOverride">A function that overrides the absolute placement of the gizmo.</param>
    public static void DrawSphere(
        EnemyAI instance,
        float radius,
        Material material,
        Transform eye = null,
        GizmoType gizmoType = GizmoType.LineOfSight,
        GizmoDuration gizmoDuration = GizmoDuration.AIInterval,
        int id = -1,
        Func<Vector3> relativePositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            instance, eye, radius, material, gizmoType, gizmoDuration,
            id, relativePositionOverride, absolutePositionOverride
        );
    }

    /// <summary>
    /// Draws a static sphere gizmo for an entity. This gizmo is not positioned relative to the entity transform.
    ///
    /// This can be used to visualize nest positions, for example.
    /// </summary>
    /// <param name="instance">The entity that owns the gizmo.</param>
    /// <param name="position">The position of the sphere.</param>
    /// <param name="radius">The radius of the sphere.</param>
    /// <param name="material">The material of the sphere.</param>
    /// <param name="gizmoType">The type of the gizmo. Used to get the config that is used to determine visibility.</param>
    /// <param name="gizmoDuration">The display duration of the gizmo.</param>
    /// <param name="id">The unique ID of the visualizer. If not set, the radius is used as ID.</param>
    public static void DrawStaticSphere(
        EnemyAI instance,
        Vector3 position,
        float radius,
        Material material,
        GizmoType gizmoType = GizmoType.Custom,
        GizmoDuration gizmoDuration = GizmoDuration.Indefinite,
        int id = -1
    )
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.EntityGizmos.StaticSphereVisualizerUpdate(
            instance, position, material, radius, gizmoType, gizmoDuration, id
        );
    }

    /// <summary>
    /// Draws a noise attention line from the entity to the noise origin.
    /// </summary>
    /// <param name="instance">The source entity.</param>
    /// <param name="origin">The origin of the noise.</param>
    public static void DrawNoiseLine(EnemyAI instance, Vector3 origin)
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.EntityGizmos.NoiseVisualizerUpdate(instance, origin);
    }
}
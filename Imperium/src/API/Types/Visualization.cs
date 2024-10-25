namespace Imperium.API.Types;

public enum GizmoType
{
    LineOfSight,
    Custom
}

public enum GizmoDuration
{
    /// <summary>
    /// The gizmo is shown until the entity dies. If this option is picked, the gizmo only needs to be drawn once.
    /// </summary>
    AIInterval,

    /// <summary>
    /// The gizmo is shown until the entity dies. If this option is picked, the gizmo only needs to be drawn once.
    /// </summary>
    Indefinite
}
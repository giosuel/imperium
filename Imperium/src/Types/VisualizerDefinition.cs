#region

using Imperium.Core;
using UnityEngine;

#endregion

namespace Imperium.Types;

internal record VisualizerDefinition(
    string identifier,
    IdentifierType type,
    float size,
    Visualizer visualizer,
    Material material
)
{
    public string identifier { get; } = identifier;
    public IdentifierType type { get; } = type;
    public float size { get; } = size;
    public Visualizer visualizer { get; } = visualizer;
    public Material material { get; } = material;
}

internal enum IdentifierType
{
    TAG,
    LAYER
}
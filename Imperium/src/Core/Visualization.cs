#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Oracle;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Imperium.Visualizers;
using UnityEngine;
using static UnityEngine.GameObject;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Core;

internal delegate void Visualizer(GameObject obj, string identifier, float thickness, Material material);

internal class Visualization
{
    private static Material DefaultMaterial => ImpAssets.WireframeCyanMaterial;

    internal Visualization(ImpBinding<OracleState> oracleStateBinding, ObjectManager objectManager)
    {
        // Static visualizers are updated by object lists
        LandmineIndicators = new LandmineIndicators(
            objectManager.CurrentLevelLandmines,
            ImpSettings.Visualizations.LandmineIndicators
        );
        SpikeTrapIndicators = new SpikeTrapIndicators(
            objectManager.CurrentLevelSpikeTraps,
            ImpSettings.Visualizations.SpikeTrapIndicators
        );
        VentTimers = new VentTimers(
            objectManager.CurrentLevelVents,
            ImpSettings.Visualizations.VentTimers
        );
        SpawnIndicators = new SpawnIndicators(
            oracleStateBinding,
            ImpSettings.Visualizations.SpawnIndicators
        );
        ScrapSpawns = new ScrapSpawnIndicators(
            objectManager.CurrentScrapSpawnPoints,
            ImpSettings.Visualizations.ScrapSpawns
        );

        // Weapon indicators are different as they are only updated via patches
        ShotgunIndicators = new ShotgunIndicators(ImpSettings.Visualizations.ShotgunIndicators);
        ShovelIndicators = new ShovelIndicators(ImpSettings.Visualizations.ShovelIndicators);
        KnifeIndicators = new KnifeIndicators(ImpSettings.Visualizations.KnifeIndicators);

        // Player and entity infos are separate as they have their own configs
        PlayerInfos = new PlayerInfos(objectManager.CurrentPlayers);
        EntityInfos = new EntityInfos(objectManager.CurrentLevelEntities);
    }

    // Contains all registered visualizers with their UNIQUE identifier
    private readonly Dictionary<string, VisualizerDefinition> VisualizerDefinitions = new();

    // Set of all the UNIQUE identifiers of the currently enabled visualizers
    private readonly HashSet<string> EnabledVisualizers = [];

    // Holds all the objects currently shown (unique identifier -> (instance ID -> visualizer objects))
    // Note: This dictionary will contain NULL values if objects are deleted
    private readonly Dictionary<string, Dictionary<int, GameObject>> VisualizationObjectMap = new();

    internal readonly ShotgunIndicators ShotgunIndicators;
    internal readonly ShovelIndicators ShovelIndicators;
    internal readonly KnifeIndicators KnifeIndicators;
    internal readonly LandmineIndicators LandmineIndicators;
    internal readonly SpikeTrapIndicators SpikeTrapIndicators;
    internal readonly SpawnIndicators SpawnIndicators;
    internal readonly VentTimers VentTimers;
    internal readonly PlayerInfos PlayerInfos;
    internal readonly EntityInfos EntityInfos;
    internal readonly ScrapSpawnIndicators ScrapSpawns;

    /// <summary>
    ///     Visualizes the colliders of a group of game objects by tag or layer
    ///     Can display multiple visualizers per object as long as they have DIFFERENT sizes.
    /// </summary>
    /// <param name="identifier">Tag or layer of the collider objects</param>
    /// <param name="type">If the identifier is a tag or a layer</param>
    /// <param name="thickness">Currently Unused</param>
    /// <param name="material"></param>
    /// <returns></returns>
    internal Action<bool> Collider(
        string identifier,
        IdentifierType type,
        float thickness = 0.05f,
        Material material = null
    ) => isOn => Visualize(identifier, isOn, VisualizeCollider, type, false, thickness, material);

    /// <summary>
    ///     Visualizes a group of game objects with a sphere by tag or layer
    ///     Can display multiple visualizers per object as long as they have DIFFERENT sizes.
    /// </summary>
    /// <param name="identifier">Tag or layer of the collider objects</param>
    /// <param name="type">If the identifier is a tag or a layer</param>
    /// <param name="size">Size of the indicating sphere</param>
    /// <param name="material"></param>
    /// <returns></returns>
    internal Action<bool> Point(
        string identifier,
        IdentifierType type,
        float size = 1,
        Material material = null
    ) => isOn => Visualize(identifier, isOn, VisualizePoint, type, false, size, material);

    /// <summary>
    ///     Refreshes all collider and point visualizers
    /// </summary>
    internal void RefreshOverlays()
    {
        foreach (var (uniqueIdentifier, definition) in VisualizerDefinitions)
        {
            Visualize(
                definition.identifier,
                EnabledVisualizers.Contains(uniqueIdentifier),
                definition.visualizer,
                definition.type,
                true,
                definition.size,
                definition.material
            );
        }
    }

    public static GameObject VisualizePoint(GameObject obj, float size, Material material = null, string name = null)
    {
        return ImpUtils.Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            obj.transform,
            material: material ?? DefaultMaterial,
            size,
            name: $"ImpVis_{name ?? obj.GetInstanceID().ToString()}"
        );
    }

    public static List<GameObject> VisualizeColliders(GameObject obj, Material material)
    {
        return obj.GetComponents<BoxCollider>()
            .Select(collider => VisualizeBoxCollider(collider, material))
            .Concat(
                obj.GetComponents<CapsuleCollider>()
                    .Select(collider => VisualizeCapsuleCollider(collider, material))
            )
            .ToList();
    }

    public static GameObject VisualizeBoxCollider(GameObject obj, Material material, string name = null)
    {
        return VisualizeBoxCollider(obj.GetComponents<BoxCollider>().First(), material, name);
    }

    public static GameObject VisualizeCapsuleCollider(GameObject obj, Material material, string name = null)
    {
        return VisualizeCapsuleCollider(obj.GetComponents<CapsuleCollider>().First(), material, name);
    }

    public static List<GameObject> VisualizeBoxColliders(GameObject obj, Material material, string name = null)
    {
        return obj.GetComponents<BoxCollider>()
            .Select(collider => VisualizeBoxCollider(collider, material, name))
            .ToList();
    }

    public static List<GameObject> VisualizeCapsuleColliders(GameObject obj, Material material, string name = null)
    {
        return obj.GetComponents<CapsuleCollider>()
            .Select(collider => VisualizeCapsuleCollider(collider, material, name))
            .ToList();
    }

    public static GameObject VisualizeBoxCollider(BoxCollider collider, Material material, string name = null)
    {
        if (!collider) return null;

        var visualizer = ImpUtils.Geometry.CreatePrimitive(
            PrimitiveType.Cube,
            collider.transform,
            material ?? DefaultMaterial,
            name: $"ImpVis_{name ?? collider.GetInstanceID().ToString()}"
        );

        var transform = collider.transform;
        visualizer.transform.position = transform.position;
        visualizer.transform.localPosition = collider.center;
        visualizer.transform.localScale = collider.size;
        visualizer.transform.rotation = transform.rotation;

        return visualizer;
    }

    public static GameObject VisualizeCapsuleCollider(CapsuleCollider collider, Material material, string name = null)
    {
        if (!collider) return null;

        var visualizer = ImpUtils.Geometry.CreatePrimitive(
            PrimitiveType.Capsule,
            collider.transform,
            material ?? DefaultMaterial,
            name: $"ImpVis_{name ?? collider.GetInstanceID().ToString()}"
        );

        visualizer.transform.position = collider.transform.position;
        visualizer.transform.localPosition = collider.center;
        visualizer.transform.localScale = new Vector3(collider.radius * 2, collider.height / 2, collider.radius * 2);
        visualizer.transform.rotation = collider.transform.rotation;

        return visualizer;
    }

    private void Visualize(
        string identifier,
        bool isOn,
        Visualizer visualizer,
        IdentifierType type,
        bool refresh,
        float size,
        Material material
    )
    {
        var uniqueIdentifier = $"{identifier}_{size}";

        if (!refresh)
        {
            VisualizerDefinitions[uniqueIdentifier] =
                new VisualizerDefinition(identifier, type, size, visualizer, material);
        }

        if (isOn)
        {
            EnabledVisualizers.Add(uniqueIdentifier);

            if (VisualizationObjectMap.TryGetValue(uniqueIdentifier, out var objectDict))
            {
                ImpUtils.ToggleGameObjects(objectDict.Values, true);
            }
            else
            {
                objectDict = [];
            }

            foreach (var obj in GetObjects(identifier, type))
            {
                if (!objectDict.ContainsKey(obj.GetInstanceID()))
                {
                    visualizer(obj, uniqueIdentifier, size, material);
                }
            }
        }
        else
        {
            EnabledVisualizers.Remove(uniqueIdentifier);

            if (VisualizationObjectMap.TryGetValue(uniqueIdentifier, out var objectDict))
            {
                ImpUtils.ToggleGameObjects(objectDict.Values, false);
            }
        }
    }

    private static IEnumerable<GameObject> GetObjects(string identifier, IdentifierType type)
    {
        return type switch
        {
            IdentifierType.TAG => FindGameObjectsWithTag(identifier),
            IdentifierType.LAYER => Object.FindObjectsOfType<GameObject>()
                .Where(obj => obj.layer == LayerMask.NameToLayer(identifier))
                .ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    // A cache of generated cones indexed by cone opening angle
    private readonly Dictionary<float, Mesh> ConeCache = [];

    internal Mesh GetOrGenerateCone(float angle)
    {
        if (ConeCache.TryGetValue(angle, out var cone)) return cone;

        var newCone = GenerateCone(angle);
        ConeCache[angle] = newCone;
        return newCone;
    }

    /// <summary>
    ///     Generates a unique "hash" for a line of sight visualizer for an entity.
    /// </summary>
    internal static string GenerateConeHash(Object obj, Object origin, float angle, float size)
    {
        return $"{obj.GetInstanceID()}{origin.GetInstanceID()}_{angle}_{size}";
    }

    /// <summary>
    ///     Generates a unique "hash" for a custom entity visualizer.
    /// </summary>
    internal static string GenerateSphereHash(Object obj, Object origin, float size)
    {
        return $"{obj.GetInstanceID()}{origin.GetInstanceID()}_{size}";
    }

    private const float SPHERE_RINGS_COUNT = 32f;
    private const float SPHERE_LINES_COUNT = 32f;

    /// <summary>
    ///     Generates a LOS cone mesh implementation by <see href="https://github.com/AdalynBlack/LC-EnemyDebug" /> :3
    /// </summary>
    /// <param name="angle">Angle of the generated cone</param>
    private static Mesh GenerateCone(float angle)
    {
        var coneMesh = new Mesh();

        // Ring count has to be 2 or higher, or it breaks because I don't get paid enough to fix it :D
        var ringsCount = Mathf.Max(2, (int)(SPHERE_RINGS_COUNT * (angle / 360f)) + 1);
        var vertCount = ringsCount * (int)SPHERE_LINES_COUNT + 2;
        var verts = new Vector3[vertCount];
        var indices = new int[6 * (ringsCount + 1) * (int)SPHERE_LINES_COUNT];

        // Set the centers of both ends of the cone
        verts[0] = new Vector3(0f, 0f, 1f);
        verts[vertCount - 1] = new Vector3(0f, 0f, 0f);

        for (var ring = 1; ring < (ringsCount + 1); ring++)
        {
            // Figure out where in the array to edit for this ring
            var vertOffset = (ring - 1) * (int)SPHERE_LINES_COUNT + 1;

            // Figure out the distance and size of the vertex ring
            var ringAngle = Mathf.Deg2Rad * angle * ((float)ring / ringsCount) / 2f;
            var ringDistance = Mathf.Cos(ringAngle);
            var ringSize = Mathf.Sin(ringAngle);

            for (var vert = 0; vert < SPHERE_LINES_COUNT; vert++)
            {
                // Find the angle of this vertex
                var vertAngle = -2 * Mathf.PI * (vert / SPHERE_LINES_COUNT);

                // Get the exact index to modify for this vertex
                var currentVert = vertOffset + vert;
                verts[currentVert] = new Vector3(
                    Mathf.Cos(vertAngle),
                    Mathf.Sin(vertAngle),
                    ringDistance / ringSize
                ) * ringSize;

                // Get the index in the indices array to modify for this vertex
                var indexOffset = 6 * vertOffset + vert * 6 - 3 * (int)SPHERE_LINES_COUNT;

                // Precalcualte the next vertex in the ring, accounting for wrapping
                var nextVert = (int)(vertOffset + (vert + 1) % SPHERE_LINES_COUNT);

                // If we're not on the first ring (yes I started at 1 to make the math easier)
                // Draw the triangles for the quad
                if (ring != 1)
                {
                    indices[indexOffset] = currentVert - (int)SPHERE_LINES_COUNT;
                    indices[indexOffset + 1] = nextVert;
                    indices[indexOffset + 2] = currentVert;
                    indices[indexOffset + 3] = nextVert - (int)SPHERE_LINES_COUNT;
                    indices[indexOffset + 4] = nextVert;
                    indices[indexOffset + 5] = currentVert - (int)SPHERE_LINES_COUNT;
                }
                else
                {
                    // We're on ring 1, offset our index to use 3 indices instead of 6, so we can use tris
                    indexOffset += 3 * (int)SPHERE_LINES_COUNT;
                    indexOffset /= 2;
                    // Connect to first index if we're on the innermost ring
                    indices[indexOffset] = 0;
                    indices[indexOffset + 1] = nextVert;
                    indices[indexOffset + 2] = currentVert;
                }

                if (ring == ringsCount)
                {
                    // Go forwards one layer if we're on the last ring
                    indexOffset += (int)SPHERE_LINES_COUNT * 6;
                    // Connect to last index if we're on the outermost ring
                    indices[indexOffset] = vertCount - 1;
                    indices[indexOffset + 1] = currentVert;
                    indices[indexOffset + 2] = nextVert;
                }
            }
        }

        coneMesh.SetVertices(verts.ToList());
        coneMesh.SetIndices(indices.ToList(), MeshTopology.Triangles, 0);

        return coneMesh;
    }

    private void VisualizePoint(GameObject obj, string uniqueIdentifier, float size, Material material)
    {
        if (ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)
            .ContainsKey(obj.GetInstanceID()))
            return;

        ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)[obj.GetInstanceID()] =
            VisualizePoint(obj, size, material, uniqueIdentifier);
    }

    private void VisualizeCollider(GameObject obj, string uniqueIdentifier, float thickness, Material material)
    {
        foreach (var collider in obj.GetComponents<BoxCollider>())
        {
            // Visualizer for object collider has already been created
            if (ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)
                .ContainsKey(collider.GetInstanceID())) return;

            ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)[collider.GetInstanceID()] =
                VisualizeBoxCollider(collider, material, uniqueIdentifier);
        }

        foreach (var collider in obj.GetComponents<CapsuleCollider>())
        {
            // Visualizer for object collider has already been created
            if (ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)
                .ContainsKey(collider.GetInstanceID())) return;

            ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)[collider.GetInstanceID()] =
                VisualizeCapsuleCollider(collider, material, uniqueIdentifier);
        }
    }
}
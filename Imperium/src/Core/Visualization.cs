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
    internal Visualization(ImpBinding<OracleState> oracleStateBinding, ObjectManager objectManager)
    {
        ShotgunIndicators = new ShotgunIndicators();
        ShovelIndicators = new ShovelIndicators();
        KnifeIndicators = new KnifeIndicators();
        LandmineIndicators = new LandmineIndicators(objectManager.CurrentLevelLandmines);
        SpikeTrapIndicators = new SpikeTrapIndicators(objectManager.CurrentLevelSpikeTraps);
        VentTimers = new VentTimers(objectManager.CurrentLevelVents);
        SpawnIndicators = new SpawnIndicators(oracleStateBinding);
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

    /// <summary>
    /// Visualizes the colliders of a group of game objects by tag or layer
    /// 
    /// Can display multiple visualizers per object as long as they have DIFFERENT sizes.
    /// </summary>
    /// <param name="identifier">Tag or layer of the collider objects</param>
    /// <param name="type">If the identifier is a tag or a layer</param>
    /// <param name="thickness">Currently Unused</param>
    /// <param name="material"></param>
    /// <returns></returns>
    internal Action<bool> Collider(
        string identifier,
        IdentifierType type = IdentifierType.TAG,
        float thickness = 0.05f,
        Material material = null
    ) => isOn => Visualize(identifier, isOn, VisualizeCollider, type, false, thickness, material);

    /// <summary>
    /// Visualizes a group of game objects with a sphere by tag or layer
    /// 
    /// Can display multiple visualizers per object as long as they have DIFFERENT sizes.
    /// </summary>
    /// <param name="identifier">Tag or layer of the collider objects</param>
    /// <param name="type">If the identifier is a tag or a layer</param>
    /// <param name="size">Size of the indicating sphere</param>
    /// <param name="material"></param>
    /// <returns></returns>
    internal Action<bool> Point(
        string identifier,
        IdentifierType type = IdentifierType.TAG,
        float size = 1,
        Material material = null
    ) => isOn => Visualize(identifier, isOn, VisualizePoint, type, false, size, material);

    /// <summary>
    /// Refreshes all collider and point visualizers
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

            if (!refresh)
            {
                Imperium.Output.Send(
                    $"Turned on Visualisation for {identifier}!",
                    notificationType: NotificationType.Confirmation
                );
            }
        }
        else
        {
            EnabledVisualizers.Remove(uniqueIdentifier);

            if (VisualizationObjectMap.TryGetValue(uniqueIdentifier, out var objectDict))
            {
                ImpUtils.ToggleGameObjects(objectDict.Values, false);
            }

            if (!refresh)
            {
                Imperium.Output.Send(
                    $"Turned off Visualisation for {identifier}!",
                    notificationType: NotificationType.Confirmation
                );
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

    private void VisualizePoint(GameObject obj, string uniqueIdentifier, float size, Material material)
    {
        if (ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)
            .ContainsKey(obj.GetInstanceID()))
            return;

        ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)[obj.GetInstanceID()] =
            ImpUtils.Geometry.CreatePrimitive(
                PrimitiveType.Sphere, obj.transform, material, size, name: $"ImpVis_{uniqueIdentifier}"
            );
    }

    private void VisualizeCollider(GameObject obj, string uniqueIdentifier, float thickness, Material material)
    {
        material ??= ImpAssets.WireframeCyanMaterial;

        foreach (var collider in obj.GetComponents<BoxCollider>())
        {
            // Visualizer for object collider has already been created
            if (ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)
                .ContainsKey(collider.GetInstanceID())) return;

            ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)[collider.GetInstanceID()] =
                VisualizeBoxCollider(collider, uniqueIdentifier, material);
        }

        foreach (var collider in obj.GetComponents<CapsuleCollider>())
        {
            // Visualizer for object collider has already been created
            if (ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)
                .ContainsKey(collider.GetInstanceID())) return;

            // Always use amaranth colored material for capsule colliders
            var visualizer = ImpUtils.Geometry.CreatePrimitive(
                PrimitiveType.Capsule, obj.transform, ImpAssets.WireframeAmaranthMaterial,
                name: $"ImpVis_{uniqueIdentifier}"
            );

            visualizer.transform.position = obj.transform.position;
            visualizer.transform.localPosition = collider.center;
            visualizer.transform.localScale =
                new Vector3(collider.radius * 2, collider.height / 2, collider.radius * 2);
            visualizer.transform.rotation = obj.transform.rotation;

            ImpUtils.DictionaryGetOrNew(VisualizationObjectMap, uniqueIdentifier)[collider.GetInstanceID()] =
                visualizer;
        }
    }

    internal static GameObject VisualizeBoxCollider(BoxCollider collider, string name, Material material)
    {
        var visualizer = ImpUtils.Geometry.CreatePrimitive(
            PrimitiveType.Cube, collider.transform, material, name: $"ImpVis_{name}");

        var transform = collider.transform;
        visualizer.transform.position = transform.position;
        visualizer.transform.localPosition = collider.center;
        visualizer.transform.localScale = collider.size;
        visualizer.transform.rotation = transform.rotation;

        return visualizer;
    }
}
#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GameNetcodeStuff;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Visualizers;

/// <summary>
/// Screen-space overlays containing custom insights of objects (e.g. Health, Behaviour State, Movement Speed, etc.)
/// </summary>
internal class ObjectInsights : BaseVisualizer<HashSet<Component>, ObjectInsight>
{
    internal readonly ImpBinding<Dictionary<Type, ImpBinding<bool>>> InsightVisibilityBindings = new([]);

    internal readonly ImpConfig<bool> CustomInsights = new(
        "Visualizers.ObjectInsights", "Custom", false
    );

    // Holds all the logical insights, per-type
    private readonly ImpBinding<Dictionary<Type, InsightDefinition<Component>>> registeredInsights = new([]);

    private readonly HashSet<int> insightVisualizerObjects = [];

    internal ObjectInsights()
    {
        RegisterDefaultInsights();
        Refresh();
    }

    internal void Refresh()
    {
        foreach (var obj in Object.FindObjectsOfType<GameObject>())
        {
            // Make sure visualizers aren't being added to other visualization objects
            if (insightVisualizerObjects.Contains(obj.GetInstanceID())) continue;

            foreach (var component in obj.GetComponents<Component>().Where(component => component))
            {
                var typeInsight = FindMostMatchingInsightDefinition(component.GetType());
                if (typeInsight != null)
                {
                    if (!visualizerObjects.TryGetValue(component.GetInstanceID(), out var objectInsight))
                    {
                        var objectInsightObject = new GameObject($"Imp_ObjectInsight_{obj.GetInstanceID()}");
                        objectInsightObject.transform.SetParent(obj.transform);
                        insightVisualizerObjects.Add(objectInsightObject.GetInstanceID());

                        objectInsight = objectInsightObject.AddComponent<ObjectInsight>();

                        // Find the type-specific config, or use the custom one if the specific can't be found
                        objectInsight.Init(component, typeInsight);

                        visualizerObjects[component.GetInstanceID()] = objectInsight;
                    }
                    else if (typeInsight != objectInsight.InsightDefinition)
                    {
                        // Update the insight definition if a more specific one was found
                        objectInsight.UpdateInsightDefinition(typeInsight);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Finds the most matching registered object insight definition for a given type.
    /// </summary>
    private InsightDefinition<Component> FindMostMatchingInsightDefinition(Type inputType)
    {
        foreach (var type in ImpUtils.GetParentTypes(inputType))
        {
            if (registeredInsights.Value.TryGetValue(type, out var typeInsight)) return typeInsight;
        }

        return null;
    }

    internal InsightDefinition<T> InsightsFor<T>() where T : Component
    {
        if (registeredInsights.Value.TryGetValue(typeof(T), out var insightsDefinition))
        {
            return insightsDefinition as InsightDefinition<T>;
        }

        registeredInsights.Value[typeof(T)] = new InsightDefinitionImpl<T>(
            registeredInsights.Value, InsightVisibilityBindings
        );
        registeredInsights.Refresh();

        return registeredInsights.Value[typeof(T)] as InsightDefinition<T>;
    }

    private void RegisterDefaultInsights()
    {
        // InsightsFor<Transform>()
        //     .RegisterInsight("Position", transform => ImpUtils.FormatVector(transform.position))
        //     .SetConfigKey("Objects");

        InsightsFor<PlayerControllerB>()
            .SetNameGenerator(player => player.playerUsername)
            .RegisterInsight("Health", player => $"{player.health} HP")
            .SetConfigKey("Players");

        InsightsFor<EnemyAI>()
            .SetNameGenerator(entity => entity.enemyType.enemyName)
            .RegisterInsight("Health", entity => $"{entity.enemyHP} HP")
            .RegisterInsight("Behaviour State", entity => entity.currentBehaviourStateIndex.ToString())
            .RegisterInsight("Movement Speed", entity => $"{entity.agent.speed:0.0}")
            .RegisterInsight("Stun Time", entity => $"{Math.Max(0, entity.stunNormalizedTimer):0.0}s")
            .RegisterInsight("Target", entity => entity.targetPlayer ? entity.targetPlayer.playerUsername : "-")
            .RegisterInsight("Location", ImpUtils.GetEntityLocationText)
            .SetPositionOverride(EntityPositionOverride)
            .SetConfigKey("Entities");

        InsightsFor<Turret>()
            .SetNameGenerator(turret => $"Turret (#{turret.GetInstanceID()})")
            .RegisterInsight("Is Active", turret => turret.turretActive ? "Yes" : "No")
            .RegisterInsight("Turret Mode", turret => turret.turretMode.ToString())
            .RegisterInsight("Rotation Speed", turret => turret.rotationSpeed.ToString(CultureInfo.InvariantCulture))
            .SetConfigKey("Turrets");

        InsightsFor<Landmine>()
            .SetNameGenerator(landmine => $"Landmine (#{landmine.GetInstanceID()})")
            .RegisterInsight("Has Exploded", landmine => landmine.hasExploded ? "Yes" : "No")
            .SetConfigKey("Landmines");

        InsightsFor<BridgeTrigger>()
            .SetNameGenerator(bridge => $"Bridge (#{bridge.GetInstanceID()})")
            .RegisterInsight("Durability", trigger => $"{trigger.bridgeDurability}")
            .RegisterInsight(
                "Has Fallen",
                bridge => Reflection.Get<BridgeTrigger, bool>(bridge, "hasBridgeFallen") ? "Yes" : "No"
            )
            .RegisterInsight(
                "Giant On Bridge",
                bridge => Reflection.Get<BridgeTrigger, bool>(bridge, "giantOnBridge") ? "Yes" : "No"
            )
            .SetConfigKey("Bridges");
        Imperium.Log.LogInfo($"REGISTERED OBJECTS: {registeredInsights.Value.Count}");
    }

    private readonly Dictionary<EnemyAI, Vector3?> entityColliderCache = [];

    private Vector3 EntityPositionOverride(EnemyAI entity)
    {
        if (!entityColliderCache.TryGetValue(entity, out var colliderCenter))
        {
            colliderCenter = entity.GetComponentInChildren<BoxCollider>()?.center
                             ?? entity.GetComponentInChildren<CapsuleCollider>()?.center;

            entityColliderCache[entity] = colliderCenter;
        }

        return colliderCenter.HasValue
            ? entity.transform.position
              + Vector3.up * colliderCenter.Value.y
                           * entity.transform.localScale.y
                           * 1.5f
            : entity.transform.position;
    }
}
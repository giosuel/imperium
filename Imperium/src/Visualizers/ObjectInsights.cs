#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using GameNetcodeStuff;
using Imperium.API.Types;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Visualizers;

/// <summary>
///     Screen-space overlays containing custom insights of objects (e.g. Health, Behaviour State, Movement Speed, etc.)
/// </summary>
internal class ObjectInsights : BaseVisualizer<HashSet<Component>, ObjectInsight>
{
    private readonly ConfigFile config;

    internal readonly ImpBinding<Dictionary<Type, ImpBinding<bool>>> InsightVisibilityBindings = new([]);

    internal readonly ImpConfig<bool> CustomInsights;

    // Holds all the logical insights, per-type
    private readonly ImpBinding<Dictionary<Type, InsightDefinition<Component>>> registeredInsights = new([]);

    private readonly HashSet<int> insightVisualizerObjects = [];

    internal ObjectInsights(ConfigFile config)
    {
        this.config = config;

        CustomInsights = new ImpConfig<bool>(
            config,
            "Visualization.Insights", "Custom", false
        );

        RegisterDefaultInsights();
        Refresh();

        registeredInsights.onTrigger += Refresh;
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
                        objectInsightObject.transform.SetParent(obj.transform, true);
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
    ///     Finds the most matching registered object insight definition for a given type.
    /// </summary>
    private InsightDefinition<Component> FindMostMatchingInsightDefinition(Type inputType)
    {
        foreach (var type in Debugging.GetParentTypes(inputType))
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

        var newInsightDefinition = new InsightDefinitionImpl<T>(
            registeredInsights.Value, InsightVisibilityBindings, config
        );
        registeredInsights.Value[typeof(T)] = newInsightDefinition;
        registeredInsights.Refresh();

        return newInsightDefinition;
    }

    private void RegisterDefaultInsights()
    {
        InsightsFor<PlayerControllerB>()
            .SetNameGenerator(player => player.playerUsername ?? player.GetInstanceID().ToString())
            .SetIsDeadGenerator(player => player.isPlayerDead)
            .RegisterInsight("Health", player => $"{player.health} HP")
            .RegisterInsight("Stamina", player => $"{player.sprintTime:0.0}s")
            .RegisterInsight("Visibility", player => $"{((IVisibleThreat)player).GetVisibility():0.0}")
            .RegisterInsight("Location", ImpUtils.GetPlayerLocationText)
            .SetPositionOverride(entity => DefaultPositionOverride(entity) + Vector3.up * 2f)
            .SetConfigKey("Players");

        InsightsFor<GrabbableObject>()
            .SetNameGenerator(item => item.itemProperties.itemName)
            .RegisterInsight("Value", item => $"{item.scrapValue}$")
            .RegisterInsight("Used Up", item => item.itemUsedUp ? "Yes" : "No")
            .RegisterInsight("Held By", ImpUtils.GetItemHeldByText)
            .RegisterInsight("Cooldown", item => $"{item.currentUseCooldown:0.0}s")
            .RegisterInsight("Location", ImpUtils.GetItemLocationText)
            .SetPositionOverride(DefaultPositionOverride)
            .SetConfigKey("Items");

        InsightsFor<EnemyAI>()
            .SetNameGenerator(entity => entity.enemyType.enemyName)
            .SetIsDeadGenerator(entity => entity.isEnemyDead)
            .RegisterInsight("Health", entity => $"{entity.enemyHP} HP")
            .RegisterInsight("Behaviour State", entity => entity.currentBehaviourStateIndex.ToString())
            .RegisterInsight("Movement Speed", entity => entity.agent ? $"{entity.agent.speed:0.0}" : "0")
            .RegisterInsight("Stun Timer", entity => $"{Math.Max(0, entity.stunNormalizedTimer):0.0}s")
            .RegisterInsight("Target", entity => entity.targetPlayer ? entity.targetPlayer.playerUsername : "-")
            .RegisterInsight("Location", ImpUtils.GetEntityLocationText)
            .SetPositionOverride(DefaultPositionOverride)
            .SetConfigKey("Entities");

        InsightsFor<NutcrackerEnemyAI>()
            .SetPositionOverride(entity => DefaultPositionOverride(entity) + Vector3.down * 7f);

        InsightsFor<Turret>()
            .SetNameGenerator(turret => $"Turret #{turret.GetInstanceID()}")
            .SetIsDeadGenerator(turret => !turret.turretActive)
            .RegisterInsight("Is Active", turret => turret.turretActive ? "Yes" : "No")
            .RegisterInsight("Turret Mode", turret => turret.turretMode.ToString())
            .RegisterInsight("Rotation Speed", turret => turret.rotationSpeed.ToString(CultureInfo.InvariantCulture))
            .SetPositionOverride(DefaultPositionOverride)
            .SetConfigKey("Turrets");

        InsightsFor<Landmine>()
            .SetNameGenerator(landmine => $"Landmine #{landmine.GetInstanceID()}")
            .SetIsDeadGenerator(landmine => landmine.hasExploded)
            .RegisterInsight("Has Exploded", landmine => landmine.hasExploded ? "Yes" : "No")
            .SetPositionOverride(DefaultPositionOverride)
            .SetConfigKey("Landmines");

        InsightsFor<SteamValveHazard>()
            .SetNameGenerator(steamValve => $"Steam Valve #{steamValve.GetInstanceID()}")
            .RegisterInsight("Cracked", steamValve => steamValve.valveHasCracked ? "Yes" : "No")
            .RegisterInsight("Burst", steamValve => steamValve.valveHasBurst ? "Yes" : "No")
            .RegisterInsight("Repaired", steamValve => steamValve.valveHasBeenRepaired ? "Yes" : "No")
            .RegisterInsight("Crack Timer", steamValve => $"{steamValve.valveCrackTime:0.0}s")
            .RegisterInsight("Burst Timer", steamValve => $"{steamValve.valveCrackTime:0.0}s")
            .SetPositionOverride(DefaultPositionOverride)
            .SetConfigKey("SteamValves");

        InsightsFor<VehicleController>()
            .SetNameGenerator(_ => "Company Cruiser")
            .SetIsDeadGenerator(cruiser => cruiser.carDestroyed)
            .RegisterInsight("Cruiser HP", cruiser => $"{cruiser.carHP} HP")
            .RegisterInsight("Ignition Started", cruiser => cruiser.ignitionStarted ? "Yes" : "No")
            .RegisterInsight("Movement", cruiser => Formatting.FormatVector(cruiser.moveInputVector, 1))
            .RegisterInsight("Steering", cruiser => $"{cruiser.steeringInput:0.0}")
            .RegisterInsight("Turbulence", cruiser => $"{cruiser.turbulenceAmount:0.0}")
            .RegisterInsight("Stress", cruiser => $"{cruiser.carStress:0.0}")
            .SetPositionOverride(DefaultPositionOverride)
            .SetConfigKey("CompanyCruiser");

        InsightsFor<BridgeTrigger>()
            .SetNameGenerator(bridge => $"Bridge #{bridge.GetInstanceID()}")
            .SetIsDeadGenerator(bridge => bridge.hasBridgeFallen)
            .RegisterInsight("Durability", trigger => $"{trigger.bridgeDurability}")
            .RegisterInsight(
                "Has Fallen",
                bridge => Reflection.Get<BridgeTrigger, bool>(bridge, "hasBridgeFallen") ? "Yes" : "No"
            )
            .RegisterInsight(
                "Giant On Bridge",
                bridge => Reflection.Get<BridgeTrigger, bool>(bridge, "giantOnBridge") ? "Yes" : "No"
            )
            .SetPositionOverride(DefaultPositionOverride)
            .SetConfigKey("Bridges");
    }

    /*
     * Default position override places the insight panel at 3/4 the hight of the object, based on the
     * first child collider found.
     */
    private readonly Dictionary<int, Vector3?> entityColliderCache = [];

    private Vector3 DefaultPositionOverride(Component obj)
    {
        if (!entityColliderCache.TryGetValue(obj.GetInstanceID(), out var colliderCenter))
        {
            colliderCenter = obj.GetComponentInChildren<BoxCollider>()?.center
                             ?? obj.GetComponentInChildren<CapsuleCollider>()?.center;

            entityColliderCache[obj.GetInstanceID()] = colliderCenter;
        }

        return colliderCenter.HasValue
            ? obj.transform.position
              + Vector3.up * colliderCenter.Value.y
                           * obj.transform.localScale.y
                           * 1.5f
            : obj.transform.position;
    }
}
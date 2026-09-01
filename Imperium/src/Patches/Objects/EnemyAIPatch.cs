#region

using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyAI))]
internal static class EnemyAIPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("KillEnemy")]
    private static void KillEnemyPatch(EnemyAI __instance)
    {
        Imperium.ObjectManager.CurrentLevelEntities.Refresh();
        Imperium.IO.LogDebug("[ORACLE] Oracle has to resimulate to entity killed");
        Imperium.Oracle.Resimulate(
            $"Entity {Imperium.ObjectManager.GetDisplayName(__instance.enemyType.enemyName)} was killed."
        );
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    private static void StartPostfixPatch(EnemyAI __instance)
    {
        Imperium.EventLog.EntityEvents.Start(__instance);
    }

    [HarmonyPrefix]
    [HarmonyPatch("SwitchToBehaviourState")]
    private static void SwitchToBehaviourStatePrefixPatch(EnemyAI __instance, int stateIndex)
    {
        Imperium.EventLog.EntityEvents.SwitchBehaviourState(__instance, __instance.currentBehaviourStateIndex, stateIndex);
    }

    [HarmonyPostfix]
    [HarmonyPatch("TargetClosestPlayer")]
    private static void TargetClosestPlayerPostfixPatch(EnemyAI __instance, bool __result)
    {
        if (__result) Imperium.EventLog.EntityEvents.TargetClosestPlayer(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch("PlayerIsTargetable")]
    private static void PlayerIsTargetablePatch(
        EnemyAI __instance, PlayerControllerB playerScript, ref bool __result
    )
    {
        if (Imperium.PlayerManager.untargetablePlayers.Value.Contains(playerScript.actualClientId))
        {
            __result = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(
        "GetAllPlayersInLineOfSightNonAlloc",
        typeof(float), typeof(int), typeof(Transform), typeof(float), typeof(int)
    )]
    private static void GetAllPlayersInLineOfSightNonAlloc1PostfixPatch(
        EnemyAI __instance, float width, int range, Transform eyeObject, float proximityCheck
    )
    {
        // Line of sight visualization
        var coneSize = range;

        if (__instance.isOutside
            && !__instance.enemyType.canSeeThroughFog
            && Imperium.TimeOfDay.currentLevelWeather == LevelWeatherType.Foggy)
        {
            coneSize = Mathf.Clamp(range, 0, 30);
        }

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            eyeObject ? eyeObject : __instance.eye,
            width,
            coneSize,
            material: ImpAssets.WireframeRed
        );

        if (proximityCheck > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                null,
                proximityCheck,
                material: ImpAssets.WireframeRed
            );
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(
        "GetAllPlayersInLineOfSightNonAlloc",
        typeof(PlayerControllerB[]), typeof(float), typeof(int), typeof(Transform), typeof(float), typeof(int)
    )]
    private static void GetAllPlayersInLineOfSightNonAlloc2PostfixPatch(
        EnemyAI __instance, PlayerControllerB[] playersArray,
        float width, int range, Transform eyeObject, float proximityCheck
    )
    {
        // Line of sight visualization
        var coneSize = range;

        if (__instance.isOutside
            && !__instance.enemyType.canSeeThroughFog
            && Imperium.TimeOfDay.currentLevelWeather == LevelWeatherType.Foggy)
        {
            coneSize = Mathf.Clamp(range, 0, 30);
        }

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            eyeObject ? eyeObject : __instance.eye,
            width,
            coneSize,
            material: ImpAssets.WireframeRed
        );

        if (proximityCheck > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                null,
                proximityCheck,
                material: ImpAssets.WireframeRed
            );
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("GetAllPlayersInLineOfSight")]
    private static void GetAllPlayersInLineOfSightPostfixPatch(
        EnemyAI __instance, float width, int range, Transform eyeObject, float proximityCheck,
        ref PlayerControllerB[] __result
    )
    {
        // Remove all invisible players from result list
        __result = __result?
            .Where(player => !Imperium.PlayerManager.invisiblePlayers.Value.Contains(player.actualClientId))
            .ToArray();

        // Line of sight visualization
        var coneSize = range;

        if (__instance.isOutside
            && !__instance.enemyType.canSeeThroughFog
            && Imperium.TimeOfDay.currentLevelWeather == LevelWeatherType.Foggy)
        {
            coneSize = Mathf.Clamp(range, 0, 30);
        }

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            eyeObject ? eyeObject : __instance.eye,
            width,
            coneSize,
            material: ImpAssets.WireframePurple
        );

        if (proximityCheck > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                null,
                proximityCheck,
                material: ImpAssets.WireframePurple
            );
        }

        if (__result != null) Imperium.EventLog.EntityEvents.GetAllPlayersInLineOfSight(__instance, __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSight")]
    private static void CheckLineOfSightPostfixPatch(
        EnemyAI __instance, List<GameObject> objectsToLookFor, float width, int range, float proximityAwareness,
        Transform useEye
    )
    {
        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            useEye,
            width,
            range,
            material: ImpAssets.WireframeRed
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                __instance.transform,
                proximityAwareness,
                material: ImpAssets.WireframeRed
            );
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPrefixPatch(
        EnemyAI __instance, float width, int range, int proximityAwareness, ref PlayerControllerB __result
    )
    {
        // If target player is invisible, set to null and skip LOS visualization
        if (__result && Imperium.PlayerManager.invisiblePlayers.Value.Contains(__result.playerClientId))
        {
            __result = null;
            return;
        }

        if (__result != null) Imperium.EventLog.EntityEvents.CheckLineOfSightForPlayer(__instance, __result);

        var coneSize = range;

        if (__instance.isOutside
            && !__instance.enemyType.canSeeThroughFog
            && Imperium.TimeOfDay.currentLevelWeather == LevelWeatherType.Foggy)
        {
            coneSize = Mathf.Clamp(range, 0, 30);
        }

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            width,
            coneSize,
            material: ImpAssets.WireframeCyan
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                proximityAwareness,
                material: ImpAssets.WireframeCyan
            );
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForPosition")]
    private static void CheckLineOfSightForPositionPrefixPatch(
        EnemyAI __instance, Vector3 objectPosition, float width, int range,
        float proximityAwareness, Transform overrideEye
    )
    {
        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            overrideEye ? overrideEye : __instance.eye,
            width,
            range,
            material: ImpAssets.WireframeYellow
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                proximityAwareness,
                material: ImpAssets.WireframeYellow
            );
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPrefixPatch(
        EnemyAI __instance, float width, int range, int proximityAwareness, ref PlayerControllerB __result
    )
    {
        // If target player is invisible, set to null and skip LOS visualization
        if (__result && Imperium.PlayerManager.invisiblePlayers.Value.Contains(__result.playerClientId))
        {
            __result = null;
            return;
        }

        if (__result != null) Imperium.EventLog.EntityEvents.CheckLineOfSightForClosestPlayer(__instance, __result);

        var coneSize = range * 2;

        if (__instance.isOutside
            && !__instance.enemyType.canSeeThroughFog
            && Imperium.TimeOfDay.currentLevelWeather == LevelWeatherType.Foggy)
        {
            coneSize = Mathf.Clamp(range, 0, 30);
        }

        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            null,
            width,
            coneSize,
            material: ImpAssets.WireframeAmaranth
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                proximityAwareness,
                material: ImpAssets.WireframeAmaranth
            );
        }
    }
}
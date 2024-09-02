#region

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
    [HarmonyPatch("PlayerIsTargetable")]
    private static void PlayerIsTargetablePatch(
        EnemyAI __instance, PlayerControllerB playerScript, ref bool __result
    )
    {
        if (Imperium.Settings.Player.Untargetable.Value && playerScript == Imperium.Player)
        {
            __result = false;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("KillEnemy")]
    private static void KillEnemyPatch(EnemyAI __instance)
    {
        Imperium.ObjectManager.CurrentLevelEntities.Refresh();
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
    [HarmonyPatch("GetAllPlayersInLineOfSight")]
    private static void GetAllPlayersInLineOfSightPostfixPatch(
        EnemyAI __instance, float width, int range, Transform eyeObject, float proximityCheck, PlayerControllerB[] __result
    )
    {
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

    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPrefixPatch(
        EnemyAI __instance, float width, int range, int proximityAwareness
    )
    {
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

    [HarmonyPrefix]
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

    /// <summary>
    ///     Temporarily removes invisible player from allPlayerScripts
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPrefixPatch(
        EnemyAI __instance, float width, int range, int proximityAwareness
    )
    {
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

    [HarmonyPrefix]
    [HarmonyPatch("MeetsStandardPlayerCollisionConditions")]
    private static void MeetsStandardPlayerCollisionConditionsPrefix()
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("MeetsStandardPlayerCollisionConditions")]
    private static void MeetsStandardPlayerCollisionConditionsPostfix()
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    private static void UpdatePrefixPatch(EnemyAI __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePostfixPatch()
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }

    /*
     * Player Invisibility Patches
     */
    [HarmonyPostfix]
    [HarmonyPatch("GetAllPlayersInLineOfSight")]
    private static void GetAllPlayersInLineOfSightPostfixPatch(EnemyAI __instance, ref PlayerControllerB[] __result)
    {
        if (Imperium.Settings.Player.Invisibility.Value && __result != null && __result.Contains(Imperium.Player))
        {
            __result = __result.Where(player => player != Imperium.Player).ToArray();
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPostfixPatch(EnemyAI __instance, ref PlayerControllerB __result)
    {
        if (Imperium.Settings.Player.Invisibility.Value && __result == Imperium.Player) __result = null;

        if (__result != null) Imperium.EventLog.EntityEvents.CheckLineOfSightForPlayer(__instance, __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPostfixPatch(EnemyAI __instance, ref PlayerControllerB __result)
    {
        if (Imperium.Settings.Player.Invisibility.Value && __result == Imperium.Player) __result = null;

        if (__result != null) Imperium.EventLog.EntityEvents.CheckLineOfSightForClosestPlayer(__instance, __result);
    }

    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForPosition")]
    private static void CheckLineOfSightForPositionPostfixPatch(
        EnemyAI __instance, Vector3 objectPosition, ref bool __result
    )
    {
        if (
            Imperium.Settings.Player.Invisibility.Value &&
            objectPosition == Imperium.Player.gameplayCamera.transform.position
        )
        {
            __result = false;
        }

        if (__result) Imperium.EventLog.EntityEvents.CheckLineOfSightForPosition(__instance, objectPosition);
    }
}
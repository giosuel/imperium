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
    // [HarmonyPostfix]
    // [HarmonyPatch("PlayerIsTargetable")]
    // private static bool PlayerIsTargetablePatch(
    //     EnemyAI __instance, PlayerControllerB playerScript, ref bool __result
    // )
    // {
    //     if (!Imperium.IsImperiumLoaded) return true;
    //
    //     if (playerScript == Imperium.Player && Imperium.Settings.Player.Invisibility.Value)
    //     {
    //         __result = false;
    //         return false;
    //     }
    //
    //     return true;
    // }

    [HarmonyPostfix]
    [HarmonyPatch("KillEnemy")]
    private static void KillEnemyPatch(EnemyAI __instance)
    {
        Imperium.ObjectManager.CurrentLevelEntities.Refresh();
        Imperium.Oracle.Simulate(
            $"Entity {Imperium.ObjectManager.GetDisplayName(__instance.enemyType.enemyName)} was killed."
        );
    }

    [HarmonyPrefix]
    [HarmonyPatch("GetAllPlayersInLineOfSight")]
    private static void GetAllPlayersInLineOfSightPrefix(
        EnemyAI __instance, float width, int range, Transform eyeObject, float proximityCheck
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
                proximityCheck * 2,
                material: ImpAssets.WireframePurple
            );
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSight")]
    private static void CheckLineOfSightPrefixPatch(
        EnemyAI __instance, List<GameObject> objectsToLookFor, float width, int range, float proximityAwareness
    )
    {
        Imperium.Visualization.EntityGizmos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            width,
            range,
            material: ImpAssets.WireframeRed
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                __instance.transform,
                proximityAwareness * 2,
                material: ImpAssets.WireframeRed
            );
        }
    }

    /// <summary>
    ///     Temporarily removes invisible player from allPlayerScripts
    /// </summary>
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
                proximityAwareness * 2,
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
        // if (!__instance.isOutside && objectPosition.y > -80f || objectPosition.y < -100f)
        // {
        //     return;
        // }

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
                proximityAwareness * 2,
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
        var coneSize = range;

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
                proximityAwareness * 2,
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

    [HarmonyPrefix]
    [HarmonyPatch("GetAllPlayersInLineOfSight")]
    private static void CheckLineOfSightForPlayerPostfixPatch(EnemyAI __instance, ref PlayerControllerB[] __result)
    {
        if (Imperium.Settings.Player.Invisibility.Value && __result.Contains(Imperium.Player))
        {
            __result = __result.Where(player => player != Imperium.Player).ToArray();
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPostfixPatch(EnemyAI __instance, ref PlayerControllerB __result)
    {
        if (Imperium.Settings.Player.Invisibility.Value && __result == Imperium.Player) __result = null;
    }

    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPostfixPatch(EnemyAI __instance, ref PlayerControllerB __result)
    {
        if (Imperium.Settings.Player.Invisibility.Value && __result == Imperium.Player) __result = null;
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
    }
}
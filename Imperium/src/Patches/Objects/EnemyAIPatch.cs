#region

using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyAI))]
internal static class EnemyAIPatch
{
    private static PlayerControllerB[] playerBackup = [];

    [HarmonyPrefix]
    [HarmonyPatch("PlayerIsTargetable")]
    private static bool PlayerIsTargetablePatch(
        EnemyAI __instance, PlayerControllerB playerScript, ref bool __result
    )
    {
        if (!Imperium.IsImperiumReady) return true;

        if (playerScript == Imperium.Player && ImpSettings.Player.Invisibility.Value)
        {
            __result = false;
            return false;
        }

        return true;
    }

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

        Imperium.Visualization.EntityInfos.ConeVisualizerUpdate(
            __instance,
            eyeObject ? eyeObject : __instance.eye,
            width,
            coneSize,
            material: ImpAssets.WireframePurpleMaterial
        );

        if (proximityCheck > 0)
        {
            Imperium.Visualization.EntityInfos.SphereVisualizerUpdate(
                __instance,
                null,
                proximityCheck * 2,
                material: ImpAssets.WireframePurpleMaterial
            );
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSight")]
    private static void CheckLineOfSightPrefixPatch(EnemyAI __instance, List<GameObject> objectsToLookFor, float width, int range, float proximityAwareness)
    {
        Imperium.Visualization.EntityInfos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            width,
            range,
            material: ImpAssets.WireframeRedMaterial
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityInfos.SphereVisualizerUpdate(
                __instance,
                __instance.transform,
                proximityAwareness * 2,
                material: ImpAssets.WireframeRedMaterial
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
        if (ImpSettings.Player.Invisibility.Value)
        {
            playerBackup = Imperium.StartOfRound.allPlayerScripts;
            Imperium.StartOfRound.allPlayerScripts = Imperium.StartOfRound.allPlayerScripts
                .Where(player => player != Imperium.Player).ToArray();
        }

        var coneSize = range;

        if (__instance.isOutside
            && !__instance.enemyType.canSeeThroughFog
            && Imperium.TimeOfDay.currentLevelWeather == LevelWeatherType.Foggy)
        {
            coneSize = Mathf.Clamp(range, 0, 30);
        }

        Imperium.Visualization.EntityInfos.ConeVisualizerUpdate(
            __instance,
            __instance.eye,
            width,
            coneSize,
            material: ImpAssets.WireframeCyanMaterial
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityInfos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                proximityAwareness * 2,
                material: ImpAssets.WireframeCyanMaterial
            );
        }
    }

    /// <summary>
    /// Restores allPlayerScripts modified by prefix patch
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForPlayer")]
    private static void CheckLineOfSightForPlayerPostfixPatch(EnemyAI __instance)
    {
        if (ImpSettings.Player.Invisibility.Value)
        {
            Imperium.StartOfRound.allPlayerScripts = playerBackup;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("CheckLineOfSightForPosition")]
    private static void CheckLineOfSightForPositionPrefixPatch(
        EnemyAI __instance, Vector3 objectPosition, float width, int range,
        float proximityAwareness, Transform overrideEye
    )
    {
        if ((!__instance.isOutside && objectPosition.y > -80f) || objectPosition.y < -100f)
        {
            return;
        }

        Imperium.Visualization.EntityInfos.ConeVisualizerUpdate(
            __instance,
            overrideEye ? overrideEye : __instance.eye,
            width,
            range,
            material: ImpAssets.WireframeYellowMaterial
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityInfos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                proximityAwareness * 2,
                material: ImpAssets.WireframeYellowMaterial
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
        if (ImpSettings.Player.Invisibility.Value)
        {
            playerBackup = Imperium.StartOfRound.allPlayerScripts;
            Imperium.StartOfRound.allPlayerScripts = Imperium.StartOfRound.allPlayerScripts
                .Where(player => player != Imperium.Player).ToArray();
        }

        var coneSize = range;

        if (__instance.isOutside
            && !__instance.enemyType.canSeeThroughFog
            && Imperium.TimeOfDay.currentLevelWeather == LevelWeatherType.Foggy)
        {
            coneSize = Mathf.Clamp(range, 0, 30);
        }

        Imperium.Visualization.EntityInfos.ConeVisualizerUpdate(
            __instance,
            null,
            width,
            coneSize,
            material: ImpAssets.WireframeAmaranthMaterial
        );

        if (proximityAwareness > 0)
        {
            Imperium.Visualization.EntityInfos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                proximityAwareness * 2,
                material: ImpAssets.WireframeAmaranthMaterial
            );
        }
    }

    /// <summary>
    ///     Restores allPlayerScripts modified by prefix patch
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
    private static void CheckLineOfSightForClosestPlayerPostfixPatch(EnemyAI __instance)
    {
        if (ImpSettings.Player.Invisibility.Value)
        {
            Imperium.StartOfRound.allPlayerScripts = playerBackup;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("MeetsStandardPlayerCollisionConditions")]
    private static void MeetsStandardPlayerCollisionConditionsPrefix()
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("MeetsStandardPlayerCollisionConditions")]
    private static void MeetsStandardPlayerCollisionConditionsPostfix()
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    private static void UpdatePrefixPatch()
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePostfixPatch()
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }
}
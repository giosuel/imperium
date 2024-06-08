#region

using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyAICollisionDetect))]
internal class EnemyAICollisionDetectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("IHittable.Hit")]
    private static void HitPrefixPatch(EnemyAICollisionDetect __instance, out bool __state)
    {
        // Pass true to postfix if enemy is currently alive
        __state = !__instance.mainScript.isEnemyDead;
    }

    [HarmonyPostfix]
    [HarmonyPatch("IHittable.Hit")]
    private static void HitPostfixPatch(
        EnemyAICollisionDetect __instance, bool __result, bool __state, int force, int hitID
    )
    {
        // If the hit didn't register, don't do anything
        if (!__result) return;

        var entityName = __instance.mainScript.enemyType.enemyName;
        Imperium.Log.LogInfo(
            $"Entity {entityName} ({__instance.GetInstanceID()}) was hit by {force} damage, ID: {hitID}");
        if (__state)
        {
            ImpOutput.Send(
                $"Entity {entityName} was hit by {force} damage.",
                type: NotificationType.Entities
            );
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("INoiseListener.DetectNoise")]
    private static void DetectNoisePatch(EnemyAICollisionDetect __instance, Vector3 noisePosition)
    {
        Imperium.Visualization.EntityGizmos.NoiseVisualizerUpdate(__instance.mainScript, noisePosition);
    }
}
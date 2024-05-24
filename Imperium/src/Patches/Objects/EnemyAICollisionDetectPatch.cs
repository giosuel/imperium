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
    private static void HitPatchPrefix(EnemyAICollisionDetect __instance, out bool __state)
    {
        __state = !__instance.mainScript.isEnemyDead; //pass true to postfix if enemy is currently alive
    }

    [HarmonyPostfix]
    [HarmonyPatch("IHittable.Hit")]
    private static void HitPatchPostfix(bool __result, bool __state, EnemyAICollisionDetect __instance, int force, int hitID)
    {
        if (!__result) return; //if the hit didn't register, don't do anything

        var entityName = __instance.mainScript.enemyType.enemyName;
        Imperium.Log.LogInfo(
            $"Entity {entityName} ({__instance.GetInstanceID()}) was hit by {force} damage, ID: {hitID}");
        if (__state) //if the enemy was alive before the hit
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
        Imperium.Visualization.EntityInfos.NoiseVisualizerUpdate(__instance.mainScript, noisePosition);
    }
}
#region

using GameNetcodeStuff;
using HarmonyLib;
using Imperium.API.Types.Networking;
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
        EnemyAICollisionDetect __instance, bool __result, bool __state, int force, int hitID,
        Vector3 hitDirection, PlayerControllerB playerWhoHit
    )
    {
        // If the hit didn't register, don't do anything
        if (!__result) return;

        var entityName = __instance.mainScript.enemyType.enemyName;
        if (__state)
        {
            Imperium.IO.LogInfo(
                $"Entity {entityName} ({__instance.GetInstanceID()}) was hit by {force} damage, ID: {hitID}"
            );
            Imperium.IO.Send(
                $"Entity {entityName} was hit by {force} damage.",
                type: NotificationType.Entities
            );

            Imperium.EventLog.EntityEvents.DetectHit(__instance.mainScript, hitDirection, playerWhoHit, force);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("INoiseListener.DetectNoise")]
    private static void DetectNoisePatch(
        EnemyAICollisionDetect __instance, Vector3 noisePosition, float noiseLoudness,
        int timesNoisePlayedInOneSpot, int noiseID
    )
    {
        Imperium.Visualization.EntityGizmos.NoiseVisualizerUpdate(__instance.mainScript, noisePosition);

        Imperium.EventLog.EntityEvents.DetectNoise(
            __instance.mainScript, noisePosition, noiseID, timesNoisePlayedInOneSpot, noiseLoudness
        );
    }
}
#region

using HarmonyLib;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyAICollisionDetect))]
internal class EnemyAICollisionDetectPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("IHittable.Hit")]
    private static void HitPatch(EnemyAICollisionDetect __instance, int force, int hitID)
    {
        var entityName = __instance.mainScript.enemyType.enemyName;
        Imperium.Log.LogInfo($"Entity {entityName} ({__instance.GetInstanceID()}) was hit by {force} damage, ID: {hitID}");
        if (!__instance.mainScript.isEnemyDead)
        {
            ImpOutput.Send(
                $"Entity {entityName} was hit by {force} damage.",
                notificationType: NotificationType.Entities
            );
        }
    }
}
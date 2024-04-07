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
    private static void HitPatch(EnemyAICollisionDetect __instance, int force)
    {
        var entityName = __instance.mainScript.enemyType.enemyName;
        Imperium.Output.Log($"Entity {entityName} ({__instance.GetInstanceID()}) was hit by {force} damage.");
        if (!__instance.mainScript.isEnemyDead)
        {
            Imperium.Output.Send(
                $"Entity {entityName} was hit by {force} damage.",
                notificationType: NotificationType.Entities
            );
        }
    }
}
#region

using System.Collections.Generic;
using HarmonyLib;
using Imperium.Patches.Systems;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyVent))]
internal static class EnemyVentPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("SyncVentSpawnTimeClientRpc")]
    private static void SyncVentSpawnTimeClientRpcPatch(EnemyVent __instance, int time, int enemyIndex)
    {
        var entity = Imperium.RoundManager.currentLevel.Enemies[enemyIndex];
        if (RoundManagerPatch.spawnedEntitiesInCycle.TryGetValue(entity.enemyType.enemyName, out var entityList))
        {
            if (!entityList.Contains(__instance.floorNode.position)) entityList.Add(__instance.floorNode.position);
        }
        else
        {
            var list = new List<Vector3> { __instance.floorNode.position };
            RoundManagerPatch.spawnedEntitiesInCycle[entity.enemyType.enemyName] = list;
        }
    }
}
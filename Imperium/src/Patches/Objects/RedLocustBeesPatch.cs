#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(RedLocustBees))]
internal static class RedLocustBeesPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(RedLocustBees.SpawnHiveNearEnemy))]
    private static void SpawnHiveNearEnemyPatch(RedLocustBees __instance)
    {
        Imperium.Log.LogInfo($"Number spawned: {__instance.enemyType.numberSpawned}");
    }
}
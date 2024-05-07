using System.Collections.Generic;
using System.Linq;
using DunGen;
using HarmonyLib;
using UnityEngine;

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(Dungeon))]
internal class DungeonPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("FromProxy")]
    private static void FromProxyPatch(Dungeon __instance, DungeonProxy proxyDungeon, DungeonGenerator generator)
    {
        Imperium.Map.FloorLevels.Set(
            proxyDungeon.AllTiles
                .SelectMany(tile => tile.doorways)
                .Select(door => Mathf.RoundToInt(door.Position.y * 1000))
                .ToHashSet()
        );
    }
}
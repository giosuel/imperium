#region

using HarmonyLib;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(BushWolfEnemy))]
public static class BushWolfPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    internal static void UpdatePrefixPatch(BushWolfEnemy __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    internal static void UpdatePostfixPatch(BushWolfEnemy __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }
}
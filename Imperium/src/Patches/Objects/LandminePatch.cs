#region

using HarmonyLib;
using Imperium.Core;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(Landmine))]
internal static class LandminePatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Detonate")]
    internal static void DetonatePatch(Landmine __instance)
    {
        Imperium.Visualization.LandmineIndicators.SnapshotPlayerHitbox(__instance.GetInstanceID());
    }

    [HarmonyPrefix]
    [HarmonyPatch("OnTriggerEnter")]
    internal static void OnTriggerEnterPrefixPatch(Landmine __instance)
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnTriggerEnter")]
    internal static void OnTriggerEnterPostfixPatch(Landmine __instance)
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("OnTriggerExit")]
    internal static void OnTriggerExitPrefixPatch(Landmine __instance)
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("OnTriggerExit")]
    internal static void OnTriggerExitPostfixPatch(Landmine __instance)
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch("SpawnExplosion")]
    internal static void SpawnExplosionPrefixPatch(Landmine __instance)
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("SpawnExplosion")]
    internal static void SpawnExplosionPostfixPatch(Landmine __instance)
    {
        if (ImpSettings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }
}
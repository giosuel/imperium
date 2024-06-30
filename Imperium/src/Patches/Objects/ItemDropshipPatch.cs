#region

using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(ItemDropship))]
public static class ItemDropshipPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    internal static void UpdatePrefixPatch(ItemDropship __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    internal static void UpdatePostfixPatch(ItemDropship __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }
}
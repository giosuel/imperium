#region

using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(VehicleController))]
public static class VehicleControllerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("ReactToDamage")]
    internal static void ReactToDamagePrefixPatch(VehicleController __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("ReactToDamage")]
    internal static void ReactToDamagePostfixPatch(VehicleController __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }
}
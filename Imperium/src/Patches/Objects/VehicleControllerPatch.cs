#region

using HarmonyLib;
using Imperium.Core.Lifecycle;
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

    [HarmonyPrefix]
    [HarmonyPatch("PushTruckWithArms")]
    internal static void PushTruckWithArmsPrefixPatch(VehicleController __instance)
    {
        __instance.pushForceMultiplier = Imperium.PlayerManager.CarPushForceBinding.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("PushTruckClientRpc")]
    internal static void PushTruckClientRpcPrefixPatch(VehicleController __instance)
    {
        __instance.pushForceMultiplier = Imperium.PlayerManager.CarPushForceBinding.Value;
    }
}
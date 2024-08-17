#region

using System.Collections.Generic;
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

    [HarmonyPrefix]
    [HarmonyPatch("PushTruckWithArms")]
    internal static void PushTruckWithArmsPrefixPatch(VehicleController __instance)
    {
        __instance.pushForceMultiplier = Imperium.CruiserManager.PushForce.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("PushTruckClientRpc")]
    internal static void PushTruckClientRpcPrefixPatch(VehicleController __instance)
    {
        __instance.pushForceMultiplier = Imperium.CruiserManager.PushForce.Value;
    }

    [HarmonyPrefix]
    [HarmonyPatch("DealPermanentDamage")]
    internal static bool DealPermanentDamagePrefixPatch(VehicleController __instance)
    {
        return !Imperium.CruiserManager.Indestructible.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch("Start")]
    internal static void StartPostfixPatch(VehicleController __instance)
    {
        __instance.mainRigidbody.MovePosition(__instance.transform.position);
        __instance.hasBeenSpawned = true;

        if (Imperium.CruiserManager.SpawnFullTurbo.Value) __instance.AddTurboBoostOnLocalClient(5);
    }

    [HarmonyPrefix]
    [HarmonyPatch("Update")]
    internal static void UpdatePrefixPatch(VehicleController __instance)
    {
        __instance.carAcceleration = Imperium.CruiserManager.Acceleration.Value;
    }

    [HarmonyPostfix]
    [HarmonyPatch("UseTurboBoostLocalClient")]
    internal static void UseTurboBoostLocalClientPrefixPatch(VehicleController __instance)
    {
        __instance.AddTurboBoostOnLocalClient(1);
    }

    [HarmonyPrefix]
    [HarmonyPatch("TryIgnition")]
    internal static void TryIgnitionPrefixPatch(VehicleController __instance)
    {
        if (Imperium.Settings.Cruiser.InstantIgnite.Value) __instance.chanceToStartIgnition = 100;
    }

    internal static readonly Harmony InstantIgnitionHarmony = new(Imperium.PLUGIN_GUID + ".InstantIgnition");

    internal static class InstantIgnitionPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch(typeof(VehicleController), "TryIgnition", MethodType.Enumerator)]
        private static IEnumerable<CodeInstruction> openingDoorsSequenceTranspiler(
            IEnumerable<CodeInstruction> instructions
        )
        {
            return ImpUtils.Transpiling.SkipWaitingForSeconds(instructions);
        }
    }
}
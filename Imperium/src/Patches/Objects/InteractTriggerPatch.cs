#region

using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(InteractTrigger))]
internal static class InteractTriggerPatch
{
    private static readonly Dictionary<int, float> OriginalAnimationWaitTimes = [];

    [HarmonyPrefix]
    [HarmonyPatch("specialInteractAnimation")]
    private static void specialInteractAnimationPrefixPatch(InteractTrigger __instance, PlayerControllerB playerController)
    {
        if (Imperium.Settings.AnimationSkipping.Interact.Value)
        {
            // Backup original animation wait time
            if (!OriginalAnimationWaitTimes.ContainsKey(__instance.GetInstanceID()))
            {
                OriginalAnimationWaitTimes[__instance.GetInstanceID()] = __instance.animationWaitTime;
            }

            __instance.animationWaitTime = 0;
        }
        else
        {
            // Restore original animation wait time if it has been changed before
            if (OriginalAnimationWaitTimes.TryGetValue(__instance.GetInstanceID(), out var originalWaitTime))
            {
                __instance.animationWaitTime = originalWaitTime;
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("specialInteractAnimation")]
    private static void specialInteractAnimationPatch(InteractTrigger __instance, PlayerControllerB playerController)
    {
        if (Imperium.Settings.AnimationSkipping.Interact.Value)
        {
            playerController.playerBodyAnimator.ResetTrigger(__instance.animationString);
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("HoldInteractNotFilled")]
    internal static void HoldInteractNotFilledPrefixPatch(InteractTrigger __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch("HoldInteractNotFilled")]
    internal static void HoldInteractNotFilledPostfixPatch(InteractTrigger __instance)
    {
        if (Imperium.Settings.Preferences.OptimizeLogs.Value) Debug.unityLogger.logEnabled = true;
    }
}
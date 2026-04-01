#region

using System.Collections;
using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(Shovel))]
internal static class ShovelPatch
{
    private static readonly int ShovelHit = Animator.StringToHash("shovelHit");
    private static readonly int ReelingUp = Animator.StringToHash("reelingUp");

    [HarmonyPostfix]
    [HarmonyPatch("DiscardItem")]
    internal static void DiscardItemPatch(Shovel __instance)
    {
        Imperium.Visualization.ShovelGizmos.Refresh(__instance, false);
    }

    [HarmonyPrefix]
    [HarmonyPatch("ItemActivate")]
    internal static bool ItemActivate(Shovel __instance, bool used, bool buttonDown = true)
    {
        if (__instance.playerHeldBy) return true;

        if (!Imperium.Settings.Shovel.Speedy.Value)
        {
            __instance.playerHeldBy.playerBodyAnimator.speed = 1;
            return true;
        }

        __instance.isHoldingButton = buttonDown;
        if (!__instance.reelingUp && buttonDown)
        {
            __instance.playerHeldBy.playerBodyAnimator.speed = 3;
            __instance.reelingUp = true;

            __instance.previousPlayerHeldBy = __instance.playerHeldBy;

            if (__instance.reelingUpCoroutine != null) __instance.StopCoroutine(__instance.reelingUpCoroutine);
            __instance.reelingUpCoroutine = __instance.StartCoroutine(__instance.reelUpShovel());
        }

        return false;
    }

    /// <summary>
    ///     Run original enumerator <see cref="Shovel.reelUpShovel" /> but remove static waiting times.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("reelUpShovel")]
    private static IEnumerator reelUpShovelPatch(IEnumerator __result)
    {
        while (__result.MoveNext())
        {
            var it = __result.Current;
            if (it is WaitForSeconds { })
            {
                continue;
            }
            yield return it;
        }
    }
}
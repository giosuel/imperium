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
        if (__instance.playerHeldBy == null)
        {
            // Vanilla would simply `return;` anyway
            return true;
        }

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
            __instance.reelingUpCoroutine = __instance.StartCoroutine(reelUpShovelPatch(__instance));
        }

        return false;
    }

    /// <summary>
    ///     Cloned from <see cref="Shovel.reelUpShovel" /> and removed static waiting times.
    /// </summary>
    private static IEnumerator reelUpShovelPatch(Shovel shovel)
    {
        shovel.playerHeldBy.activatingItem = true;
        shovel.playerHeldBy.twoHanded = true;
        shovel.playerHeldBy.playerBodyAnimator.ResetTrigger(ShovelHit);
        shovel.playerHeldBy.playerBodyAnimator.SetBool(ReelingUp, value: true);
        shovel.shovelAudio.PlayOneShot(shovel.reelUp);
        shovel.ReelUpSFXServerRpc();
        yield return new WaitUntil(() => !shovel.isHoldingButton || !shovel.isHeld);
        shovel.SwingShovel(!shovel.isHeld);
        yield return new WaitForEndOfFrame();
        shovel.HitShovel(!shovel.isHeld);
        shovel.reelingUp = false;
        shovel.reelingUpCoroutine = null;

        yield return null;
    }
}
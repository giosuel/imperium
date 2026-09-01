#region

using System.Collections;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(Shovel))]
internal static class ShovelPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DiscardItem")]
    internal static void DiscardItemPatch(Shovel __instance)
    {
        Imperium.Visualization.ShovelGizmos.Refresh(__instance, false);
    }

    /// <summary>
    ///     Run the original <see cref="Shovel.reelUpShovel" /> coroutine,
    ///     but remove static waiting times and setup/reset speed.
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch("reelUpShovel")]
    internal static IEnumerator reelUpShovelPostfixPatch(IEnumerator __result, Shovel __instance)
    {
        if (Imperium.Settings.Shovel.Speedy.Value)
        {
            return SpeedyShovelPlayerBehaviour.reelUpShovel(__result, __instance);
        }
        else
        {
            // just run vanilla
            return __result;
        }
    }
}

internal class SpeedyShovelPlayerBehaviour : MonoBehaviour
{
    private int LayerIndex;

    private Animator PlayerBodyAnimator;

    private Coroutine Coroutine;

    private const float SPEED_DEFAULT = 1f;
    private const float SPEED_SPEEDY = 3f;

    /// <summary>
    ///     Run the original <see cref="Shovel.reelUpShovel" /> coroutine,
    ///     but remove static waiting times and setup/reset speed.
    ///
    ///     Assumes that Speedy Shovel setting is enabled at the call time.
    /// </summary>
    internal static IEnumerator reelUpShovel(IEnumerator source, Shovel shovel)
    {
        var self = ImpUtils.GetOrAddComponent<SpeedyShovelPlayerBehaviour>(shovel.playerHeldBy);
        self.ReelUp();

        var wrapper = ImpUtils.SkipWaitingForSeconds(source);
        while (wrapper.MoveNext())
        {
            yield return wrapper.Current;
        }
        // reelingUpCoroutine has been reset to null at the end of vanilla method,
        // so StopCoroutine won't be called for whatever comes next. Hence this custom
        // MonoBehaviour with our own coroutine management.
        self.ResetSpeedAfterAnimation();
    }

    private void Awake()
    {
        PlayerBodyAnimator = gameObject.GetComponent<PlayerControllerB>().playerBodyAnimator;
        LayerIndex = PlayerBodyAnimator.GetLayerIndex(ImpAnimatorHash.Metarig.Layer_HoldingItemsBothHands);
    }

    private void ReelUp()
    {
        StopResetCoroutine();
        if (Imperium.Settings.Shovel.Speedy.Value)
        {
            SpeedUp();
        }
        else
        {
            ResetSpeed();
        }
    }

    private void ResetSpeedAfterAnimation()
    {
        StopResetCoroutine();
        Coroutine = StartCoroutine(ResetSpeedAfterAnimationCoroutine());
    }

    private void StopResetCoroutine()
    {
        if (Coroutine != null)
        {
            StopCoroutine(Coroutine);
        }
    }

    private IEnumerator ResetSpeedAfterAnimationCoroutine()
    {
        // Wait until the (sped up) animation is completed before resetting the speed
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo stateInfo = PlayerBodyAnimator.GetCurrentAnimatorStateInfo(LayerIndex);
            var hash = stateInfo.shortNameHash;
            return hash != ImpAnimatorHash.Metarig.ShovelReelUp
                && hash != ImpAnimatorHash.Metarig.HitShovel;
        });
        ResetSpeed();
        Coroutine = null;
    }

    private void SpeedUp()
    {
        SetSpeed(SPEED_SPEEDY);
    }

    private void ResetSpeed()
    {
        SetSpeed(SPEED_DEFAULT);
    }

    private void SetSpeed(float speed)
    {
        PlayerBodyAnimator.speed = speed;
    }
}

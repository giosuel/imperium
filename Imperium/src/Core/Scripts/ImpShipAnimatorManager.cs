#region

using System.Collections;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Core.Scripts;

internal class ImpShipAnimatorManager : MonoBehaviour
{
    private const int LayerIndex = 0;

    private Animator animator;
    private PlayAudioAnimationEvent playAudioAnimationEvent;

    private Coroutine coroutine;

    private const float SPEED_DEFAULT = 1f;
    private const float SPEED_SPEEDY = 1000f;

    // New commands override current animator speed.
    // If the condition is true, WaitForSeconds will be skipped from the source coroutine,
    // and a reset will be scheduled when the animation completes.
    internal static IEnumerator SkipAnimationIf(IEnumerator source, bool condition)
    {
        var self = ImpUtils.GetOrAddComponent<ImpShipAnimatorManager>(Imperium.StartOfRound.shipAnimatorObject);
        self.SkipAnimationIfImpl(condition);

        var wrapper = source;
        if (condition)
        {
            wrapper = ImpUtils.SkipWaitingForSeconds(source);
        }
        while (wrapper.MoveNext())
        {
            yield return wrapper.Current;
        }

        self.ResetSpeedAfterAnimation();
    }

    // This method assumes the ship animation has already started,
    // so it immediately schedules a coroutine that will check the animation state every frame.
    internal static void SkipAnimationIf(bool condition)
    {
        var self = ImpUtils.GetOrAddComponent<ImpShipAnimatorManager>(Imperium.StartOfRound.shipAnimatorObject);
        self.SkipAnimationIfImpl(condition);
        self.ResetSpeedAfterAnimation();
    }

    private void Awake()
    {
        animator = Imperium.StartOfRound.shipAnimator;
        playAudioAnimationEvent = animator.GetComponent<PlayAudioAnimationEvent>();
    }

    private void SkipAnimationIfImpl(bool condition)
    {
        StopResetCoroutine();
        if (condition)
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
        coroutine = StartCoroutine(ResetSpeedAfterAnimationCoroutine());
    }

    private void StopResetCoroutine()
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
        }
    }

    private IEnumerator ResetSpeedAfterAnimationCoroutine()
    {
        // Wait until the (sped up) animation is completed before resetting the speed
        yield return new WaitUntil(() =>
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(LayerIndex);
            var hash = stateInfo.shortNameHash;
            // Skip all the temporary states with unconditional Exit Time into "stable" states.
            return hash != ImpAnimatorHash.ShipAnimator.ShipFlyAwayFromPlanet
                && hash != ImpAnimatorHash.ShipAnimator.ShipFlyToPlanet
                && hash != ImpAnimatorHash.ShipAnimator.ShipOpen
                && hash != ImpAnimatorHash.ShipAnimator.ShipLeave;
        });
        ResetSpeed();
        coroutine = null;
    }

    private void SpeedUp()
    {
        SetProperties(SPEED_SPEEDY, mute: true);
    }

    private void ResetSpeed()
    {
        SetProperties(SPEED_DEFAULT, mute: false);
    }

    private void SetProperties(float speed, bool mute)
    {
        animator.speed = speed;
        playAudioAnimationEvent.audioToPlay.mute = mute;
        playAudioAnimationEvent.audioToPlayB.mute = mute;
    }
}

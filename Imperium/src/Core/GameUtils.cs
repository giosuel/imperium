using UnityEngine;

namespace Imperium.Core;

public static class GameUtils
{
    internal static void PlayClip(AudioClip audioClip, bool randomize = false)
    {
        RoundManager.PlayRandomClip(Imperium.HUDManager.UIAudio, [audioClip], randomize);
    }
}
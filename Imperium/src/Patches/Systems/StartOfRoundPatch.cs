#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(StartOfRound))]
public class StartOfRoundPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("unloadSceneForAllPlayers")]
    private static void unloadSceneForAllPlayersPatch(StartOfRound __instance)
    {
        Imperium.IsSceneLoaded.SetFalse();
    }

    [HarmonyPostfix]
    [HarmonyPatch("ChooseNewRandomMapSeed")]
    private static void ChooseNewRandomMapSeedPatch(StartOfRound __instance)
    {
        if (Imperium.GameManager.CustomSeed.Value != -1)
        {
            __instance.randomMapSeed = Imperium.GameManager.CustomSeed.Value;
        }
    }
}
#region

using HarmonyLib;
using Imperium.Core;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(HangarShipDoor))]
internal static class HangarShipDoorPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("PlayDoorAnimation")]
    private static bool PlayDoorAnimationPatch(HangarShipDoor __instance, bool closed)
    {
        if (ImpSettings.Game.OverwriteShipDoors.Value)
        {
            __instance.shipDoorsAnimator.SetBool("Closed", closed);
            return false;
        }

        return true;
    }
}
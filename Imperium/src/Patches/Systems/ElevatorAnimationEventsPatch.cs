#region

using HarmonyLib;
using Imperium.Core;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(ElevatorAnimationEvents))]
internal static class ElevatorAnimationEventsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(ElevatorAnimationEvents.ElevatorFullyRunning))]
    private static void ElevatorFullyRunningPatch()
    {
        // Make the game think the player is in the elevator, so they get teleported no matter what
        if (Imperium.Settings.Ship.DisableAbandoned.Value)
        {
            Imperium.Player.isInElevator = true;
        }
    }
}
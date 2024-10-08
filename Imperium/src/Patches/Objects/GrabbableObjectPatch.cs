#region

using HarmonyLib;
using Imperium.Interface.ImperiumUI;
using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(GrabbableObject))]
internal static class GrabbableObjectPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePatch(GrabbableObject __instance)
    {
        if (Imperium.Settings.Player.InfiniteBattery.Value) __instance.insertedBattery.charge = 1;
    }

    [HarmonyPostfix]
    [HarmonyPatch("EquipItem")]
    internal static void EquipItemPatch(GrabbableObject __instance)
    {
        // This needs to be patched here as the Shovel and Knife scripts do not override this method
        switch (__instance)
        {
            case Shovel shovel:
                Imperium.Visualization.ShovelGizmos.Refresh(shovel, true);
                break;
            case KnifeItem knife:
                Imperium.Visualization.KnifeGizmos.Refresh(knife, true);
                break;
        }

        // Refresh object explorer for drop button
        Imperium.Interface.Get<ImperiumUI>()?.Get<ObjectExplorerWindow>()?.RefreshEntries();
    }

    [HarmonyPostfix]
    [HarmonyPatch("PocketItem")]
    internal static void PocketItemPatch(GrabbableObject __instance)
    {
        // This needs to be patched here as Shovel does not override this method
        switch (__instance)
        {
            case Shovel shovel:
                Imperium.Visualization.ShovelGizmos.Refresh(shovel, false);
                break;
            case KnifeItem knife:
                Imperium.Visualization.KnifeGizmos.Refresh(knife, false);
                break;
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("DiscardItem")]
    internal static void DiscardItemPatch(GrabbableObject __instance)
    {
        // Refresh object explorer for drop button
        Imperium.Interface.Get<ImperiumUI>()?.Get<ObjectExplorerWindow>()?.RefreshEntries();
    }
}
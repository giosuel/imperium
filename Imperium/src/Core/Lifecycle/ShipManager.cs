#region

using Imperium.Netcode;
using Imperium.Patches.Systems;
using Imperium.Util.Binding;

#endregion

namespace Imperium.Core.Lifecycle;

public class ShipManager(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected)
    : ImpLifecycleObject(sceneLoaded, playersConnected)
{
    internal readonly ImpNetworkBinding<bool> InstantTakeoff = new(
        "InstantTakeoff",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Ship.InstantTakeoff,
        onUpdateClient: value =>
        {
            if (value)
            {
                StartOfRoundPatch.InstantTakeoffHarmony.PatchAll(typeof(StartOfRoundPatch.InstantTakeoffPatches));
            }
            else
            {
                StartOfRoundPatch.InstantTakeoffHarmony.UnpatchSelf();
                Imperium.StartOfRound.shipAnimator.ResetTrigger("landing");
                Imperium.StartOfRound.shipAnimator.enabled = true;
            }
        }
    );

    internal readonly ImpNetworkBinding<bool> InstantLanding = new(
        "InstantLanding",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Ship.InstantLanding,
        onUpdateClient: value =>
        {
            if (value)
            {
                StartOfRoundPatch.InstantLandingHarmony.PatchAll(typeof(StartOfRoundPatch.InstantLandingPatches));
            }
            else
            {
                StartOfRoundPatch.InstantLandingHarmony.UnpatchSelf();
                Imperium.StartOfRound.shipAnimator.ResetTrigger("landing");
                Imperium.StartOfRound.shipAnimator.enabled = true;
            }
        }
    );

    internal readonly ImpNetworkBinding<bool> UnlockShop = new(
        "UnlockShop",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Ship.UnlockShop,
        onUpdateClient: value =>
        {
            // Reset selection when locking shop
            if (!value) Imperium.Terminal.RotateShipDecorSelection();
        }
    );

    internal readonly ImpNetworkBinding<bool> DisableAbandoned = new(
        "DisableAbandoned",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Ship.DisableAbandoned
    );

    internal readonly ImpNetworkBinding<bool> PreventShipLeave = new(
        "PreventShipLeave",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Ship.PreventLeave
    );
}
using Imperium.Netcode;
using Imperium.Patches.Systems;
using Imperium.Util.Binding;

namespace Imperium.Core;

public class ShipManager : ImpLifecycleObject
{
    internal readonly ImpNetworkBinding<bool> InstantTakeoff = new(
        "InstantTakeoff",
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
                Imperium.StartOfRound.shipAnimator.enabled = true;
            }
        }
    );

    internal readonly ImpNetworkBinding<bool> InstantLanding = new(
        "InstantLanding",
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
                Imperium.StartOfRound.shipAnimator.enabled = true;
            }
        }
    );

    internal readonly ImpNetworkBinding<bool> DisableAbandoned = new(
        "DisableAbandoned",
        masterBinding: Imperium.Settings.Ship.DisableAbandoned
    );

    internal readonly ImpNetworkBinding<bool> PreventShipLeave = new(
        "PreventShipLeave",
        masterBinding: Imperium.Settings.Ship.PreventLeave
    );

    public ShipManager(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected)
        : base(sceneLoaded, playersConnected)
    {

    }
}
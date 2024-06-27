#region

using Imperium.Netcode;
using Imperium.Patches.Systems;
using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;

#endregion

namespace Imperium.Core.Lifecycle;

public class ShipManager : ImpLifecycleObject
{
    private readonly ImpNetMessage<int> navigateShipMessage = new("NavigateShip", Imperium.Networking);

    public ShipManager(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected) : base(sceneLoaded, playersConnected)
    {
        if (NetworkManager.Singleton.IsHost) navigateShipMessage.OnServerReceive += OnNavigateToServer;
        navigateShipMessage.OnClientRecive += OnNavigateToClient;
    }

    [ImpAttributes.RemoteMethod]
    internal void NavigateTo(int levelIndex) => navigateShipMessage.DispatchToServer(levelIndex);

    [ImpAttributes.HostOnly]
    private void OnNavigateToServer(int levelIndex, ulong clientId) => navigateShipMessage.DispatchToClients(levelIndex);

    [ImpAttributes.HostOnly]
    private static void OnNavigateToClient(int levelIndex)
    {
        Imperium.StartOfRound.ChangeLevelClientRpc(levelIndex, Imperium.GameManager.GroupCredits.Value);

        // Send scene refresh so moon related data is refreshed
        Imperium.IsSceneLoaded.Refresh();
    }

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
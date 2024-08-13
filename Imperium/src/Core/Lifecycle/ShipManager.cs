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

    [ImpAttributes.LocalMethod]
    private static void OnNavigateToClient(int levelIndex)
    {
        if (!Imperium.StartOfRound.inShipPhase) return;

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
                if (!Imperium.StartOfRound.inShipPhase) Imperium.StartOfRound.shipAnimator.speed = 1000f;
            }
            else
            {
                StartOfRoundPatch.InstantTakeoffHarmony.UnpatchSelf();
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
                if (!Imperium.StartOfRound.inShipPhase) Imperium.StartOfRound.shipAnimator.speed = 1000f;
            }
            else
            {
                StartOfRoundPatch.InstantLandingHarmony.UnpatchSelf();
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
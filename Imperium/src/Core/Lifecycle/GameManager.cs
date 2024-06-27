#region

using Imperium.API.Types.Networking;
using Imperium.Netcode;
using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Core.Lifecycle;

internal class GameManager : ImpLifecycleObject
{
    internal readonly ImpNetEvent FulfillQuotaEvent = new("FulfillQuota", Imperium.Networking);

    internal GameManager(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected)
        : base(sceneLoaded, playersConnected)
    {
        if (NetworkManager.Singleton.IsHost) FulfillQuotaEvent.OnServerReceive += FulfillQuota;
    }

    internal readonly ImpBinding<int> CustomSeed = new(-1);

    internal readonly ImpNetworkBinding<int> GroupCredits = new(
        "GroupCredits",
        Imperium.Networking,
        Imperium.Terminal.groupCredits,
        onUpdateClient: value => Imperium.Terminal.groupCredits = value
    );

    internal readonly ImpNetworkBinding<int> ProfitQuota = new(
        "ProfitQuota",
        Imperium.Networking,
        Imperium.TimeOfDay.profitQuota,
        Imperium.TimeOfDay.quotaVariables.startingQuota,
        onUpdateClient: value => Imperium.TimeOfDay.profitQuota = value,
        onUpdateServer: value =>
        {
            Imperium.TimeOfDay.SyncNewProfitQuotaClientRpc(
                value, 0, Imperium.TimeOfDay.timesFulfilledQuota
            );
        }
    );

    internal readonly ImpNetworkBinding<int> QuotaDeadline = new(
        "QuotaDeadline",
        Imperium.Networking,
        Imperium.TimeOfDay.daysUntilDeadline,
        onUpdateClient: value =>
        {
            var startMatchLever = Object.FindObjectOfType<StartMatchLever>();
            startMatchLever.hasDisplayedTimeWarning = false;
            startMatchLever.triggerScript.timeToHold = 0.7f;

            Imperium.TimeOfDay.timesFulfilledQuota--;
            Imperium.TimeOfDay.quotaVariables.deadlineDaysAmount = value;
            Imperium.TimeOfDay.timeUntilDeadline = Imperium.TimeOfDay.totalTime * value;
            Imperium.TimeOfDay.UpdateProfitQuotaCurrentTime();
            Imperium.TimeOfDay.SetBuyingRateForDay();
            Imperium.HUDManager.DisplayDaysLeft(value);
        }
    );

    internal readonly ImpNetworkBinding<bool> DisableQuota = new(
        "DisableQuota",
        Imperium.Networking,
        onUpdateClient: value =>
        {
            if (value) Imperium.TimeOfDay.timeUntilDeadline = Imperium.TimeOfDay.totalTime * 3f;
        }
    );

    [ImpAttributes.HostOnly]
    private static void FulfillQuota(ulong clientId)
    {
        Imperium.TimeOfDay.daysUntilDeadline = -1;
        Imperium.TimeOfDay.quotaFulfilled = Imperium.TimeOfDay.profitQuota;
        Imperium.TimeOfDay.SetNewProfitQuota();
    }

    protected override void OnSceneLoad()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        ProfitQuota.Sync(Imperium.TimeOfDay.profitQuota);
        QuotaDeadline.Sync(Imperium.TimeOfDay.daysUntilDeadline);
        GroupCredits.Sync(Imperium.Terminal.groupCredits);
    }

    internal static void NavigateTo(int levelIndex)
    {
        Imperium.StartOfRound.ChangeLevelServerRpc(levelIndex, Imperium.Terminal.groupCredits);

        // Send scene refresh so moon related data is refreshed
        Imperium.IsSceneLoaded.Refresh();
    }
}
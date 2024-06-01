#region

using Imperium.Core;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// This is a network behaviour so the members have to not be static
public class ImpNetQuota : NetworkBehaviour
{
    internal static ImpNetQuota Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.IsHost && Instance)
        {
            Instance.gameObject.GetComponent<NetworkObject>().Despawn();
        }

        Instance = this;
        base.OnNetworkSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetProfitQuotaServerRpc(int newProfitQuota)
    {
        if (!ImpSettings.Preferences.AllowClients.Value && !NetworkManager.IsHost) return;

        SetProfitQuotaClientRpc(newProfitQuota);
    }

    [ClientRpc]
    private void SetProfitQuotaClientRpc(int newProfitQuota)
    {
        Imperium.GameManager.ProfitQuota.Set(newProfitQuota, skipSync: true);
        Imperium.TimeOfDay.SyncNewProfitQuotaClientRpc(newProfitQuota, 0, Imperium.TimeOfDay.timesFulfilledQuota);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetDeadlineDaysServerRpc(int days)
    {
        if (!ImpSettings.Preferences.AllowClients.Value && !NetworkManager.IsHost) return;

        OnChangeDeadlineDaysClientRpc(days);
    }

    [ClientRpc]
    private void OnChangeDeadlineDaysClientRpc(int days)
    {
        Imperium.GameManager.QuotaDeadline.Set(days, skipSync: true);

        // Reset the warning now that the quota has been reset
        var startMatchLever = FindObjectOfType<StartMatchLever>();
        startMatchLever.hasDisplayedTimeWarning = false;
        startMatchLever.triggerScript.timeToHold = 0.7f;

        Imperium.TimeOfDay.timesFulfilledQuota--;
        Imperium.TimeOfDay.quotaVariables.deadlineDaysAmount = days;
        Imperium.TimeOfDay.timeUntilDeadline = Imperium.TimeOfDay.totalTime * days;
        Imperium.TimeOfDay.UpdateProfitQuotaCurrentTime();
        Imperium.TimeOfDay.SetBuyingRateForDay();
        Imperium.HUDManager.DisplayDaysLeft(days);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetGroupCreditsServerRpc(int credits)
    {
        if (!ImpSettings.Preferences.AllowClients.Value && !NetworkManager.IsHost) return;

        SetGroupCreditsClientRpc(credits);
    }

    [ClientRpc]
    private void SetGroupCreditsClientRpc(int credits)
    {
        Imperium.GameManager.GroupCredits.Set(credits, skipSync: true);
        Imperium.Terminal.SyncGroupCreditsClientRpc(credits, 0);
    }
}
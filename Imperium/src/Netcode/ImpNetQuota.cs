#region

using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// This is a network behaviour so the members have to not be static
public class ImpNetQuota : NetworkBehaviour
{
    internal static ImpNetQuota Instance { get; private set; }
    
    public override void OnNetworkSpawn()
    {
        if (ImpNetworkManager.IsHost.Value && Instance)
        {
            Instance.gameObject.GetComponent<NetworkObject>().Despawn();
        }
        Instance = this;
        base.OnNetworkSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetProfitQuotaServerRpc(int newProfitQuota)
    {
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
        OnChangeDeadlineDaysClientRpc(days);
    }

    [ClientRpc]
    private void OnChangeDeadlineDaysClientRpc(int days)
    {
        Imperium.GameManager.QuotaDeadline.Set(days, skipSync: true);

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
        SetGroupCreditsClientRpc(credits);
    }

    [ClientRpc]
    private void SetGroupCreditsClientRpc(int credits)
    {
        Imperium.GameManager.GroupCredits.Set(credits, skipSync: true);
        Imperium.Terminal.SyncGroupCreditsClientRpc(credits, 0);
    }
}
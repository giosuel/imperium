#region

using Imperium.Core;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// This is a network behaviour so the members have to not be static
public class ImpNetTime : NetworkBehaviour
{
    internal static ImpNetTime Instance { get; private set; }

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
    internal void RequestTimeServerRpc()
    {
        if (!ImpSettings.Preferences.AllowClients.Value) return;

        SyncTimeClientRpc(Imperium.GameManager.TimeSpeed.Value, Imperium.GameManager.TimeIsPaused.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void UpdateTimeServerRpc(float timeSpeed, bool isPaused)
    {
        if (!ImpSettings.Preferences.AllowClients.Value) return;

        SyncTimeClientRpc(timeSpeed, isPaused);
    }

    [ClientRpc]
    private void SyncTimeClientRpc(float timeSpeed, bool isPaused)
    {
        Imperium.GameManager.TimeSpeed.Set(timeSpeed, skipSync: true);
        Imperium.GameManager.TimeIsPaused.Set(isPaused, skipSync: true);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetShipLeaveAutomaticallyServerRpc(bool leaveAutomatically)
    {
        if (!ImpSettings.Preferences.AllowClients.Value) return;

        SetShipLeaveAutomaticallyClientRpc(leaveAutomatically);
    }

    [ClientRpc]
    private void SetShipLeaveAutomaticallyClientRpc(bool leaveAutomatically)
    {
        ImpSettings.Game.PreventShipLeave.Set(!leaveAutomatically, skipSync: true);
    }
}
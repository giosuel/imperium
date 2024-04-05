#region

using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public abstract class ImpNetworkManager
{
    internal static readonly ImpBinding<bool> IsHost = new(NetworkManager.Singleton.IsHost);
    internal static readonly ImpBinding<int> ConnectedPlayers = new(1);

    internal static void OnClientConnected(ulong clientId)
    {
        ImpOutput.Log(
            $"[NET] Imperium has detected a connect: {clientId} (host: {NetworkManager.Singleton.IsHost})"
        );
        ImpOutput.Send($"A client has connected! ID: {clientId}", "Imperium Networking");
        ConnectedPlayers.Set(ConnectedPlayers.Value + 1);
    }

    internal static void OnClientDisconnected(ulong clientId)
    {
        ImpOutput.Log(
            $"[NET] Imperium has detected a disconnect: {clientId} (host: {NetworkManager.Singleton.IsHost})"
        );
        ImpOutput.Send($"A client has disconnected! ID: {clientId}", "Imperium Networking");

        ConnectedPlayers.Set(ConnectedPlayers.Value - 1);
    }
}
#region

using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Netcode;

public abstract class ImpNetworkManager
{
    internal static readonly ImpBinding<int> ConnectedPlayers = new(1);
    internal static GameObject NetworkPrefab;

    internal static void OnClientConnected(ulong clientId)
    {
        Imperium.Log.LogInfo(
            $"[NET] Imperium has detected a connect: {clientId} (host: {NetworkManager.Singleton.IsHost})"
        );
        ImpOutput.Send($"A client has connected! ID: {clientId}", "Imperium Networking");
        ConnectedPlayers.Set(ConnectedPlayers.Value + 1);
    }

    internal static void OnClientDisconnected(ulong clientId)
    {
        Imperium.Log.LogInfo(
            $"[NET] Imperium has detected a disconnect: {clientId} (host: {NetworkManager.Singleton.IsHost})"
        );
        ImpOutput.Send($"A client has disconnected! ID: {clientId}", "Imperium Networking");

        ConnectedPlayers.Set(ConnectedPlayers.Value - 1);
    }
}
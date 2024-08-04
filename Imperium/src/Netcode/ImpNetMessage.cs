#region

using System;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public class ImpNetMessage<T> : INetworkSubscribable
{
    private readonly LNetworkMessage<T> networkMessage;

    internal event Action<T, ulong> OnServerReceive;

    internal event Action<T> OnClientRecive;
    internal event Action<T, ulong> OnClientReciveFromClient;

    private readonly string identifier;

    public ImpNetMessage(string identifier, ImpNetworking networking)
    {
        this.identifier = identifier;

        networkMessage = LNetworkMessage<T>.Connect($"{identifier}_message");

        networkMessage.OnServerReceived += (data, clientId) =>
        {
            if (clientId == NetworkManager.ServerClientId || Imperium.Settings.Preferences.AllowClients.Value)
            {
                OnServerReceive?.Invoke(data, clientId);
            }
        };
        networkMessage.OnClientReceived += data => OnClientRecive?.Invoke(data);
        networkMessage.OnClientReceivedFromClient += (data, clientId) => OnClientReciveFromClient?.Invoke(data, clientId);

        networking.RegisterSubscriber(this);
    }

    internal void DispatchToServer(T data)
    {
        Imperium.IO.LogInfo($"[NET] Client sends {identifier} data to server");
        networkMessage.SendServer(data);
    }

    internal void DispatchToClients(T data)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Imperium.IO.LogInfo($"[NET] Server sends {identifier} data to clients");
            networkMessage.SendClients(data);
        }
        else
        {
            Imperium.IO.LogInfo($"[NET] Client sends {identifier} data to clients");
            networkMessage.SendClients(data);
        }
    }

    internal void DispatchToClients(T data, params ulong[] clientIds) => networkMessage.SendClients(data, clientIds);

    public void Clear()
    {
        networkMessage.ClearSubscriptions();
    }

    public void BroadcastToClient(ulong clientId)
    {
    }
}
#region

using System;
using Imperium.Util;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public class ImpNetMessage<T> : INetworkSubscribable
{
    private readonly LNetworkMessage<T> networkMessage;

    internal event Action<T, ulong> OnServerReceive;

    internal event Action<T> OnClientRecive;

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
        networkMessage.OnClientReceivedFromClient += (data, _) => OnClientRecive?.Invoke(data);

        networking.RegisterSubscriber(this);
    }

    [ImpAttributes.RemoteMethod]
    internal void DispatchToServer(T data)
    {
        Imperium.IO.LogInfo($"[NET] Client sends {identifier} data to server.");
        networkMessage.SendServer(data);
    }

    [ImpAttributes.RemoteMethod]
    internal void DispatchToClients(T data)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Imperium.IO.LogInfo($"[NET] Server sends {identifier} data to clients.");
            networkMessage.SendClients(data);
        }
        else
        {
            Imperium.IO.LogInfo($"[NET] Client sends {identifier} data to other clients.");
            networkMessage.SendOtherClients(data);

            OnClientRecive?.Invoke(data);
        }
    }

    [ImpAttributes.HostOnly]
    internal void DispatchToClients(T data, params ulong[] clientIds)
    {
        Imperium.IO.LogInfo($"[NET] Server sends {identifier} data to clients ({string.Join(",", clientIds)}).");
        networkMessage.SendClients(data, clientIds);
    }

    public void Clear()
    {
        networkMessage.ClearSubscriptions();
    }

    public void BroadcastToClient(ulong clientId)
    {
    }
}
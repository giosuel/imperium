#region

using System;
using Imperium.Util.Binding;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public class ImpNetMessage<T> : IClearable
{
    private readonly LethalClientMessage<T> clientMessage;
    private readonly LethalServerMessage<T> serverMessage;

    internal event Action<T, ulong> OnServerReceive;

    internal event Action<T> OnClientRecive;
    internal event Action<T, ulong> OnClientReciveFromClient;

    private readonly string identifier;

    public ImpNetMessage(string identifier, ImpNetworking networking)
    {
        this.identifier = identifier;

        clientMessage = new LethalClientMessage<T>($"{identifier}_message");
        serverMessage = new LethalServerMessage<T>($"{identifier}_message");

        serverMessage.OnReceived += (data, clientId) =>
        {
            if (clientId == NetworkManager.ServerClientId || Imperium.Settings.Preferences.AllowClients.Value)
            {
                OnServerReceive?.Invoke(data, clientId);
            }
        };
        clientMessage.OnReceived += data => OnClientRecive?.Invoke(data);
        clientMessage.OnReceivedFromClient += (data, clientId) => OnClientReciveFromClient?.Invoke(data, clientId);

        networking.RegisterSubscriber(this);
    }

    internal void DispatchToServer(T data)
    {
        Imperium.IO.LogInfo($"[NET] Client sends {identifier} data to server");
        clientMessage.SendServer(data);
    }

    internal void DispatchToClients(T data)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Imperium.IO.LogInfo($"[NET] Server sends {identifier} data to clients");
            serverMessage.SendAllClients(data);
        }
        else
        {
            Imperium.IO.LogInfo($"[NET] Client sends {identifier} data to clients");
            clientMessage.SendAllClients(data);
        }
    }

    internal void DispatchToClients(T data, params ulong[] clientIds) => serverMessage.SendClients(data, clientIds);

    public void Clear()
    {
        clientMessage.ClearSubscriptions();
        serverMessage.ClearSubscriptions();
    }
}
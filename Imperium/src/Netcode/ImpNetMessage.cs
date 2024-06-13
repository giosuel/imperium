#region

using System;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public class ImpNetMessage<T>
{
    private readonly LethalClientMessage<T> clientMessage;
    private readonly LethalServerMessage<T> serverMessage;

    internal event Action<T, ulong> OnServerReceive;

    internal event Action<T> OnClientRecive;
    internal event Action<T, ulong> OnClientReciveFromClient;

    private readonly string identifier;

    public ImpNetMessage(string identifier)
    {
        this.identifier = identifier;

        clientMessage = new LethalClientMessage<T>($"{identifier}_message");
        serverMessage = new LethalServerMessage<T>($"{identifier}_message");

        serverMessage.OnReceived += (data, clientId) => OnServerReceive?.Invoke(data, clientId);
        clientMessage.OnReceived += data => OnClientRecive?.Invoke(data);
        clientMessage.OnReceivedFromClient += (data, clientId) => OnClientReciveFromClient?.Invoke(data, clientId);
    }

    internal void DispatchToServer(T data) => clientMessage.SendServer(data);

    internal void DispatchToClients(T data)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            serverMessage.SendAllClients(data);
        }
        else
        {
            clientMessage.SendAllClients(data);
        }
    }

    internal void DispatchToClients(T data, params ulong[] clientIds) => serverMessage.SendClients(data, clientIds);
}
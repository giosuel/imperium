using System;
using LethalNetworkAPI;
using Unity.Netcode;

namespace Imperium.Netcode;

public class ImpNetEvent
{
    private readonly LethalClientEvent clientEvent;
    private readonly LethalServerEvent serverEvent;

    internal event Action<ulong> OnServerReceive;

    internal event Action OnClientRecive;
    internal event Action<ulong> OnClientReciveFromClient;

    public ImpNetEvent(string identifier)
    {
        clientEvent = new LethalClientEvent($"{identifier}_event");
        serverEvent = new LethalServerEvent($"{identifier}_event");

        serverEvent.OnReceived += clientId => OnServerReceive?.Invoke(clientId);
        clientEvent.OnReceived += () => OnClientRecive?.Invoke();
        clientEvent.OnReceivedFromClient += clientId => OnClientReciveFromClient?.Invoke(clientId);
    }

    internal void DispatchToServer() => clientEvent.InvokeServer();

    internal void DispatchToClients()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            serverEvent.InvokeAllClients();
        }
        else
        {
            clientEvent.InvokeAllClients();
        }
    }

    internal void DispatchToClients(params ulong[] clientIds) => serverEvent.InvokeClients(clientIds);
}
#region

using System;
using Imperium.Util.Binding;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public class ImpNetEvent : IClearable
{
    private readonly LethalClientEvent clientEvent;
    private readonly LethalServerEvent serverEvent;

    internal event Action<ulong> OnServerReceive;
    internal event Action OnClientRecive;
    internal event Action<ulong> OnClientReciveFromClient;

    private string identifier;

    public ImpNetEvent(string identifier, ImpNetworking networking)
    {
        this.identifier = identifier;

        clientEvent = new LethalClientEvent($"{identifier}_event");
        serverEvent = new LethalServerEvent($"{identifier}_event");

        serverEvent.OnReceived += clientId =>
        {
            if (clientId == NetworkManager.ServerClientId || Imperium.Settings.Preferences.AllowClients.Value)
            {
                OnServerReceive?.Invoke(clientId);
            }
        };
        clientEvent.OnReceived += () => OnClientRecive?.Invoke();
        clientEvent.OnReceivedFromClient += clientId => OnClientReciveFromClient?.Invoke(clientId);

        networking.RegisterSubscriber(this);
    }

    internal void DispatchToServer()
    {
        Imperium.IO.LogInfo($"Client sends {identifier} event to server");
        clientEvent.InvokeServer();
    }

    internal void DispatchToClients()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Imperium.IO.LogInfo($"Server sends {identifier} event to clients");
            serverEvent.InvokeAllClients();
        }
        else
        {
            Imperium.IO.LogInfo($"Client sends {identifier} event to clients");
            clientEvent.InvokeAllClients();
        }
    }

    internal void DispatchToClients(params ulong[] clientIds) => serverEvent.InvokeClients(clientIds);
    public void Clear()
    {
        clientEvent.ClearSubscriptions();
        serverEvent.ClearSubscriptions();
    }
}
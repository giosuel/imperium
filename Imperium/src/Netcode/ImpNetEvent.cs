#region

using System;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public class ImpNetEvent : INetworkSubscribable
{
    private readonly LNetworkEvent networkEvent;

    // private readonly LethalClientEvent clientEvent;
    // private readonly LethalServerEvent serverEvent;

    internal event Action<ulong> OnServerReceive;
    internal event Action OnClientRecive;
    internal event Action<ulong> OnClientReciveFromClient;

    private readonly string identifier;

    public ImpNetEvent(string identifier, ImpNetworking networking)
    {
        this.identifier = identifier;

        // clientEvent = new LethalClientEvent($"{identifier}_event");
        // serverEvent = new LethalServerEvent($"{identifier}_event");

        networkEvent = LNetworkEvent.Connect($"{identifier}_event");

        networkEvent.OnServerReceived += clientId =>
        {
            Imperium.IO.LogInfo("ImpNetEvent::OnServerReceived");
            if (clientId == NetworkManager.ServerClientId || Imperium.Settings.Preferences.AllowClients.Value)
            {
                OnServerReceive?.Invoke(clientId);
            }
        };
        networkEvent.OnClientReceived += () =>
        {
            Imperium.IO.LogInfo("ImpNetEvent::OnClientReceived");
            OnClientRecive?.Invoke();
        };
        networkEvent.OnClientReceivedFromClient += clientId =>
        {
            Imperium.IO.LogInfo("ImpNetEvent::OnClientReceivedFromClient");
            OnClientReciveFromClient?.Invoke(clientId);
        };

        networking.RegisterSubscriber(this);
    }

    internal void DispatchToServer()
    {
        Imperium.IO.LogInfo($"[NET] Client sends {identifier} event to server");
        networkEvent.InvokeServer();
    }

    internal void DispatchToClients()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Imperium.IO.LogInfo($"[NET] Server sends {identifier} event to clients");
            networkEvent.InvokeClients();
        }
        else
        {
            Imperium.IO.LogInfo($"[NET] Client sends {identifier} event to clients");
            networkEvent.InvokeClients();
        }
    }

    internal void DispatchToClients(params ulong[] clientIds)
    {
        Imperium.IO.LogInfo($"[NET] Server sends {identifier} event to clients");
        networkEvent.InvokeClients(clientIds);
    }

    public void Clear()
    {
        networkEvent.ClearSubscriptions();
    }

    public void BroadcastToClient(ulong clientId)
    {
    }
}
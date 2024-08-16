#region

using System;
using Imperium.Util;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public class ImpNetEvent : INetworkSubscribable
{
    private readonly LNetworkEvent networkEvent;
    internal event Action<ulong> OnServerReceive;
    internal event Action OnClientRecive;

    private readonly string identifier;

    public ImpNetEvent(string identifier, ImpNetworking networking)
    {
        this.identifier = identifier;

        networkEvent = LNetworkEvent.Connect($"{identifier}_event");

        networkEvent.OnServerReceived += clientId =>
        {
            if (clientId == NetworkManager.ServerClientId || Imperium.Settings.Preferences.AllowClients.Value)
            {
                OnServerReceive?.Invoke(clientId);
            }
        };
        networkEvent.OnClientReceived += () => OnClientRecive?.Invoke();
        networkEvent.OnClientReceivedFromClient += _ => OnClientRecive?.Invoke();

        networking.RegisterSubscriber(this);
    }

    [ImpAttributes.RemoteMethod]
    internal void DispatchToServer()
    {
        Imperium.IO.LogInfo($"[NET] Client sends {identifier} event to server.");
        networkEvent.InvokeServer();
    }

    [ImpAttributes.RemoteMethod]
    internal void DispatchToClients()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Imperium.IO.LogInfo($"[NET] Server sends {identifier} event to clients.");
            networkEvent.InvokeClients();
        }
        else
        {
            Imperium.IO.LogInfo($"[NET] Client sends {identifier} event to other clients.");
            networkEvent.InvokeOtherClients();

            OnClientRecive?.Invoke();
        }
    }

    [ImpAttributes.HostOnly]
    internal void DispatchToClients(params ulong[] clientIds)
    {
        Imperium.IO.LogInfo($"[NET] Server sends {identifier} event to clients ({string.Join(",", clientIds)}).");
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
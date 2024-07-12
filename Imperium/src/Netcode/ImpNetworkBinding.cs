#region

using System;
using System.Collections.Generic;
using Imperium.API.Types.Networking;
using Imperium.Util.Binding;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public sealed class ImpNetworkBinding<T> : IBinding<T>, INetworkSubscribable
{
    public event Action<T> onUpdate;
    public event Action<T> onUpdateFromLocal;
    public event Action onTrigger;
    public event Action onTriggerFromLocal;

    private readonly Action<T> onUpdateServer;

    public T DefaultValue { get; }

    public T Value { get; private set; }

    // private readonly LethalServerMessage<BindingUpdateRequest<T>> serverMessage;
    // private readonly LethalClientMessage<BindingUpdateRequest<T>> clientMessage;

    private readonly LNetworkMessage<BindingUpdateRequest<T>> networkMessage;

    // This optional binding provides the initial value and is changed only when the local client updates the state.
    private readonly IBinding<T> masterBinding;

    private readonly string identifier;

    public ImpNetworkBinding(
        string identifier,
        ImpNetworking networking,
        T currentValue = default,
        T defaultValue = default,
        Action<T> onUpdateClient = null,
        Action<T> onUpdateServer = null,
        IBinding<T> masterBinding = null
    )
    {
        this.identifier = identifier;

        Value = currentValue;
        DefaultValue = masterBinding != null
            ? masterBinding.DefaultValue
            : !EqualityComparer<T>.Default.Equals(defaultValue, default)
                ? defaultValue
                : currentValue;

        onUpdate += onUpdateClient;
        this.onUpdateServer = onUpdateServer;
        this.masterBinding = masterBinding;

        networkMessage = LNetworkMessage<BindingUpdateRequest<T>>.Connect($"{identifier}_binding");

        // serverMessage = new LethalServerMessage<BindingUpdateRequest<T>>($"{identifier}_binding");
        // clientMessage = new LethalClientMessage<BindingUpdateRequest<T>>($"{identifier}_binding");

        // serverMessage.OnReceived += OnServerReceived;
        // clientMessage.OnReceived += OnClientReceived;

        networkMessage.OnServerReceived += OnServerReceived;
        networkMessage.OnClientReceived += OnClientReceived;

        if (masterBinding != null && NetworkManager.Singleton.IsHost) Set(masterBinding.Value);

        networking.RegisterSubscriber(this);
    }

    private void OnServerReceived(BindingUpdateRequest<T> request, ulong clientId)
    {
        Imperium.IO.LogInfo($"[NET] Server received binding update for {identifier}");
        if (clientId == NetworkManager.ServerClientId || Imperium.Settings.Preferences.AllowClients.Value)
        {
            // Invoke optional custom binding (e.g. Calls to vanilla client RPCs)
            // if (request.InvokeServerUpdate) onUpdateServer?.Invoke(request.Payload);

            networkMessage.SendClients(request);
        }
    }

    private void OnClientReceived(BindingUpdateRequest<T> updatedValue)
    {
        Imperium.IO.LogInfo($"[NET] Client received binding update for {identifier}");
        Value = updatedValue.Payload;

        if (updatedValue.InvokeUpdate)
        {
            onUpdate?.Invoke(Value);
            onTrigger?.Invoke();
        }
    }

    public void Sync(T updatedValue) => Set(updatedValue, false, false);

    public void Set(T updatedValue, bool invokeUpdate = true) => Set(updatedValue, invokeUpdate, true);

    private void Set(T updatedValue, bool invokeUpdate, bool invokeServerUpdate)
    {
        Value = updatedValue;
        masterBinding?.Set(updatedValue);

        if (invokeUpdate)
        {
            onUpdateFromLocal?.Invoke(updatedValue);
            onTriggerFromLocal?.Invoke();
        }

        networkMessage.SendServer(new BindingUpdateRequest<T>
        {
            Payload = updatedValue,
            InvokeUpdate = invokeUpdate,
            InvokeServerUpdate = invokeServerUpdate
        });
    }

    public void Refresh()
    {
    }

    public void Reset(bool invokeUpdate = true) => Set(DefaultValue, invokeUpdate);

    public void Clear()
    {
        networkMessage.ClearSubscriptions();
    }

    public void BroadcastToClient(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost) return;

        networkMessage.SendClient(new BindingUpdateRequest<T>
        {
            Payload = Value,
            InvokeUpdate = true
        }, clientId);
    }
}

public interface INetworkSubscribable
{
    public void Clear();
    public void BroadcastToClient(ulong clientId);
}
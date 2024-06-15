#region

using System;
using System.Collections.Generic;
using Imperium.API.Types.Networking;
using Imperium.Util.Binding;
using LethalNetworkAPI;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

public sealed class ImpNetworkBinding<T> : IBinding<T>
{
    public event Action<T> onUpdate;
    public event Action<T> onUpdateFromLocal;
    public event Action onTrigger;
    public event Action onTriggerFromLocal;

    private readonly Action<T> onUpdateServer;

    public T DefaultValue { get; }

    public T Value { get; private set; }

    private readonly LethalServerMessage<BindingUpdateRequest<T>> serverMessage;
    private readonly LethalClientMessage<BindingUpdateRequest<T>> clientMessage;

    // This optional binding provides the initial value and is changed only when the local client updates the state.
    private readonly IBinding<T> masterBinding;

    private string identifier;

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
        DefaultValue = EqualityComparer<T>.Default.Equals(defaultValue, default)
            ? defaultValue
            : currentValue;

        onUpdate += onUpdateClient;
        this.onUpdateServer = onUpdateServer;
        this.masterBinding = masterBinding;

        serverMessage = new LethalServerMessage<BindingUpdateRequest<T>>($"{identifier}_binding");
        clientMessage = new LethalClientMessage<BindingUpdateRequest<T>>($"{identifier}_binding");

        serverMessage.OnReceived += OnServerReceived;
        clientMessage.OnReceived += OnClientReceived;

        if (masterBinding != null) Set(masterBinding.Value);

        networking.RegisterSubscriber(this);
    }

    private void OnServerReceived(BindingUpdateRequest<T> request, ulong clientId)
    {
        Imperium.IO.LogInfo($"Server received binding update for {identifier}");
        if (clientId == NetworkManager.ServerClientId || Imperium.Settings.Preferences.AllowClients.Value)
        {
            // Invoke optional custom binding (e.g. Calls to vanilla client RPCs)
            if (request.InvokeServerUpdate) onUpdateServer?.Invoke(request.Payload);

            serverMessage.SendAllClients(request);
        }
    }

    private void OnClientReceived(BindingUpdateRequest<T> updatedValue)
    {
        Imperium.IO.LogInfo($"Client received binding update for {identifier}");
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

        clientMessage.SendServer(new BindingUpdateRequest<T>
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
        // onUpdate = null;
        // onTrigger = null;
        // onTriggerFromLocal = null;
        // onUpdateFromLocal = null;

        serverMessage.ClearSubscriptions();
        clientMessage.ClearSubscriptions();
    }
}
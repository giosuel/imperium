using System;
using System.Collections.Generic;
using Imperium.API.Types.Networking;
using Imperium.Util.Binding;
using LethalNetworkAPI;
using Unity.Netcode;

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

    public ImpNetworkBinding(
        string identifier,
        T currentValue = default,
        T defaultValue = default,
        Action<T> onUpdateClient = null,
        Action<T> onUpdateServer = null,
        IBinding<T> masterBinding = null
    )
    {
        Value = currentValue;
        DefaultValue = EqualityComparer<T>.Default.Equals(defaultValue, default)
            ? defaultValue
            : currentValue;

        onUpdate += onUpdateClient;
        this.onUpdateServer = onUpdateServer;
        this.masterBinding = masterBinding;

        serverMessage = new LethalServerMessage<BindingUpdateRequest<T>>(identifier);
        clientMessage = new LethalClientMessage<BindingUpdateRequest<T>>(identifier);

        serverMessage.OnReceived += OnServerReceived;
        clientMessage.OnReceived += OnClientReceived;

        if (masterBinding != null) Set(masterBinding.Value);
    }

    private void OnServerReceived(BindingUpdateRequest<T> request, ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId || Imperium.Settings.Preferences.AllowClients.Value)
        {
            // Invoke optional custom binding (e.g. Calls to vanilla client RPCs)
            if(request.InvokeServerUpdate) onUpdateServer?.Invoke(request.Payload);

            serverMessage.SendAllClients(request);
        }
    }

    private void OnClientReceived(BindingUpdateRequest<T> updatedValue)
    {
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
}
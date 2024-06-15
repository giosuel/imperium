#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Util;
using Imperium.Util.Binding;
using LethalNetworkAPI;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Netcode;

public class ImpNetworking
{
    private readonly HashSet<IClearable> RegisteredNetworkSubscribers = [];

    internal static readonly ImpBinding<int> ConnectedPlayers = new(1);

    private readonly ImpNetEvent authenticateEvent;
    private readonly ImpNetEvent enableImperiumEvent;
    private readonly ImpNetEvent disableImperiumEvent;

    private readonly ImpNetMessage<NetworkNotification> networkLog;

    internal readonly ImpNetworkBinding<List<ulong>> ImperiumUsers;

    public ImpNetworking(IBinding<bool> allowClientsBinding)
    {
        authenticateEvent = new ImpNetEvent("AuthenticateImperium", this);
        enableImperiumEvent = new ImpNetEvent("EnableImperium", this);
        disableImperiumEvent = new ImpNetEvent("DisableImperium", this);
        networkLog = new ImpNetMessage<NetworkNotification>("NetworkLog", this);

        ImperiumUsers = new ImpNetworkBinding<List<ulong>>(
            "ImperiumUsers",
            this,
            currentValue: [NetworkManager.ServerClientId]
        );

        if (NetworkManager.Singleton.IsHost)
        {
            authenticateEvent.OnServerReceive += OnAuthenticateRequest;
            allowClientsBinding.onUpdate += ToggleImperiumAccess;
        }
        else
        {
            enableImperiumEvent.OnClientRecive += OnEnableImperiumAccess;
            disableImperiumEvent.OnClientRecive += OnDisableImperiumAccess;
        }

        authenticateEvent.OnClientRecive += OnAuthenticateResponse;
        networkLog.OnClientRecive += OnLogReceived;
    }

    internal void RegisterSubscriber(IClearable subscriber) => RegisteredNetworkSubscribers.Add(subscriber);

    internal void SendLog(NetworkNotification report)
    {
        if (!NetworkManager.Singleton.IsHost) return;

        networkLog.DispatchToClients(report);
    }

    private static void OnLogReceived(NetworkNotification report) => Imperium.IO.Send(
        report.Message, report.Title ?? "Imperium Server", report.IsWarning, report.Type
    );

    [ImpAttributes.HostOnly]
    private void OnAuthenticateRequest(ulong clientId)
    {
        // Always grant Imperium access if the request comes from the host
        if (clientId == NetworkManager.ServerClientId)
        {
            authenticateEvent.DispatchToClients();
            return;
        }

        if (Imperium.Settings.Preferences.AllowClients.Value)
        {
            var playerName = clientId.GetPlayerController()?.playerUsername ?? $"#{clientId}";
            Imperium.IO.Send(
                $"Imperium access was granted to client #{playerName}.",
                type: NotificationType.AccessControl
            );
            Imperium.IO.LogInfo($"[NET] Client #{clientId} successfully requested Imperium access ({playerName})!");

            authenticateEvent.DispatchToClients();
            ImperiumUsers.Set(ImperiumUsers.Value.Concat([clientId]).ToList());
        }
        else
        {
            var playerName = clientId.GetPlayerController()?.playerUsername ?? $"#{clientId}";
            Imperium.IO.Send(
                $"Imperium access was denied to client #{playerName}.",
                type: NotificationType.AccessControl
            );
            Imperium.IO.LogInfo($"[NET] Client #{clientId} failed to request Imperium access ({playerName})!");
        }
    }

    [ImpAttributes.HostOnly]
    private void ToggleImperiumAccess(bool hasAccess)
    {
        if (hasAccess)
        {
            enableImperiumEvent.DispatchToClients();
        }
        else
        {
            disableImperiumEvent.DispatchToClients();
        }
    }

    [ImpAttributes.LocalMethod]
    private static void OnAuthenticateResponse()
    {
        Imperium.IO.Send(
            "Imperium access was granted!",
            type: NotificationType.AccessControl
        );
        Imperium.IO.LogInfo("[NET] Imperium access was granted!");
        Imperium.Launch();
    }

    [ImpAttributes.LocalMethod]
    private static void OnDisableImperiumAccess()
    {
        if (NetworkManager.Singleton.IsHost) return;

        Imperium.IO.Send(
            "Imperium access was revoked!",
            type: NotificationType.AccessControl,
            isWarning: true
        );
        Imperium.IO.LogInfo("[NET] Imperium access was revoked!");
        Imperium.DisableImperium();
    }

    [ImpAttributes.LocalMethod]
    private static void OnEnableImperiumAccess()
    {
        if (NetworkManager.Singleton.IsHost) return;

        Imperium.IO.Send(
            "Imperium access was granted!",
            type: NotificationType.AccessControl
        );
        Imperium.IO.LogInfo("[NET] Imperium access was granted!");
        if (Imperium.WasImperiumAccessGranted)
        {
            Imperium.EnableImperium();
        }
        else
        {
            Imperium.Launch();
        }
    }

    [ImpAttributes.RemoteMethod]
    internal void RequestImperiumAccess() => Imperium.RoundManager.StartCoroutine(waitForImperiumAccess());

    private IEnumerator waitForImperiumAccess()
    {
        authenticateEvent.DispatchToServer();

        yield return new WaitForSeconds(5f);
        if (!Imperium.IsImperiumLaunched)
        {
            Imperium.IO.Send("Failed to aqcuire Imperium access! Shutting down...", isWarning: true);
        }
    }

    internal static void OnClientConnected(ulong clientId)
    {
        Imperium.IO.LogInfo(
            $"[NET] Imperium has detected a connect: {clientId} (host: {NetworkManager.Singleton.IsHost})"
        );
        Imperium.IO.Send($"A client has connected! ID: {clientId}", "Imperium Networking");
        ConnectedPlayers.Set(ConnectedPlayers.Value + 1);
    }

    internal static void OnClientDisconnected(ulong clientId)
    {
        Imperium.IO.LogInfo(
            $"[NET] Imperium has detected a disconnect: {clientId} (host: {NetworkManager.Singleton.IsHost})"
        );
        Imperium.IO.Send($"A client has disconnected! ID: {clientId}", "Imperium Networking");

        ConnectedPlayers.Set(ConnectedPlayers.Value - 1);
    }

    public void Unsubscribe()
    {
        foreach (var subscriber in RegisteredNetworkSubscribers) subscriber.Clear();
        RegisteredNetworkSubscribers.Clear();
    }
}
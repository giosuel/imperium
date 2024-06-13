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
    internal static readonly ImpBinding<int> ConnectedPlayers = new(1);

    private readonly ImpNetEvent authenticateEvent = new("AuthenticateImperium");
    private readonly ImpNetEvent enableImperiumEvent = new("EnableImperium");
    private readonly ImpNetEvent disableImperiumEvent = new("DisableImperium");

    private readonly ImpNetMessage<NetworkNotification> networkLog = new("NetworkLog");

    internal readonly ImpNetworkBinding<List<ulong>> ImperiumUsers = new(
        "ImperiumUsers",
        currentValue: [NetworkManager.ServerClientId]
    );

    public ImpNetworking(IBinding<bool> allowClientsBinding)
    {
        authenticateEvent.OnServerReceive += OnAuthenticateRequest;
        authenticateEvent.OnClientRecive += OnAuthenticateResponse;

        allowClientsBinding.onUpdate += ToggleImperiumAccess;

        networkLog.OnClientRecive += OnLogReceived;
    }

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

    [ImpAttributes.LocalMethod]
    private void OnAuthenticateResponse()
    {
        Imperium.IO.Send(
            "Imperium access was granted!",
            type: NotificationType.AccessControl
        );
        Imperium.IO.LogInfo("[NET] Imperium access was granted!");
        Imperium.WasImperiumAccessGranted = true;
        Imperium.Launch();
    }

    [ImpAttributes.HostOnly]
    private void ToggleImperiumAccess(bool hasAccess)
    {
        if (!NetworkManager.Singleton.IsHost) return;

        if (hasAccess)
        {
            enableImperiumEvent.DispatchToClients();
            EnableImperiumAccess();
        }
        else
        {
            disableImperiumEvent.DispatchToClients();
            DisableImperiumAccess();
        }
    }

    [ImpAttributes.LocalMethod]
    private void DisableImperiumAccess()
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
    private void EnableImperiumAccess()
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
}
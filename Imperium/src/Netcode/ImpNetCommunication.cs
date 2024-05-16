#region

using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using Imperium.Core;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// This is a network behaviour so the members have to not be static
public class ImpNetCommunication : NetworkBehaviour
{
    internal static ImpNetCommunication Instance { get; private set; }

    internal readonly HashSet<ulong> ImperiumUsers = [NetworkManager.ServerClientId];

    public override void OnNetworkSpawn()
    {
        if (NetworkManager.IsHost && Instance)
        {
            Instance.gameObject.GetComponent<NetworkObject>().Despawn();
        }

        if (NetworkManager.IsHost)
        {
            ImpSettings.Preferences.AllowClients.onUpdate += ToggleImperiumAccess;
        }

        Instance = this;
        base.OnNetworkSpawn();
    }

    internal void RequestImperiumAccess() => StartCoroutine(waitForImperiumAccess());

    private void ToggleImperiumAccess(bool hasAccess)
    {
        if (hasAccess)
        {
            EnableImperiumAccessClientRpc();
        }
        else
        {
            DisableImperiumAccessClientRpc();
        }
    }

    [ClientRpc]
    internal void SendClientRpc(
        string text,
        string title = "Imperium Server",
        bool isWarning = false
    )
    {
        ImpOutput.Send(text: text, title: title, isWarning: isWarning, notificationType: NotificationType.Server);
    }

    /// <summary>
    /// This guard makes it so clients can only use Imperium when the host also has it.
    /// It would be a shame if someone were to comment this out :(
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void RequestImperiumAccessServerRpc(ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        if (ImpSettings.Preferences.AllowClients.Value)
        {
            ReceiveImperiumAccessClientRpc(clientId);
        }
        else
        {
            var playerName = PlayerManager.GetPlayerFromID((int)clientId)?.playerUsername ?? $"#{clientId}";
            ImpOutput.Send($"Imperium access was denied to client #{playerName}.");
            Imperium.Log.LogInfo($"[NET] Client #{clientId} failed to request Imperium access ({playerName})!");
        }
    }

    [ClientRpc]
    private void ReceiveImperiumAccessClientRpc(ulong clientId)
    {
        if (Imperium.IsImperiumLaunched) return;

        if (clientId == NetworkManager.LocalClientId)
        {
            ImpOutput.Send($"Imperium access was granted!");
            Imperium.Log.LogInfo($"[NET] Imperium access was granted!");
            Imperium.WasImperiumAccessGranted = true;
            Imperium.Launch();
        }
        else
        {
            var playerName = PlayerManager.GetPlayerFromID((int)clientId)?.playerUsername ?? $"#{clientId}";
            ImpOutput.Send($"Imperium access was granted to client #{playerName}.");
            Imperium.Log.LogInfo($"[NET] Client #{clientId} successfully requested Imperium access ({playerName})!");
        }

        ImperiumUsers.Add(clientId);
    }

    [ClientRpc]
    private void DisableImperiumAccessClientRpc()
    {
        if (NetworkManager.IsHost) return;

        ImpOutput.Send("Imperium access was revoked!", isWarning: true);
        Imperium.Log.LogInfo($"[NET] Imperium access was revoked!");
        Imperium.DisableImperium();
    }

    [ClientRpc]
    private void EnableImperiumAccessClientRpc()
    {
        if (NetworkManager.IsHost) return;

        ImpOutput.Send($"Imperium access was granted!");
        Imperium.Log.LogInfo($"[NET] Imperium access was granted!");
        if (Imperium.WasImperiumAccessGranted)
        {
            Imperium.EnableImperium();
        }
        else
        {
            Imperium.Launch();
        }
    }

    private IEnumerator waitForImperiumAccess()
    {
        RequestImperiumAccessServerRpc();
        yield return new WaitForSeconds(5f);
        if (!Imperium.IsImperiumLaunched)
        {
            ImpOutput.Send("Failed to aqcuire Imperium access! Shutting down...", isWarning: true);
        }
    }
}
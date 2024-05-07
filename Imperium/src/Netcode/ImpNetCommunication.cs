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

    internal readonly HashSet<ulong> ImperiumUsers = [];

    public override void OnNetworkSpawn()
    {
        if (ImpNetworkManager.IsHost.Value && Instance)
        {
            Instance.gameObject.GetComponent<NetworkObject>().Despawn();
        }

        Instance = this;
        base.OnNetworkSpawn();
    }

    internal void RequestImperiumAccess() => StartCoroutine(waitForImperiumAccess());

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
    private void RequestImperiumAccessServerRpc(ulong clientId)
    {
        if (!ImpSettings.Preferences.AllowClients.Value)
        {
            var playerName = PlayerManager.GetPlayerFromID((int)clientId)?.playerUsername ?? $"#{clientId}";
            ImpOutput.Send($"Imperium access was denied to client #{playerName}.");
            Imperium.Log.LogInfo($"[NET] Client #{clientId} failed to request Imperium access ({playerName})!");
            return;
        }
        ReceiveImperiumAccessClientRpc(clientId);
    }

    [ClientRpc]
    private void ReceiveImperiumAccessClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.LocalClientId)
        {
            ImpOutput.Send($"Imperium access was granted!");
            Imperium.Log.LogInfo($"[NET] Imperium access was granted!");
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

    private IEnumerator waitForImperiumAccess()
    {
        RequestImperiumAccessServerRpc(NetworkManager.Singleton.LocalClientId);
        yield return new WaitForSeconds(5f);
        if (Imperium.IsSceneLoaded == null)
        {
            ImpOutput.Send("Failed to aqcuire Imperium access! Shutting down...", isWarning: true);
        }
    }
}
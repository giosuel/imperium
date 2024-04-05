#region

using Imperium.Util;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// This is a network behaviour so the members have to not be static
public class ImpNetCommunication : NetworkBehaviour
{
    internal static ImpNetCommunication Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance.gameObject);
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

    [ServerRpc(RequireOwnership = false)]
    internal void RequestImperiumAccessServerRpc(ulong clientId)
    {
        ReceiveImperiumAccessClientRpc(clientId);
    }

    [ClientRpc]
    private void ReceiveImperiumAccessClientRpc(ulong clientId)
    {
        if (clientId == NetworkManager.LocalClientId) Imperium.Launch();
    }
}
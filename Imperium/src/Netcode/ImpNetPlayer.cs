#region

using Imperium.Core;
using Imperium.Types;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// This is a network behaviour so the members have to not be static
public class ImpNetPlayer : NetworkBehaviour
{
    internal static ImpNetPlayer Instance { get; private set; }

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

    [ServerRpc(RequireOwnership = false)]
    internal void TeleportPlayerServerRpc(int playerId, ImpVector position)
    {
        TeleportPlayerClientRpc(playerId, position);
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(int playerId, ImpVector position)
    {
        PlayerManager.TeleportPlayer(position.Vector3(), playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void KillPlayerServerRpc(int playerId)
    {
        KillPlayerClientRpc(playerId);

        Imperium.Output.SendToClients(
            $"Player {Imperium.StartOfRound.allPlayerScripts[playerId].playerUsername} has been murdered!"
        );
    }

    [ClientRpc]
    private void KillPlayerClientRpc(int playerId)
    {
        // Increase the amount of living players so the ship doesn't leave when someone gets manually killed
        Imperium.StartOfRound.livingPlayers++;

        Imperium.PlayerManager.AllowPlayerDeathOverride = true;
        if (PlayerManager.IsLocalPlayer(playerId)) Imperium.Player.KillPlayer(Vector3.zero, deathAnimation: 1);
        Imperium.PlayerManager.AllowPlayerDeathOverride = false;
    }

    [ServerRpc(RequireOwnership = false)]
    internal void RespawnPlayerServerRpc(int playerId)
    {
        RespawnPlayerClientRpc(playerId);
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc(int playerId)
    {
        PlayerManager.RevivePlayer(playerId);

        Imperium.Output.Send($"Player {Imperium.StartOfRound.allPlayerScripts[playerId].playerUsername} has been revived!");
    }

    [ServerRpc(RequireOwnership = false)]
    internal void DiscardHotbarItemServerRpc(int playerId, int itemSlot)
    {
        DiscardHotbarItemClientRpc(playerId, itemSlot);
    }

    [ClientRpc]
    private void DiscardHotbarItemClientRpc(int playerId, int itemSlot)
    {
        PlayerManager.DiscardHotbarItem(playerId, itemSlot);
    }
}
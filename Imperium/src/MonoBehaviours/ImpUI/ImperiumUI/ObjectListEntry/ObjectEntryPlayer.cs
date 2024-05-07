#region

using GameNetcodeStuff;
using Imperium.Core;
using Imperium.Netcode;
using Imperium.Types;
using Imperium.Util;
using Unity.Netcode;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI.ObjectListEntry;

internal class ObjectEntryPlayer : ObjectEntry
{
    protected override bool CanDestroy() => false;
    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;
    protected override bool CanToggle() => true;

    protected override bool CanKill() => !((PlayerControllerB)component).isPlayerDead;
    protected override bool CanRevive() => ((PlayerControllerB)component).isPlayerDead;
    protected override bool CanTeleportHere() => !((PlayerControllerB)component).isPlayerDead;

    protected override string GetObjectName()
    {
        var player = (PlayerControllerB)component;
        var playerName = player.playerUsername;
        if (string.IsNullOrEmpty(playerName)) playerName = $"Player {component.GetInstanceID()}";

        // Check if player is also using Imperium
        if (ImpNetCommunication.Instance.ImperiumUsers.Contains(player.playerClientId))
        {
            playerName = $"[I] {playerName}";
        }

        if (player.isPlayerControlled)
        {
            if (player.isInHangarShipRoom)
            {
                playerName += " (In Ship)";
            }
            else if (player.isInsideFactory)
            {
                playerName += " (Indoors)";
            }
            else
            {
                playerName += " (Outdoors)";
            }
        }

        return player.isPlayerDead ? ImpUtils.RichText.Strikethrough(playerName) : playerName;
    }

    protected override void Kill()
    {
        ImpNetPlayer.Instance.KillPlayerServerRpc(PlayerManager.GetPlayerID((PlayerControllerB)component));
        UpdateEntry();
    }

    protected override void Revive()
    {
        ImpNetPlayer.Instance.RespawnPlayerServerRpc(PlayerManager.GetPlayerID((PlayerControllerB)component));
        UpdateEntry();
    }

    protected override void TeleportHere()
    {
        Imperium.ImpPositionIndicator.Activate(position =>
        {
            ((PlayerControllerB)component).TeleportPlayer(position);
            // ImpNetPlayer.Instance.TeleportPlayerServerRpc(
            //     PlayerManager.GetPlayerID((PlayerControllerB)component),
            //     new ImpVector(position)
            // );
        }, Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null);
        Imperium.Interface.Close();
    }

    public override void UpdateEntry()
    {
        base.UpdateEntry();

        reviveButton.gameObject.SetActive(CanRevive());
        killButton.gameObject.SetActive(CanKill());
    }
}
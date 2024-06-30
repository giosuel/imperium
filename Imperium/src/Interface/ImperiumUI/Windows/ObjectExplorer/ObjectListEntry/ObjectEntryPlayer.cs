#region

using GameNetcodeStuff;
using Imperium.API.Types.Networking;
using Imperium.Util;
using Unity.Netcode;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntryPlayer : ObjectEntry
{
    protected override bool CanDestroy() => false;
    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;
    protected override bool CanToggle() => false;

    protected override bool CanKill() => !((PlayerControllerB)component).isPlayerDead;
    protected override bool CanRevive() => ((PlayerControllerB)component).isPlayerDead;
    protected override bool CanTeleportHere() => !((PlayerControllerB)component).isPlayerDead;

    protected override string GetObjectName()
    {
        var player = (PlayerControllerB)component;
        var playerName = player.playerUsername;
        if (string.IsNullOrEmpty(playerName)) playerName = $"Player {component.GetInstanceID()}";

        // Check if player is also using Imperium
        if (Imperium.Networking.ImperiumUsers.Value.Contains(player.playerClientId))
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

        if (player.playerClientId == NetworkManager.ServerClientId)
        {
            playerName = RichText.Bold(playerName);
        }

        return player.isPlayerDead ? RichText.Strikethrough(playerName) : playerName;
    }

    protected override void Kill()
    {
        Imperium.PlayerManager.KillPlayer(((PlayerControllerB)component).playerClientId);
        UpdateEntry();
    }

    protected override void Revive()
    {
        Imperium.PlayerManager.RevivePlayer(((PlayerControllerB)component).playerClientId);
        UpdateEntry();
    }

    protected override void TeleportHere()
    {
        Imperium.ImpPositionIndicator.Activate(position =>
        {
            Imperium.PlayerManager.TeleportPlayer(new TeleportPlayerRequest
            {
                PlayerId = ((PlayerControllerB)component).playerClientId,
                Destination = position
            });
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
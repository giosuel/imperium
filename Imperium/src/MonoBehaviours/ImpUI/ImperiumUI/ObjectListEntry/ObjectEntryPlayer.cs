#region

using GameNetcodeStuff;
using Imperium.Core;
using Imperium.Netcode;
using Imperium.Types;

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
        var playerName = ((PlayerControllerB)component).playerUsername;
        if (string.IsNullOrEmpty(playerName)) playerName = $"Player <i>{component.GetInstanceID()}</i>";
        if (((PlayerControllerB)component).isPlayerDead) playerName = $"<s>{playerName}</s>";

        return playerName;
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
        Imperium.PositionIndicator.Activate(position =>
        {
            ImpNetPlayer.Instance.TeleportPlayerServerRpc(
                PlayerManager.GetPlayerID((PlayerControllerB)component),
                new ImpVector(position)
            );
        });
        Imperium.Interface.Close();
    }

    public override void UpdateEntry()
    {
        SetName(GetObjectName());

        reviveButton.gameObject.SetActive(CanRevive());
        killButton.gameObject.SetActive(CanKill());
    }
}
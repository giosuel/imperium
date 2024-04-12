#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.TeleportUI.Windows;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.TeleportUI;

internal class TeleportUI : MultiplexUI
{
    public override void Awake() => InitializeUI(closeOnMovement: false);

    protected override void InitUI()
    {
        RegisterWindow<TeleportWindow>("Teleport", false);
        RegisterWindow<WaypointWindow>("Waypoints");

        Imperium.InputBindings.BaseMap["Teleport"].performed += OnTeleport;
    }

    private static void OnTeleport(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat) return;

        // Set origin of indicator to freecam if freecam is enabled
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(PlayerManager.TeleportTo, origin);
    }
}
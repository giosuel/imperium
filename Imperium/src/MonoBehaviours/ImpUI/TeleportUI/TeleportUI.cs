#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.TeleportUI.Windows;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.TeleportUI;

internal class TeleportUI : MultiplexUI
{
    protected override void InitUI(
        )
    {
        RegisterWindow<TeleportWindow>("Teleport", theme);
        RegisterWindow<WaypointWindow>("Waypoints", theme);

        Imperium.InputBindings.BaseMap["Teleport"].performed += OnTeleport;
    }

    private static void OnTeleport(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat) return;

        // Set origin of indicator to freecam if freecam is enabled
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        
        if (Imperium.ImpPositionIndicator.IsActive)
        {
            Imperium.ImpPositionIndicator.HideIndicator();
        }
        else
        {
            Imperium.ImpPositionIndicator.Activate(PlayerManager.TeleportTo, origin);
        }
    }
}
#region

using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Input;

public class ImpInputBindings
{
    internal readonly InputActionMap StaticMap = new();
    internal readonly ImpInputBaseMap BaseMap = new();
    internal readonly ImpInputFreecamMap FreecamMap = new();
    internal readonly ImpInputInterfaceMap InterfaceMap = new();

    private static float SpaceDoubleClickTimer;

    internal ImpInputBindings()
    {
        StaticMap.AddAction("Alt", binding: "<Keyboard>/alt");
        StaticMap.Enable();

        BaseMap.ToggleFlight.performed += _ =>
        {
            if (Imperium.Settings.Player.EnableFlying.Value
                && Imperium.IsImperiumEnabled
                && !Imperium.Player.quickMenuManager.isMenuOpen
                && !Imperium.Player.inTerminalMenu
                && !Imperium.Player.isTypingChat
                && !Imperium.Freecam.IsFreecamEnabled.Value
                && !Imperium.PlayerManager.FlyIsAscending
                && !Imperium.PlayerManager.FlyIsDescending)
            {
                if (Time.realtimeSinceStartup - SpaceDoubleClickTimer < 0.63)
                {
                    Imperium.PlayerManager.IsFlying.Toggle();
                    Imperium.PlayerManager.FlyIsAscending = true;
                    Imperium.Map.SetCameraClipped(!Imperium.PlayerManager.IsFlying.Value);
                    SpaceDoubleClickTimer = Time.realtimeSinceStartup - 2f;
                }
                else
                {
                    SpaceDoubleClickTimer = Time.realtimeSinceStartup;
                }
            }
        };

        BaseMap.FlyAscend.performed += _ => Imperium.PlayerManager.FlyIsAscending = true;
        BaseMap.FlyAscend.canceled += _ => Imperium.PlayerManager.FlyIsAscending = false;
        BaseMap.FlyDescend.performed += _ => Imperium.PlayerManager.FlyIsDescending = true;
        BaseMap.FlyDescend.canceled += _ => Imperium.PlayerManager.FlyIsDescending = false;
    }
}
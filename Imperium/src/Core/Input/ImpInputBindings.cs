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

        BaseMap.ToggleFlight.performed += OnToggleFlight;
        BaseMap.FlyAscend.performed += OnFlyAscendPerformed;
        BaseMap.FlyAscend.canceled += OnFlyAscendCancelled;
        BaseMap.FlyDescend.performed += OnFlyDescendPerformed;
        BaseMap.FlyDescend.canceled += OnFlyDescendCancelled;
    }

    private static void OnFlyAscendPerformed(InputAction.CallbackContext _) => Imperium.PlayerManager.FlyIsAscending = true;

    private static void OnFlyAscendCancelled(InputAction.CallbackContext _) =>
        Imperium.PlayerManager.FlyIsAscending = false;

    private static void OnFlyDescendPerformed(InputAction.CallbackContext _) =>
        Imperium.PlayerManager.FlyIsDescending = true;

    private static void OnFlyDescendCancelled(InputAction.CallbackContext _) =>
        Imperium.PlayerManager.FlyIsDescending = false;

    private static void OnToggleFlight(InputAction.CallbackContext _)
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
    }
}
#region

using Imperium.Patches.Objects;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

#endregion

namespace Imperium.Core;

public class ImpInputBindings
{
    internal readonly InputActionMap BaseMap = new();
    internal readonly InputActionMap FreecamMap = new();
    internal readonly InputActionMap SpawningMap = new();
    internal static PlayerActions GameMap => Imperium.Player.playerActions;

    private static float SpaceDoubleClickTimer;
    private static float SpaceDoubleClickTimerChanged;

    internal ImpInputBindings()
    {
        BaseMap.AddAction("Teleport", binding: "<Keyboard>/t");
        BaseMap.AddAction("Freecam", binding: "<Keyboard>/f");
        BaseMap.AddAction("Minicam", binding: "<Keyboard>/x");
        BaseMap.AddAction("ToggleHUD", binding: "<Keyboard>/z");
        BaseMap.AddAction("Look", binding: "<Mouse>/delta");
        BaseMap.AddAction("LeftClick", binding: "<Mouse>/leftButton");
        BaseMap.AddAction("RightClick", binding: "<Mouse>/rightButton");
        BaseMap.AddAction("Reset", binding: "<Keyboard>/r");
        BaseMap.AddAction("Minimap", binding: "<Keyboard>/m");
        BaseMap.AddAction("Alt", binding: "<Keyboard>/alt");
        BaseMap.AddAction("Flying", binding: "<Keyboard>/space", interactions: "multiTap(tapTime=2, tapCount=2, tapDelay=0.4)");
        BaseMap.AddAction("FlyAscend", binding: "<Keyboard>/space", interactions: "hold(duration=0.1)");
        BaseMap.AddAction("FlyDescend", binding: "<Keyboard>/ctrl", interactions: "hold(duration=0.1)");
        BaseMap.Enable();

        SpawningMap.AddAction("ArrowUp", binding: "<Keyboard>/upArrow");
        SpawningMap.AddAction("ArrowDown", binding: "<Keyboard>/downArrow");
        SpawningMap.AddAction("Submit", binding: "<Keyboard>/return");

        FreecamMap.AddAction("Move");
        FreecamMap["Move"].AddCompositeBinding("dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        FreecamMap.AddAction("Ascend", binding: "<Keyboard>/space");
        FreecamMap.AddAction("Descend", binding: "<Keyboard>/ctrl");
        FreecamMap.AddAction("LayerSelector", binding: "<Keyboard>/l");
        FreecamMap.AddAction("Select", binding: "<Keyboard>/enter");
        FreecamMap.AddAction("ArrowUp", binding: "<Keyboard>/upArrow");
        FreecamMap.AddAction("ArrowDown", binding: "<Keyboard>/downArrow");
        FreecamMap.AddAction("ArrowLeft", binding: "<Keyboard>/leftArrow");
        FreecamMap.AddAction("ArrowRight", binding: "<Keyboard>/rightArrow");

        BaseMap["Flying"].performed += interaction =>
        {
            if (ImpSettings.Player.EnableFlying.Value
                && !Imperium.Player.quickMenuManager.isMenuOpen
                && !Imperium.Player.inTerminalMenu
                && !Imperium.Player.isTypingChat
                && !PlayerControllerPatch.isAscending
                && !PlayerControllerPatch.isDescending)
            {
                // if (Time.realtimeSinceStartup - SpaceDoubleClickTimerChanged > 1f
                //     && Time.realtimeSinceStartup - SpaceDoubleClickTimer < 7f / 11f)
                // {
                //     SpaceDoubleClickTimerChanged = Time.realtimeSinceStartup;
                //     Imperium.PlayerManager.IsFlying.Toggle();
                // }
                Imperium.PlayerManager.IsFlying.Toggle();

                Imperium.Map.SetCameraClipped(!Imperium.PlayerManager.IsFlying.Value);

                // if (tap.tapCount == 2)
                // {
                //     SpaceDoubleClickTimerChanged = Time.realtimeSinceStartup;
                //     Imperium.PlayerManager.IsFlying.Toggle();
                //     //SpaceDoubleClickTimer = Time.realtimeSinceStartup;
                // }
            }
        };

        BaseMap["FlyAscend"].performed += _ => PlayerControllerPatch.isAscending = true;
        BaseMap["FlyAscend"].canceled += _ => PlayerControllerPatch.isAscending = false;
        BaseMap["FlyDescend"].performed += _ => PlayerControllerPatch.isDescending = true;
        BaseMap["FlyDescend"].canceled += _ => PlayerControllerPatch.isDescending = false;
    }
}
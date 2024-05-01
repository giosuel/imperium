#region

using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core;

public class ImpInputBindings
{
    internal readonly InputActionMap BaseMap = new();
    internal readonly InputActionMap FreecamMap = new();
    internal readonly InputActionMap SpawningMap = new();

    internal static PlayerActions GameMap => Imperium.Player.playerActions;

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
        FreecamMap.AddAction("Ascend", binding: "<Keyboard>/q");
        FreecamMap.AddAction("Descend", binding: "<Keyboard>/e");
        FreecamMap.AddAction("LayerSelector", binding: "<Keyboard>/l");
        FreecamMap.AddAction("Select", binding: "<Keyboard>/space");
        FreecamMap.AddAction("ArrowUp", binding: "<Keyboard>/upArrow");
        FreecamMap.AddAction("ArrowDown", binding: "<Keyboard>/downArrow");
        FreecamMap.AddAction("ArrowLeft", binding: "<Keyboard>/leftArrow");
        FreecamMap.AddAction("ArrowRight", binding: "<Keyboard>/rightArrow");
    }
}
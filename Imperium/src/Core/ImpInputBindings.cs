#region

using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core;

public class ImpInputBindings
{
    internal readonly InputActionMap BaseMap = new();
    internal readonly InputActionMap FreecamMap = new();

    internal static PlayerActions GameMap => Imperium.Player.playerActions;

    internal ImpInputBindings()
    {
        BaseMap.AddAction("Teleport", binding: "<Keyboard>/t");
        BaseMap.AddAction("Freecam", binding: "<Keyboard>/f");
        BaseMap.Enable();

        FreecamMap.AddAction("Move");
        FreecamMap["Move"].AddCompositeBinding("dpad")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        FreecamMap.AddAction("Look", binding: "<Mouse>/delta");
        FreecamMap.AddAction("Reset", binding: "<Keyboard>/r");
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
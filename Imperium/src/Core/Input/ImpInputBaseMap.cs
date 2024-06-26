#region

using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Input;

internal sealed class ImpInputBaseMap : LcInputActions
{
    [InputAction("<Keyboard>/t", Name = "Teleport")]
    internal InputAction Teleport { get; set; }

    [InputAction("<Keyboard>/f", Name = "Freecam")]
    internal InputAction Freecam { get; set; }

    [InputAction("<Keyboard>/m", Name = "Minimap")]
    internal InputAction Minimap { get; set; }

    [InputAction("<Keyboard>/x", Name = "Minicam")]
    internal InputAction Minicam { get; set; }

    [InputAction("<Keyboard>/z", Name = "Toggle HUD")]
    internal InputAction ToggleHUD { get; set; }

    [InputAction("<Keyboard>/r", Name = "Reset")]
    internal InputAction Reset { get; set; }

    [InputAction("<Keyboard>/space", Name = "Toggle Flight")]
    internal InputAction ToggleFlight { get; set; }

    [InputAction("<Keyboard>/space", Name = "Flying Ascend", KbmInteractions = "hold(duration=0.1)")]
    internal InputAction FlyAscend { get; set; }

    [InputAction("<Keyboard>/ctrl", Name = "Flying Descend", KbmInteractions = "hold(duration=0.1)")]
    internal InputAction FlyDescend { get; set; }

    [InputAction("<Mouse>/leftButton", Name = "Map Rotate")]
    internal InputAction MapRotate { get; set; }

    [InputAction("<Mouse>/rightButton", Name = "Map Pan")]
    internal InputAction MapPan { get; set; }

    [InputAction("<Keyboard>/upArrow", Name = "Previous Item")]
    internal InputAction PreviousItem { get; set; }

    [InputAction("<Keyboard>/downArrow", Name = "Next Item")]
    internal InputAction NextItem { get; set; }

    [InputAction("<Keyboard>/return", Name = "Select Item")]
    internal InputAction SelectItem { get; set; }
}
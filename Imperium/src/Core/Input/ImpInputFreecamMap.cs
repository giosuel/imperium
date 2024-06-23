using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace Imperium.Core.Input;

internal sealed class ImpInputFreecamMap : LcInputActions
{
    [InputAction("<Keyboard>/l", Name = "Toggle Layer Selector")]
    internal InputAction LayerSelector { get; set; }

    [InputAction("<Keyboard>/enter", Name = "Toggle Layer")]
    internal InputAction ToggleLayer { get; set; }

    [InputAction("<Keyboard>/upArrow", Name = "Previous Layer")]
    internal InputAction PreviousLayer { get; set; }

    [InputAction("<Keyboard>/downArrow", Name = "Next Layer")]
    internal InputAction NextLayer { get; set; }

    [InputAction("<Keyboard>/leftArrow", Name = "Freecam Increase FOV")]
    internal InputAction IncreaseFOV { get; set; }

    [InputAction("<Keyboard>/rightArrow", Name = "Freecam Decrease FOV")]
    internal InputAction DecreaseFOV { get; set; }
}
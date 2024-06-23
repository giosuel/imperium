using LethalCompanyInputUtils.Api;
using UnityEngine.InputSystem;

namespace Imperium.Core.Input;

public class ImpInputInterfaceMap : LcInputActions
{
    [InputAction("<Keyboard>/F1", Name = "Imperium UI")]
    internal InputAction ImperiumUI { get; set; }

    [InputAction("<Keyboard>/F2", Name = "Spawning UI")]
    internal InputAction SpawningUI { get; set; }

    [InputAction("<Keyboard>/F8", Name = "Map UI")]
    internal InputAction MapUI { get; set; }

    [InputAction("<Keyboard>/F6", Name = "Oracle UI")]
    internal InputAction OracleUI { get; set; }

    [InputAction("<Keyboard>/F3", Name = "Teleportation Window")]
    internal InputAction TeleportWindow { get; set; }
}
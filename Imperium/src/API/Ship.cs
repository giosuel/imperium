using Imperium.Core.Lifecycle;

namespace Imperium.API;

public static class Ship
{
    /// <summary>
    /// Navigates the ship to another moon. Only works in orbit.
    /// </summary>
    /// <param name="levelIndex">The level index of the moon to navigate to</param>
    public static void NavigateTo(int levelIndex)
    {
        APIHelpers.AssertImperiumReady();
        APIHelpers.AssertShipInOrbit();

        Imperium.ShipManager.NavigateTo(levelIndex);
    }
}
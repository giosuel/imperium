using Imperium.API.Types.Networking;
using Imperium.Core.Lifecycle;

namespace Imperium.API;

public static class Moon
{
    /// <summary>
    /// Changes the weather on the specified moon.
    /// </summary>
    /// <param name="request"></param>
    public static void ChangeWeather(ChangeWeatherRequest request)
    {
        APIHelpers.AssertImperiumReady();

        Imperium.MoonManager.ChangeWeather(request);
    }

    /// <summary>
    /// Open or closes all doors on the map. Only works when the ship is in orbit.
    /// </summary>
    /// <param name="areOpen">Whether to open or close the doors</param>
    public static void ToggleDoors(bool areOpen)
    {
        APIHelpers.AssertImperiumReady();
        APIHelpers.AssertShipLanded();

        MoonManager.ToggleDoors(areOpen);
    }

    /// <summary>
    /// Open or closes a specific door on the map. Only works when the ship is landed.
    /// </summary>
    /// <param name="door">The target door</param>
    /// <param name="isOpen">Whether to open or close the door</param>
    public static void ToggleDoor(DoorLock door, bool isOpen)
    {
        APIHelpers.AssertImperiumReady();
        APIHelpers.AssertShipLanded();

        MoonManager.ToggleDoor(door, isOpen);
    }
}
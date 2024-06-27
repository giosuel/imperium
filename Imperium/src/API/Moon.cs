using Imperium.API.Types.Networking;

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
}
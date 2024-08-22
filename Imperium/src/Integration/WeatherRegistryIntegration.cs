#region

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using WeatherRegistry;

#endregion

namespace Imperium.Integration;

public static class WeatherRegistryIntegration
{
    internal static bool IsEnabled => Chainloader.PluginInfos.ContainsKey("mrov.WeatherRegistry");

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static List<LevelWeatherType> GetWeathers()
    {
        if (!IsEnabled)
            return null;

        return WeatherManager.Weathers.Select(weather => weather.VanillaWeatherType)
            .ToList();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void ChangeWeather(SelectableLevel level, LevelWeatherType weather)
    {
        if (!IsEnabled)
            return;

        WeatherController.ChangeWeather(level, weather);
    }
}
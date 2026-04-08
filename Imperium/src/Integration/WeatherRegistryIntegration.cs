#region

using System.Collections.Generic;
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

        List<LevelWeatherType> weatherTypes = [];
        foreach (var weather in WeatherManager.Weathers)
        {
            weatherTypes.Add(weather.VanillaWeatherType);
        }

        return weatherTypes;
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void ChangeWeather(SelectableLevel level, LevelWeatherType weather)
    {
        if (!IsEnabled)
            return;

        WeatherController.ChangeWeather(level, weather);
    }
}
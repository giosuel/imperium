#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using HarmonyLib;
using Imperium.Util;

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

        return WeatherRegistry
            .WeatherManager.Weathers.Select(weather => weather.VanillaWeatherType)
            .ToList();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void ChangeWeather(SelectableLevel level, LevelWeatherType weather)
    {
        if (!IsEnabled)
            return;

        WeatherRegistry.WeatherController.ChangeWeather(level, weather);
    }
}

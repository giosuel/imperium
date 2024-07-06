using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Integration;

namespace Imperium.Util;

public static class WeatherData
{
    public static List<LevelWeatherType> Weathers
    {
        get
        {
            if (WeatherRegistryIntegration.IsEnabled)
            {
                return WeatherRegistryIntegration.GetWeathers();
            }

            return Enum.GetValues(typeof(LevelWeatherType))
                .Cast<LevelWeatherType>()
                .OrderBy(enumValue => enumValue)
                .ToList();
        }
    }
}

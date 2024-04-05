#region

using System.Collections.Generic;

#endregion

namespace Imperium.Types;

public record MoonData
{
    public Dictionary<string, int> IndoorEntityRarities = [];
    public Dictionary<string, int> OutdoorEntityRarities = [];
    public Dictionary<string, int> DaytimeEntityRarities = [];

    public Dictionary<string, int> ScrapRarities = [];

    public int maxIndoorPower;
    public int maxOutdoorPower;
    public int maxDaytimePower;

    public float indoorDeviation;
    public float daytimeDeviation;
}
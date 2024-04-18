#region

using System.Collections.Generic;

#endregion

namespace Imperium.Types;

public record MoonData
{
    public Dictionary<EnemyType, int> IndoorEntityRarities = [];
    public Dictionary<EnemyType, int> OutdoorEntityRarities = [];
    public Dictionary<EnemyType, int> DaytimeEntityRarities = [];

    public Dictionary<Item, int> ScrapRarities = [];

    public int maxIndoorPower;
    public int maxOutdoorPower;
    public int maxDaytimePower;

    public float indoorDeviation;
    public float daytimeDeviation;
}
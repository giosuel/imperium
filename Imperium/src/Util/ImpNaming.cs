#region

using System.Collections.Generic;

#endregion

namespace Imperium.Util;

public abstract class ImpNaming
{
    private static readonly Dictionary<string, string> RenameMap = new()
    {
        { "centipede", "Snare Flea" },
        { "bunker spider", "Bunker Spider" },
        { "hoarding bug", "Hoarding Bug" },
        { "flowerman", "Bracken" },
        { "crawler", "Thumper" },
        { "blob", "Hygrodere" },
        { "girl", "Ghost Girl" },
        { "puffer", "Spore Lizard" },
        { "nutcracker", "Nutcracker" },
        { "masked", "masked" },
        { "spring", "Coil Head" },
        { "jester", "Jester" },
        { "lasso", "Lasso" },
        { "red pill", "Red Pill" },
        { "mouthdog", "Eyeless Dog" },
        { "forestgiant", "Forest Keeper" },
        { "earth leviathan", "Earth Leviathan" },
        { "baboon hawk", "Baboon Hawk" },
        { "red locust bees", "Circuit Bees" },
        { "manticoil", "Manticoil" },
        { "docile locust bees", "Roaming Locusts" },
    };

    // internal static string Get(string key)
    // {
    //     return RenameMap.GetValueOrDefault(key.ToLower(), key);
    // }
}
namespace Imperium.API;

internal static class APIHelpers
{
    /// <summary>
    ///     Throws an <see cref="ImperiumAPIException" /> when Imperium is not ready to serve API calls.
    /// </summary>
    /// <exception cref="ImperiumAPIException">When Imperium is not ready to serve API calls.</exception>
    internal static void AssertImperiumReady()
    {
        if (Imperium.IsImperiumLaunched) return;

        throw new ImperiumAPIException("Failed to execute API call. Imperium has not yet been initialized.");
    }

    internal static void AssertShipLanded()
    {
        if (Imperium.IsSceneLoaded.Value) return;

        throw new ImperiumAPIException("Ship is not currently in orbit.");
    }

    internal static void AssertShipInOrbit()
    {
        if (!Imperium.IsSceneLoaded.Value) return;

        throw new ImperiumAPIException("Ship is not currently in orbit.");
    }
}
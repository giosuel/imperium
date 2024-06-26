namespace Imperium.API;

internal static class APIHelpers
{
    /// <summary>
    /// Throws an <see cref="ImperiumAPIException"/> when Imperium is not ready to serve API calls.
    /// </summary>
    /// <exception cref="ImperiumAPIException">When Imperium is not ready to serve API calls.</exception>
    internal static void AssertImperiumReady()
    {
        if (Imperium.IsImperiumLaunched) return;

        throw new ImperiumAPIException(
            "Failed to execute API call. Imperium has not yet been initialized."
        );
    }
}
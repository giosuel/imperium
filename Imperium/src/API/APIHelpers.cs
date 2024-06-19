namespace Imperium.API;

internal static class APIHelpers
{
    internal static void AssertImperiumReady()
    {
        if (Imperium.IsImperiumLaunched) return;

        throw new ImperiumAPIException(
            "Failed to execute API call. Imperium has not yet been initialized."
        );
    }
}
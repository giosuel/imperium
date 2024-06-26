#region

using Imperium.Util.Binding;

#endregion

namespace Imperium.API;

public static class Events
{
    /// <summary>
    ///     Is called when the moon scene is either loaded or unloaded.
    /// </summary>
    public static IBinding<bool> IsSceneLoaded => Imperium.IsSceneLoaded;
}
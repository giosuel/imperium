#region

using Imperium.Util.Binding;

#endregion

namespace Imperium.API;

public static class Events
{
    /// <summary>
    ///     Is called when the moon scene is either loaded or unloaded.
    /// </summary>
    public static ImpBinding<bool> IsSceneLoaded => Imperium.IsSceneLoaded;
}
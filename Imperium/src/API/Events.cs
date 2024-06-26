#region

using Imperium.Netcode;
using Imperium.Util.Binding;

#endregion

namespace Imperium.API;

public static class Events
{
    /// <summary>
    ///     Is called when the moon scene is either loaded or unloaded.
    /// </summary>
    public static ReadOnlyBinding<bool> IsSceneLoaded => ReadOnlyBinding<bool>.Wrap(Imperium.IsSceneLoaded);

    /// <summary>
    /// The amount of players, including the host, that are currently connected to the game.
    /// </summary>
    public static ReadOnlyBinding<int> ConnectedPlayers => ReadOnlyBinding<int>.Wrap(ImpNetworking.ConnectedPlayers);
}
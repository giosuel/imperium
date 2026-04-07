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
    public static ImpImmutableBinding<bool> IsSceneLoaded => ImpImmutableBinding<bool>.Wrap(Imperium.IsSceneLoaded);

    /// <summary>
    ///     The amount of players, including the host, that are currently connected to the game.
    /// </summary>
    public static ImpImmutableBinding<int> ConnectedPlayers =>
        ImpImmutableBinding<int>.Wrap(ImpNetworking.ConnectedPlayers);
}
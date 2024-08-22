#region

using Imperium.Netcode;

#endregion

namespace Imperium.Core.Lifecycle;

internal class CruiserManager : ImpLifecycleObject
{
    internal readonly ImpNetworkBinding<bool> Indestructible = new(
        "CruiserIndestructible",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Cruiser.Indestructible
    );

    internal readonly ImpNetworkBinding<bool> InfiniteTurbo = new(
        "InfiniteTurbo",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Cruiser.InfiniteTurbo
    );

    internal readonly ImpNetworkBinding<bool> SpawnFullTurbo = new(
        "SpawnFullTurbo",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Cruiser.SpawnFullTurbo
    );

    internal readonly ImpNetworkBinding<float> PushForce = new(
        "PushForce",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Cruiser.PushForce
    );

    internal readonly ImpNetworkBinding<float> Acceleration = new(
        "Speed",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Cruiser.Acceleration
    );
}
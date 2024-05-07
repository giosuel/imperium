#region

using Imperium.Core;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// This is a network behaviour so the members have to not be static
public class ImpNetWeather : NetworkBehaviour
{
    internal static ImpNetWeather Instance { get; private set; }

    public override void OnNetworkSpawn()
    {
        if (ImpNetworkManager.IsHost.Value && Instance)
        {
            Instance.gameObject.GetComponent<NetworkObject>().Despawn();
        }

        Instance = this;
        base.OnNetworkSpawn();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void ChangeWeatherServerRpc(int levelIndex, LevelWeatherType weatherType)
    {
        if (!ImpSettings.Preferences.AllowClients.Value) return;

        OnWeatherChangedClientRpc(levelIndex, weatherType);
    }

    [ClientRpc]
    private void OnWeatherChangedClientRpc(int levelIndex, LevelWeatherType weatherType)
    {
        GameManager.ChangeWeather(levelIndex, weatherType);
    }
}
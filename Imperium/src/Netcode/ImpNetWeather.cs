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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance.gameObject);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    internal void ChangeWeatherServerRpc(int levelIndex, LevelWeatherType weatherType)
    {
        OnWeatherChangedClientRpc(levelIndex, weatherType);
    }

    [ClientRpc]
    private void OnWeatherChangedClientRpc(int levelIndex, LevelWeatherType weatherType)
    {
        GameManager.ChangeWeather(levelIndex, weatherType);
    }
}
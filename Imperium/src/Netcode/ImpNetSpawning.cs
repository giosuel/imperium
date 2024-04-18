#region

using Imperium.Types;
using Unity.Netcode;

#endregion

namespace Imperium.Netcode;

// ReSharper disable MemberCanBeMadeStatic.Global
// This is a network behaviour so the members have to not be static
public class ImpNetSpawning : NetworkBehaviour
{
    internal static ImpNetSpawning Instance { get; private set; }

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
    internal void SpawnEntityServerRpc(
        string entityName,
        string prefabName,
        ImpVector position,
        int amount,
        int health
    )
    {
        Imperium.ObjectManager.SpawnEntityServer(entityName, prefabName, position.Vector3(), amount, health);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SpawnItemServerRpc(
        string itemName,
        string prefabName,
        int spawningPlayerId,
        ImpVector position,
        int amount,
        int value
    )
    {
        Imperium.ObjectManager.SpawnItemServer(itemName, prefabName, spawningPlayerId, position.Vector3(), amount,
            value);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SpawnMapHazardServerRpc(
        string objectName,
        ImpVector position,
        int amount
    )
    {
        Imperium.ObjectManager.SpawnMapHazardServer(objectName, position.Vector3(), amount);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void DespawnMapHazardServerRpc(ulong netId)
    {
        if (Imperium.ObjectManager.DespawnObject(netId)) OnMapHazardsChangedClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void DespawnEntityServerRpc(ulong netId)
    {
        if (Imperium.ObjectManager.DespawnObject(netId)) OnEntitiesChangedClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    internal void DespawnItemServerRpc(ulong netId)
    {
        if (Imperium.ObjectManager.DespawnObject(netId)) OnItemsChangedClientRpc();
    }

    [ClientRpc]
    public void OnMapHazardsChangedClientRpc()
    {
        Imperium.ObjectManager.RefreshLevelObstacles();
    }

    [ClientRpc]
    public void OnEntitiesChangedClientRpc()
    {
        Imperium.ObjectManager.RefreshLevelEntities();
    }

    [ClientRpc]
    public void OnItemsChangedClientRpc()
    {
        Imperium.ObjectManager.RefreshLevelItems();
    }

    [ServerRpc]
    public void OnSpawningChangedServerRpc()
    {
        // TODO(giosuel): Implement syncing of spawn lists
        // OnSpawningChangedClientRpc();
    }

    // [ClientRpc]
    // private void OnSpawningChangedClientRpc()
    // {
    //     Oracle.Simulate();
    //     Imperium.MoonUI.SpawnListsWindow.Refresh();
    // }

    [ServerRpc(RequireOwnership = false)]
    internal void SetMaxIndoorPowerServerRpc(float value)
    {
        SetMaxIndoorPowerClientRpc(value);
    }

    [ClientRpc]
    private void SetMaxIndoorPowerClientRpc(float value)
    {
        Imperium.GameManager.MaxIndoorPower.Set(value, skipSync: true);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetMaxOutdoorPowerServerRpc(float value)
    {
        SetMaxOutdoorPowerClientRpc(value);
    }

    [ClientRpc]
    private void SetMaxOutdoorPowerClientRpc(float value)
    {
        Imperium.GameManager.MaxOutdoorPower.Set(value, skipSync: true);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetMaxDaytimePowerServerRpc(int value)
    {
        SetMaxDaytimePowerClientRpc(value);
    }

    [ClientRpc]
    private void SetMaxDaytimePowerClientRpc(int value)
    {
        Imperium.GameManager.MaxDaytimePower.Set(value, skipSync: true);
    }


    [ServerRpc(RequireOwnership = false)]
    internal void SetMinIndoorEntitiesServerRpc(int value)
    {
        SetMinIndoorEntitiesClientRpc(value);
    }

    [ClientRpc]
    private void SetMinIndoorEntitiesClientRpc(int value)
    {
        Imperium.GameManager.MinIndoorSpawns.Set(value, skipSync: true);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetMinOutdoorEntitiesServerRpc(int value)
    {
        SetMinOutdoorEntitiesClientRpc(value);
    }

    [ClientRpc]
    private void SetMinOutdoorEntitiesClientRpc(int value)
    {
        Imperium.GameManager.MinOutdoorSpawns.Set(value, skipSync: true);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetIndoorDeviationServerRpc(float value)
    {
        SetIndoorDeviationClientRpc(value);
    }

    [ClientRpc]
    private void SetIndoorDeviationClientRpc(float value)
    {
        Imperium.GameManager.IndoorDeviation.Set(value, skipSync: true);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetDaytimeDeviationServerRpc(float value)
    {
        SetDaytimeDeviationClientRpc(value);
    }

    [ClientRpc]
    private void SetDaytimeDeviationClientRpc(float value)
    {
        Imperium.GameManager.DaytimeDeviation.Set(value, skipSync: true);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetWeatherVariable1ServerRpc(float value)
    {
        SetWeatherVariable1ClientRpc(value);
    }

    [ClientRpc]
    private void SetWeatherVariable1ClientRpc(float value)
    {
        Imperium.GameManager.WeatherVariable1.Set(value, skipSync: true);
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SetWeatherVariable2ServerRpc(float value)
    {
        SetWeatherVariable2ClientRpc(value);
    }

    [ClientRpc]
    private void SetWeatherVariable2ClientRpc(float value)
    {
        Imperium.GameManager.WeatherVariable2.Set(value, skipSync: true);
    }
}
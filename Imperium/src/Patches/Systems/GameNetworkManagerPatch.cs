#region

using HarmonyLib;
using Imperium.Netcode;
using Imperium.Util;
using Unity.Netcode;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(GameNetworkManager))]
internal static class GameNetworkManagerPatch
{
    private static bool hasSubscribedToConnectionCallbacks;

    [HarmonyPostfix]
    [HarmonyPatch("SubscribeToConnectionCallbacks")]
    private static void SubscribeToConnectionCallbacksPatch(GameNetworkManager __instance)
    {
        if (!hasSubscribedToConnectionCallbacks)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += ImpNetworking.OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ImpNetworking.OnClientDisconnected;
            hasSubscribedToConnectionCallbacks = true;
        }
    }

    // [HarmonyPatch(typeof(GameNetworkManager))]
    // internal static class PreloadPatches
    // {
    //     [HarmonyPostfix]
    //     [HarmonyPatch(typeof(GameNetworkManager), "Start")]
    //     private static void StartPatch()
    //     {
    //         if (ImpNetworkManager.NetworkPrefab != null) return;
    //
    //         ImpNetworkManager.NetworkPrefab = ImpAssets.NetworkHandler;
    //         ImpNetworkManager.NetworkPrefab.AddComponent<ImpNetCommunication>();
    //         ImpNetworkManager.NetworkPrefab.AddComponent<ImpNetPlayer>();
    //         ImpNetworkManager.NetworkPrefab.AddComponent<ImpNetQuota>();
    //         ImpNetworkManager.NetworkPrefab.AddComponent<ImpNetSpawning>();
    //         ImpNetworkManager.NetworkPrefab.AddComponent<ImpNetTime>();
    //         ImpNetworkManager.NetworkPrefab.AddComponent<ImpNetWeather>();
    //
    //         NetworkManager.Singleton.AddNetworkPrefab(ImpNetworkManager.NetworkPrefab);
    //     }
    // }
}
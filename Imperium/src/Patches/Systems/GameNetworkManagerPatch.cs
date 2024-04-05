#region

using HarmonyLib;
using Imperium.Netcode;
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
            NetworkManager.Singleton.OnClientConnectedCallback += ImpNetworkManager.OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += ImpNetworkManager.OnClientDisconnected;
            hasSubscribedToConnectionCallbacks = true;
        }
    }
}
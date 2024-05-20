#region

using System.Collections.Generic;
using GameNetcodeStuff;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class PlayerInfos : BaseVisualizer<HashSet<PlayerControllerB>>
{
    internal readonly Dictionary<PlayerControllerB, PlayerInfoConfig> PlayerInfoConfigs = [];

    internal PlayerInfos(
        ImpBinding<HashSet<PlayerControllerB>> objectsBinding
    ) : base("Player Infos", objectsBinding)
    {
        foreach (var player in Imperium.StartOfRound.allPlayerScripts)
        {
            PlayerInfoConfigs[player] = new PlayerInfoConfig(player.playerUsername);
        }
    }

    protected override void Refresh(HashSet<PlayerControllerB> objects)
    {
        ClearObjects();

        foreach (var player in objects)
        {
            if (!indicatorObjects.ContainsKey(player.GetInstanceID()))
            {
                var parent = player.transform;
                var playerInfoObject = Object.Instantiate(ImpAssets.PlayerInfo, parent, true);
                playerInfoObject.transform.position = parent.position + Vector3.up * 1.4f;
                playerInfoObject.transform.localScale = Vector3.one * 0.4f;

                var playerInfo = playerInfoObject.AddComponent<PlayerInfo>();
                playerInfo.Init(PlayerInfoConfigs[player], player.GetComponent<PlayerControllerB>());

                indicatorObjects[player.GetInstanceID()] = playerInfoObject;
            }
        }
    }
}
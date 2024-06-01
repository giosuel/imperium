#region

using System.Collections.Generic;
using GameNetcodeStuff;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class PlayerInfos : BaseVisualizer<HashSet<PlayerControllerB>>
{
    internal readonly Dictionary<PlayerControllerB, PlayerInfoConfig> PlayerInfoConfigs = [];

    internal PlayerInfos(ImpBinding<HashSet<PlayerControllerB>> objectsBinding) : base(objectsBinding)
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
                var playerInfoObject = new GameObject($"Imp_PlayerInfo_{player.GetInstanceID()}");
                var playerInfo = playerInfoObject.AddComponent<PlayerInfo>();
                playerInfo.Init(PlayerInfoConfigs[player], player.GetComponent<PlayerControllerB>());

                indicatorObjects[player.GetInstanceID()] = playerInfoObject;
            }
        }
    }
}
#region

using System.Collections.Generic;
using GameNetcodeStuff;
using Imperium.MonoBehaviours.VisualizerObjects;
using Imperium.Util.Binding;
using Imperium.Visualizers.MonoBehaviours;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class PlayerGizmos : BaseVisualizer<HashSet<PlayerControllerB>, PlayerGizmo>
{
    internal readonly Dictionary<PlayerControllerB, PlayerGizmoConfig> PlayerInfoConfigs = [];

    internal PlayerGizmos(ImpBinding<HashSet<PlayerControllerB>> objectsBinding) : base(objectsBinding)
    {
        foreach (var player in Imperium.StartOfRound.allPlayerScripts)
        {
            PlayerInfoConfigs[player] = new PlayerGizmoConfig(player.playerUsername);
        }
    }

    protected override void OnRefresh(HashSet<PlayerControllerB> objects)
    {
        ClearObjects();

        foreach (var player in objects)
        {
            if (!visualizerObjects.ContainsKey(player.GetInstanceID()))
            {
                var playerGizmoObject = new GameObject($"Imp_PlayerInfo_{player.GetInstanceID()}");
                var playerGizmo = playerGizmoObject.AddComponent<PlayerGizmo>();
                playerGizmo.Init(PlayerInfoConfigs[player], player.GetComponent<PlayerControllerB>());

                visualizerObjects[player.GetInstanceID()] = playerGizmo;
            }
        }
    }

    internal void PlayerNoiseUpdate(PlayerControllerB player, float range)
    {
        visualizerObjects[player.GetInstanceID()].NoiseUpdate(range);
    }

    /*
     * Player Hit Ground
     * Player Footstep
     * Player Voice
     */
}
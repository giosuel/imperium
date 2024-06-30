#region

using System.Collections.Generic;
using BepInEx.Configuration;
using GameNetcodeStuff;
using Imperium.API.Types;
using Imperium.Util.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class PlayerGizmos : BaseVisualizer<IReadOnlyCollection<PlayerControllerB>, PlayerGizmo>
{
    internal readonly Dictionary<PlayerControllerB, PlayerGizmoConfig> PlayerInfoConfigs = [];

    internal PlayerGizmos(
        IBinding<IReadOnlyCollection<PlayerControllerB>> objectsBinding, ConfigFile config
    ) : base(objectsBinding)
    {
        foreach (var player in Imperium.StartOfRound.allPlayerScripts)
        {
            PlayerInfoConfigs[player] = new PlayerGizmoConfig(player.playerUsername, config);
        }
    }

    protected override void OnRefresh(IReadOnlyCollection<PlayerControllerB> objects)
    {
        ClearObjects();

        foreach (var player in objects)
        {
            if (!visualizerObjects.ContainsKey(player.GetInstanceID()))
            {
                var playerGizmoObject = new GameObject($"Imp_PlayerInfo_{player.GetInstanceID()}");
                var playerGizmo = playerGizmoObject.AddComponent<PlayerGizmo>();
                if (!PlayerInfoConfigs.TryGetValue(player, out var playerInfoConfig))
                {
                    Imperium.IO.LogInfo("[ERR] Player was not found, no config loaded for insight.");
                    continue;
                }

                playerGizmo.Init(playerInfoConfig, player.GetComponent<PlayerControllerB>());
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
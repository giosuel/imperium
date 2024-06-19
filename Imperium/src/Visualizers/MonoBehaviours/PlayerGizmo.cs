#region

using BepInEx.Configuration;
using GameNetcodeStuff;
using Imperium.API;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers.MonoBehaviours;

public class PlayerGizmo : MonoBehaviour
{
    private PlayerControllerB playerController;
    private PlayerGizmoConfig playerGizmoConfig;

    private GameObject noiseRangeSphere;

    internal void Init(PlayerGizmoConfig config, PlayerControllerB player)
    {
        playerGizmoConfig = config;
        playerController = player;

        noiseRangeSphere = ImpGeometry.CreatePrimitive(
            PrimitiveType.Sphere, player.transform, Materials.WireframeRed
        );
    }

    internal void NoiseUpdate(float range)
    {
    }

    private void Update()
    {
        if (!playerController)
        {
            Destroy(gameObject);
            return;
        }

        DrawNoiseRange(playerGizmoConfig.NoiseRange.Value);
    }

    private void DrawNoiseRange(bool isShown)
    {
        if (!isShown)
        {
            noiseRangeSphere.SetActive(false);
        }
    }
}

internal class PlayerGizmoConfig
{
    internal readonly string playerName;

    internal readonly ImpBinding<bool> NoiseRange;

    internal PlayerGizmoConfig(string playerName, ConfigFile config)
    {
        this.playerName = playerName;
        NoiseRange = new ImpConfig<bool>(config, "Visualization.PlayerGizmos", "NoiseRange", false);
    }
}
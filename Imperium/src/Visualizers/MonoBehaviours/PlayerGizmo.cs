#region

using GameNetcodeStuff;
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
    private float timeSinceSpecialNoise;

    internal void Init(PlayerGizmoConfig config, PlayerControllerB player)
    {
        playerGizmoConfig = config;
        playerController = player;

        noiseRangeSphere = ImpGeometry.CreatePrimitive(
            PrimitiveType.Sphere, player.transform, ImpAssets.WireframeRedMaterial
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
            return;
        }
    }
}

internal class PlayerGizmoConfig
{
    internal readonly string playerName;

    internal readonly ImpBinding<bool> NoiseRange;

    internal PlayerGizmoConfig(string playerName)
    {
        this.playerName = playerName;
        NoiseRange = new ImpConfig<bool>("Visualization.PlayerGizmos", "NoiseRange", false);
    }
}
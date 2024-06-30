#region

using UnityEngine;

#endregion

namespace Imperium.Core.Scripts;

public class ImpNightVision : MonoBehaviour
{
    private Light FarLight;
    private Light NearLight;

    internal static ImpNightVision Create() => new GameObject("Imp_NightVision").AddComponent<ImpNightVision>();

    private void Awake()
    {
        var mapCamera = GameObject.Find("MapCamera").transform;
        transform.SetParent(Imperium.Player.gameplayCamera.transform);

        NearLight = new GameObject("Near").AddComponent<Light>();
        NearLight.transform.SetParent(transform);
        NearLight.transform.position = mapCamera.position + Vector3.down * 80f;
        NearLight.range = 70f;
        NearLight.color = new Color(0.875f, 0.788f, 0.791f, 1);

        FarLight = new GameObject("Far").AddComponent<Light>();
        FarLight.transform.SetParent(transform);
        FarLight.transform.position = mapCamera.position + Vector3.down * 30f;
        FarLight.range = 500f;
    }

    private void Update()
    {
        FarLight.enabled = Imperium.Player.nightVision.enabled;
        NearLight.enabled = Imperium.Player.nightVision.enabled;

        NearLight.intensity = Imperium.Settings.Player.NightVision.Value * 100f;
        FarLight.intensity = Imperium.Settings.Player.NightVision.Value * 1100f;
    }
}
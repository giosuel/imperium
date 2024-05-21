#region

using Imperium.Core;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours;

public class ImpNightVision : MonoBehaviour
{
    private Light FarLight;
    private Light NearLight;

    internal static ImpNightVision Create() => new GameObject("ImpNightVision").AddComponent<ImpNightVision>();

    private void Awake()
    {
        var mapCamera = GameObject.Find("MapCamera").transform;
        transform.SetParent(mapCamera);

        NearLight = new GameObject("Near").AddComponent<Light>();
        NearLight.transform.SetParent(transform);
        NearLight.transform.position = mapCamera.position + Vector3.up * 20f;
        NearLight.range = 70f;
        NearLight.color = new Color(0.875f, 0.788f, 0.791f, 1);

        FarLight = new GameObject("Far").AddComponent<Light>();
        FarLight.transform.SetParent(transform);
        FarLight.transform.position = mapCamera.position + Vector3.up * 70f;
        FarLight.range = 500f;
    }

    private void Update()
    {
        FarLight.enabled = Imperium.Player.nightVision.enabled;
        NearLight.enabled = Imperium.Player.nightVision.enabled;

        NearLight.intensity = ImpSettings.Player.NightVision.Value * 100f;
        FarLight.intensity = ImpSettings.Player.NightVision.Value * 1100f;
    }
}
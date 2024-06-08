#region

using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects.NoiseOverlay;

internal class ImpNoiseListener : MonoBehaviour, INoiseListener
{
    private readonly NoiseIndicator[] noiseArrows = new NoiseIndicator[20];
    private int noiseArrowIndex;

    internal static ImpNoiseListener Create()
    {
        var noiseListenerObj = ImpGeometry.CreatePrimitive(
            PrimitiveType.Sphere,
            Imperium.Player.gameplayCamera.transform,
            name: "Imp_NoiseListener",
            layer: 19,
            removeCollider: false,
            removeRenderer: true
        );
        noiseListenerObj.transform.position = Imperium.Player.gameplayCamera.transform.position;
        noiseListenerObj.GetComponent<Collider>().isTrigger = true;

        var noiseListener = noiseListenerObj.AddComponent<ImpNoiseListener>();

        var canvas = Instantiate(ImpAssets.NoiseOverlay);
        var template = canvas.transform.Find("Indicator");
        template.gameObject.SetActive(false);

        for (var i = 0; i < noiseListener.noiseArrows.Length; i++)
        {
            var noiseIndicatorObj = Instantiate(template, canvas.transform);
            var noiseIndicator = noiseIndicatorObj.gameObject.AddComponent<NoiseIndicator>();
            noiseIndicator.Init(canvas.GetComponent<Canvas>());
            noiseListener.noiseArrows[i] = noiseIndicator;
        }

        return noiseListener;
    }

    void INoiseListener.DetectNoise(Vector3 noisePosition, float noiseLoudness, int timesPlayedInOneSpot, int noiseID)
    {
        if (!ImpSettings.Visualizations.NoiseIndicators.Value) return;

        noiseArrows[noiseArrowIndex].Activate(noisePosition, 10, noiseLoudness, noiseID);
        noiseArrowIndex = (noiseArrowIndex + 1) % noiseArrows.Length;
    }
}
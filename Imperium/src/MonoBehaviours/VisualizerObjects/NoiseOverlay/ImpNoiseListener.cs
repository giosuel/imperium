#region

using System.Collections.Generic;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects.NoiseOverlay;

internal class ImpNoiseListener : MonoBehaviour, INoiseListener
{
    private readonly HashSet<NoiseIndicator> indicators = [];
    private readonly Queue<NoiseIndicator> arrowQueue = new();

    internal static ImpNoiseListener Create()
    {
        var noiseListenerObj = ImpUtils.Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            Imperium.Player.gameplayCamera.transform,
            name: "ImpNoiseListener",
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

        for (var i = 0; i < 20; i++)
        {
            var noiseIndicatorObj = Instantiate(template, canvas.transform);
            var noiseIndicator = noiseIndicatorObj.gameObject.AddComponent<NoiseIndicator>();
            noiseIndicator.Init(canvas.GetComponent<Canvas>());
            noiseIndicator.onDone += () => noiseListener.arrowQueue.Enqueue(noiseIndicator);
            noiseListener.indicators.Add(noiseIndicator);
            noiseListener.arrowQueue.Enqueue(noiseIndicator);
        }

        return noiseListener;
    }

    void INoiseListener.DetectNoise(Vector3 noisePosition, float noiseLoudness, int timesPlayedInOneSpot, int noiseID)
    {
        if (!ImpSettings.Visualizations.NoiseIndicators.Value) return;

        arrowQueue.Dequeue().Activate(noisePosition, 10, noiseLoudness, noiseID);
    }
}
#region

using System.Collections.Generic;
using Imperium.Util;
using TMPro;
using UnityEngine;
using Visualization = Imperium.Core.Visualization;

#endregion

namespace Imperium.Visualizers.Objects;

public class SpikeTrapGizmo : MonoBehaviour
{
    private GameObject sphere;
    private SpikeRoofTrap spikeTrap;

    private LineRenderer playerRay;

    private readonly List<GameObject> colliders = [];

    private Transform spikeTimerCanvas;
    private TMP_Text spikeTimerText;

    private void Update()
    {
        var slamOnIntervals = Reflection.Get<SpikeRoofTrap, bool>(spikeTrap, "slamOnIntervals");
        playerRay.gameObject.SetActive(!slamOnIntervals);
        sphere.SetActive(true);

        var slamInterval = Reflection.Get<SpikeRoofTrap, float>(spikeTrap, "slamInterval");
        bool isReady;

        if (slamOnIntervals)
        {
            sphere.GetComponent<MeshRenderer>().material =
                !Physics.CheckSphere(spikeTrap.laserEye.position, 8f, 524288, QueryTriggerInteraction.Collide)
                    ? ImpAssets.WireframeRed
                    : ImpAssets.WireframeGreen;
            isReady = Time.realtimeSinceStartup - spikeTrap.timeSinceMovingUp > slamInterval;
        }
        else
        {
            sphere.SetActive(false);
            spikeTimerCanvas.gameObject.SetActive(false);

            var eyeTransform = spikeTrap.laserEye.transform;
            var eyePosition = eyeTransform.position;
            ImpGeometry.SetLinePositions(
                playerRay,
                eyePosition,
                eyePosition + eyeTransform.forward * 4.4f
            );
            ImpGeometry.SetLineColor(playerRay, Color.red);
            isReady = Time.realtimeSinceStartup - spikeTrap.timeSinceMovingUp >= 0.75f;
        }

        var isSlamming = isReady && spikeTrap.trapActive && spikeTrap.slammingDown;
        if (isSlamming)
        {
            spikeTimerText.text = "SLAM!";
        }
        else if (isReady)
        {
            spikeTimerText.text = "Ready!";
        }
        else
        {
            var timeLeft = Mathf.Max(
                slamInterval
                - Time.realtimeSinceStartup
                + spikeTrap.timeSinceMovingUp,
                0
            );
            spikeTimerText.text = $"{timeLeft:0.0}";
        }

        // Set collider visualizer colors based on if the trap does damage.
        var material = isSlamming ? ImpAssets.WireframeRed : ImpAssets.WireframeGreen;
        foreach (var collider in colliders)
        {
            collider.SetActive(true);
            collider.GetComponent<MeshRenderer>().material = material;
        }

        spikeTimerCanvas.gameObject.SetActive(true);

        spikeTimerCanvas.transform.LookAt(Imperium.Freecam.IsFreecamEnabled.Value
            ? Imperium.Freecam.transform.position
            : Imperium.Player.gameplayCamera.transform.position);
    }

    private void OnDisable()
    {
        sphere.SetActive(false);
        playerRay.gameObject.SetActive(false);
        spikeTimerCanvas.gameObject.SetActive(false);
        colliders.ForEach(collider => collider.SetActive(false));
    }

    public void Init(SpikeRoofTrap trap)
    {
        spikeTrap = trap;
        playerRay = ImpGeometry.CreateLine(spikeTrap.transform, useWorldSpace: true);

        sphere = ImpGeometry.CreatePrimitive(
            PrimitiveType.Sphere, spikeTrap.transform,
            ImpAssets.WireframeRed, 8
        );
        sphere.SetActive(false);

        var spikeTimer = Instantiate(ImpAssets.SpikeTrapTimerObject, transform);
        var trapTransform = spikeTrap.transform;
        spikeTimer.transform.position = trapTransform.position + Vector3.down * 2f;
        spikeTimerCanvas = spikeTimer.transform.Find("Canvas");
        spikeTimerText = spikeTimer.transform.Find("Canvas/Time").GetComponent<TMP_Text>();

        foreach (var collider in trap.GetComponentsInChildren<BoxCollider>())
        {
            var visualizer = Visualization.VisualizeBoxCollider(
                collider, ImpAssets.WireframeGreen, "SpikeTrap"
            );
            visualizer.SetActive(false);
            colliders.Add(visualizer);
        }
    }
}
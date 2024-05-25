#region

using System.Collections.Generic;
using GameNetcodeStuff;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects;

public class LandmineIndicator : MonoBehaviour
{
    private GameObject sphere;
    private Landmine landmine;

    private Dictionary<int, LineRenderer> targetRays = [];

    private void Awake()
    {
        sphere = ImpUtils.Geometry.CreatePrimitive(PrimitiveType.Sphere, transform, ImpAssets.WireframeRedMaterial,
            12f);
    }

    public void SnapshotHitboxes()
    {
        var explosionPosition = landmine.transform.position + Vector3.up;

        // ReSharper disable once Unity.PreferNonAllocApi
        // Allocating cast since this is a replication of the actual algorithm
        var colliders =
            Physics.OverlapSphere(explosionPosition, 6f, 2621448, QueryTriggerInteraction.Collide);

        foreach (var collider in colliders)
        {
            var colliderPosition = collider.transform.position;

            var distance = Vector3.Distance(explosionPosition, colliderPosition);

            // Check if mine would have missed the collider based on line of sight and distance
            var lineOfSightObstructed = distance > 4f &&
                                        Physics.Linecast(explosionPosition,
                                            colliderPosition + Vector3.up * 0.3f,
                                            256, QueryTriggerInteraction.Ignore);

            var snapshotRayColor = lineOfSightObstructed ? Color.green : Color.red;

            // Only draw rays for players, entities and other mines
            var drawRay = false;

            switch (collider.gameObject.layer)
            {
                case 3:
                {
                    // Set color to green if out of range
                    if (!lineOfSightObstructed && distance >= 6f)
                    {
                        snapshotRayColor = Color.green;
                    }
                    // Set color to orange when player would only get damaged and not killed
                    else if (!lineOfSightObstructed && distance >= 5.7f)
                    {
                        snapshotRayColor = new Color(1f, 0.63f, 0.2f);
                    }

                    var snapshotHitbox =
                        ImpUtils.Geometry.CreatePrimitive(PrimitiveType.Cube, transform,
                            ImpAssets.WireframeYellowMaterial);

                    var player = collider.GetComponent<PlayerControllerB>();
                    if (!player) continue;

                    var playerCollider = player.GetComponent<BoxCollider>();
                    var playerTransform = playerCollider.transform;

                    snapshotHitbox.transform.position = playerTransform.position + playerCollider.center;
                    snapshotHitbox.transform.localScale = playerCollider.size;
                    snapshotHitbox.transform.rotation = playerTransform.rotation;

                    drawRay = true;
                    break;
                }
                case 19:
                {
                    if (distance >= 4.5f)
                    {
                        snapshotRayColor = Color.green;
                    }

                    var entityColliderScript = collider.GetComponentInChildren<EnemyAICollisionDetect>();
                    if (entityColliderScript == null) break;

                    var entityTransform = entityColliderScript.transform;

                    if (entityColliderScript.TryGetComponent<BoxCollider>(out var boxCollider))
                    {
                        var snapshotHitbox =
                            ImpUtils.Geometry.CreatePrimitive(PrimitiveType.Cube, entityColliderScript.transform,
                                ImpAssets.WireframeYellowMaterial);

                        snapshotHitbox.transform.position = entityTransform.position;
                        snapshotHitbox.transform.localPosition = boxCollider.center;
                        snapshotHitbox.transform.localScale = boxCollider.size;
                        snapshotHitbox.transform.rotation = entityTransform.rotation;

                        snapshotHitbox.transform.SetParent(transform, true);
                    }

                    if (entityColliderScript.TryGetComponent<CapsuleCollider>(out var capsuleCollider))
                    {
                        var snapshotHitbox =
                            ImpUtils.Geometry.CreatePrimitive(PrimitiveType.Capsule, entityColliderScript.transform,
                                ImpAssets.WireframeYellowMaterial);

                        snapshotHitbox.transform.position = entityTransform.position;
                        snapshotHitbox.transform.localPosition = capsuleCollider.center;
                        snapshotHitbox.transform.localScale = new Vector3(
                            capsuleCollider.radius * 2,
                            capsuleCollider.height / 2,
                            capsuleCollider.radius * 2
                        );
                        snapshotHitbox.transform.rotation = entityTransform.rotation;

                        snapshotHitbox.transform.SetParent(transform, true);
                    }

                    drawRay = true;
                    break;
                }
                case 21:
                    if (distance >= 6f) break;

                    var otherLandmine = collider.gameObject.GetComponentInChildren<Landmine>();
                    if (!otherLandmine || otherLandmine.hasExploded) break;

                    snapshotRayColor = new Color(0.34f, 0f, 0.56f);
                    drawRay = true;
                    break;
            }

            if (drawRay)
            {
                var snapshotRay = ImpUtils.Geometry.CreateLine(transform, useWorldSpace: true);
                ImpUtils.Geometry.SetLineColor(snapshotRay, snapshotRayColor);
                ImpUtils.Geometry.SetLinePositions(snapshotRay, explosionPosition, colliderPosition + Vector3.up * 0.3f);
            }
        }
    }

    private void Update()
    {
        if (landmine.hasExploded)
        {
            if (targetRays.Count > 0)
            {
                foreach (var lineRenderer in targetRays.Values)
                {
                    Destroy(lineRenderer.gameObject);
                }

                targetRays.Clear();
            }

            return;
        }

        var explosionPosition = landmine.transform.position + Vector3.up;

        // ReSharper disable Unity.PreferNonAllocApi
        // Allocating cast since this is a replication of the actual algorithm
        var colliders =
            Physics.OverlapSphere(explosionPosition, 6f, 2621448, QueryTriggerInteraction.Collide);

        var collisionIds = new HashSet<int>();
        foreach (var collider in colliders)
        {
            var instanceId = collider.gameObject.GetInstanceID();
            if (!targetRays.TryGetValue(instanceId, out var lineRenderer))
            {
                // ReSharper disable Unity.PerformanceCriticalCodeInvocation
                // This is only executed when a new collider is detected
                lineRenderer = ImpUtils.Geometry.CreateLine(transform, useWorldSpace: true);
                targetRays[instanceId] = lineRenderer;
            }

            var colliderPosition = collider.transform.position;

            var distance = Vector3.Distance(explosionPosition, colliderPosition);

            // Check if mine would have missed the collider based on line of sight and distance
            var lineOfSightObstructed = distance > 4f &&
                                        Physics.Linecast(explosionPosition,
                                            colliderPosition + Vector3.up * 0.3f,
                                            256, QueryTriggerInteraction.Ignore);

            var snapshotRayColor = lineOfSightObstructed ? Color.green : Color.red;

            // Only draw rays for players, entities and other mines
            var drawRay = false;

            switch (collider.gameObject.layer)
            {
                case 3:
                {
                    // Set color to green if out of range
                    if (!lineOfSightObstructed && distance >= 6f)
                    {
                        snapshotRayColor = Color.green;
                    }
                    // Set color to orange when player would only get damaged and not killed
                    else if (!lineOfSightObstructed && distance >= 5.7f)
                    {
                        snapshotRayColor = new Color(1f, 0.63f, 0.2f);
                    }

                    drawRay = true;
                    break;
                }
                case 19:
                {
                    if (!collider.GetComponent<EnemyAICollisionDetect>()) break;
                    if (distance >= 4.5f)
                    {
                        snapshotRayColor = Color.green;
                    }

                    drawRay = true;
                    break;
                }
                case 21:
                    var otherLandmine = collider.gameObject.GetComponentInChildren<Landmine>();
                    if (!otherLandmine || otherLandmine.hasExploded || otherLandmine == landmine) break;

                    snapshotRayColor = distance >= 6f ? Color.green : new Color(0.79f, 0.09f, 1f);
                    drawRay = true;
                    break;
            }

            if (drawRay)
            {
                collisionIds.Add(instanceId);
                ImpUtils.Geometry.SetLineColor(lineRenderer, snapshotRayColor);
                ImpUtils.Geometry.SetLinePositions(lineRenderer, explosionPosition, colliderPosition + Vector3.up * 0.3f);
            }
        }

        // Destroy old rays that are not drawn anymore
        var newTargetRays = new Dictionary<int, LineRenderer>();
        foreach (var (id, lineRenderer) in targetRays)
        {
            if (collisionIds.Contains(id))
            {
                newTargetRays[id] = lineRenderer;
            }
            else
            {
                Destroy(lineRenderer.gameObject);
            }
        }

        targetRays = newTargetRays;
    }

    public void Init(Landmine mine)
    {
        landmine = mine;

        var position = landmine.transform.position;
        sphere.transform.position = position + Vector3.up;
    }
}
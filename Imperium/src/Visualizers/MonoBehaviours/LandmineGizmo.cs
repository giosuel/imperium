#region

using System.Collections.Generic;
using GameNetcodeStuff;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Visualizers.MonoBehaviours;

public class LandmineGizmo : MonoBehaviour
{
    private GameObject sphere;
    private Landmine landmine;

    private Dictionary<int, LineRenderer> targetRays = [];

    private void Awake()
    {
        sphere = ImpGeometry.CreatePrimitive(
            PrimitiveType.Sphere, transform, API.Materials.WireframeRed, 12f
        );
    }

    public void SnapshotHitboxes()
    {
        var explosionPosition = landmine.transform.position + Vector3.up;

        // ReSharper disable once Unity.PreferNonAllocApi
        // Allocating cast since this is a replication of the actual algorithm
        var colliders = Physics.OverlapSphere(
            explosionPosition, 6f, 2621448, QueryTriggerInteraction.Collide
        );

        foreach (var collider in colliders)
        {
            var colliderPosition = collider.transform.position;

            var distance = Vector3.Distance(explosionPosition, colliderPosition);

            // Check if mine had missed the collider based on LOS and distance
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
                    snapshotRayColor = distance switch
                    {
                        // Set color to green if out of range
                        >= 6f when lineOfSightObstructed is false => Color.green,
                        // Set color to orange when player would only get damaged and not killed
                        >= 5.7f when lineOfSightObstructed is false => new Color(1f, 0.63f, 0.2f),
                        _ => snapshotRayColor
                    };

                    var snapshotHitbox =
                        ImpGeometry.CreatePrimitive(
                            PrimitiveType.Cube, transform, API.Materials.WireframeYellow
                        );

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
                    if (distance >= 4.5f) snapshotRayColor = Color.green;

                    var entityColliderScript = collider.GetComponentInChildren<EnemyAICollisionDetect>();
                    if (entityColliderScript == null) break;

                    var entityTransform = entityColliderScript.transform;

                    if (entityColliderScript.TryGetComponent<BoxCollider>(out var boxCollider))
                    {
                        var snapshotHitbox =
                            ImpGeometry.CreatePrimitive(
                                PrimitiveType.Cube, entityColliderScript.transform, API.Materials.WireframeYellow
                            );

                        snapshotHitbox.transform.position = entityTransform.position;
                        snapshotHitbox.transform.localPosition = boxCollider.center;
                        snapshotHitbox.transform.localScale = boxCollider.size;
                        snapshotHitbox.transform.rotation = entityTransform.rotation;

                        snapshotHitbox.transform.SetParent(transform, true);
                    }

                    if (entityColliderScript.TryGetComponent<CapsuleCollider>(out var capsuleCollider))
                    {
                        var snapshotHitbox =
                            ImpGeometry.CreatePrimitive(
                                PrimitiveType.Capsule, entityColliderScript.transform, API.Materials.WireframeYellow
                            );

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
                var snapshotRay = ImpGeometry.CreateLine(transform, useWorldSpace: true);
                ImpGeometry.SetLineColor(snapshotRay, snapshotRayColor);
                ImpGeometry.SetLinePositions(snapshotRay, explosionPosition, colliderPosition + Vector3.up * 0.3f);
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

        // ReSharper disable Unity.PreferNonAllocApi
        // Allocating cast since this is a replication of the actual algorithm
        var colliders =
            Physics.OverlapSphere(landmine.transform.position, 6f, 2621448, QueryTriggerInteraction.Collide);

        var collisionIds = new HashSet<int>();
        foreach (var collider in colliders)
        {
            var instanceId = collider.gameObject.GetInstanceID();
            if (!targetRays.TryGetValue(instanceId, out var lineRenderer))
            {
                // ReSharper disable Unity.PerformanceCriticalCodeInvocation
                // This is only executed when a new collider is detected
                lineRenderer = ImpGeometry.CreateLine(transform, useWorldSpace: true);
                targetRays[instanceId] = lineRenderer;
            }

            var colliderPosition = collider.transform.position;
            var landminePosition = landmine.transform.position;

            var distance = Vector3.Distance(landminePosition, colliderPosition);

            // Check if mine would have missed the collider based on line of sight and distance
            var lineOfSightObstructed = distance > 4f &&
                                        Physics.Linecast(landminePosition,
                                            colliderPosition + Vector3.up * 0.3f,
                                            256, QueryTriggerInteraction.Ignore);

            var snapshotRayColor = lineOfSightObstructed ? Color.green : Color.red;

            // Only draw rays for players, entities and other mines
            var drawRay = false;

            switch (collider.gameObject.layer)
            {
                case 3:
                {
                    // Set color to orange when player would only get damaged and not killed
                    if (!lineOfSightObstructed && distance >= 5.7f)
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
                ImpGeometry.SetLineColor(lineRenderer, snapshotRayColor);
                ImpGeometry.SetLinePositions(lineRenderer, landmine.transform.position, colliderPosition);
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
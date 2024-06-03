#region

using System.Collections.Generic;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Visualizers.MonoBehaviours;

public class ShotgunGizmo : MonoBehaviour
{
    // Lock-on rays for players
    private readonly Dictionary<int, LineRenderer> playerRays = [];

    // Sphere cast visualization for entities
    private readonly GameObject[] spheres = new GameObject[ImpConstants.ShotgunCollisionCount];

    // Collider visualizations for entities
    private readonly GameObject[] collisionSpheres = new GameObject[ImpConstants.ShotgunCollisionCount];

    // Line of sight visualization for entities
    private readonly LineRenderer[] entityRays = new LineRenderer[ImpConstants.ShotgunCollisionCount];

    // Sphere cast to entity visualization for entities
    private readonly LineRenderer[] sphereRays = new LineRenderer[ImpConstants.ShotgunCollisionCount];

    // Forward ray of the gun
    private LineRenderer forwardRay;

    private ShotgunItem shotgun;

    private bool isActivelyHolding;

    private void Awake()
    {
        for (var i = 0; i < ImpConstants.ShotgunCollisionCount; i++)
        {
            spheres[i] = ImpUtils.Geometry.CreatePrimitive(
                PrimitiveType.Sphere, transform, ImpAssets.WireframePurpleMaterial, 10
            );

            collisionSpheres[i] = ImpUtils.Geometry.CreatePrimitive(
                PrimitiveType.Sphere, transform, Color.red, 0.2f
            );

            entityRays[i] = ImpUtils.Geometry.CreateLine(transform, useWorldSpace: true);
            sphereRays[i] = ImpUtils.Geometry.CreateLine(transform, useWorldSpace: true);
        }

        forwardRay = ImpUtils.Geometry.CreateLine(transform, useWorldSpace: true);
        ImpUtils.Geometry.SetLineColor(forwardRay, new Color(0.41f, 0.41f, 0.41f));
    }

    public void Init(ShotgunItem shotgunItem, bool isHolding)
    {
        isActivelyHolding = isHolding;
        shotgun = shotgunItem;

        if (!isActivelyHolding)
        {
            foreach (var sphere in spheres) sphere.SetActive(false);
            foreach (var sphere in collisionSpheres) sphere.SetActive(false);
            foreach (var (_, lineRenderer) in playerRays) lineRenderer.gameObject.SetActive(false);
            foreach (var lineRenderer in entityRays) lineRenderer.gameObject.SetActive(false);
            foreach (var lineRenderer in sphereRays) lineRenderer.gameObject.SetActive(false);
            forwardRay.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isActivelyHolding) return;

        var (shotgunPosition, shotgunForward) = GetShotgunPosition(shotgun);

        forwardRay.gameObject.SetActive(true);

        foreach (var player in Imperium.ObjectManager.CurrentPlayers.Value)
        {
            if (!playerRays.TryGetValue(player.GetInstanceID(), out var lineRenderer))
            {
                // ReSharper disable Unity.PerformanceCriticalCodeInvocation
                // This is only executed when a new collider is detected
                lineRenderer = ImpUtils.Geometry.CreateLine(transform, useWorldSpace: true);
                playerRays[player.GetInstanceID()] = lineRenderer;
            }

            var collider = player.playerCollider;

            lineRenderer.gameObject.SetActive(false);

            // Don't draw ray to dead players
            if (player.isPlayerDead || !player.isPlayerControlled) continue;

            var closestPoint = collider.ClosestPoint(shotgunPosition);
            if (Vector3.Angle(shotgunForward, closestPoint - shotgunPosition) >= 30f) continue;

            lineRenderer.gameObject.SetActive(true);

            var playerRayColor = Color.green;

            if (Physics.Linecast(shotgunPosition, closestPoint,
                    StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                playerRayColor = Color.red;
            }

            ImpUtils.Geometry.SetLineColor(lineRenderer, playerRayColor);
            ImpUtils.Geometry.SetLinePositions(lineRenderer, shotgunPosition, closestPoint);
        }

        var forwardRayColor = Color.red;

        var colliders = new RaycastHit[10];
        var ray = new Ray(shotgunPosition - shotgunForward * 10f, shotgunForward);
        var hits = Physics.SphereCastNonAlloc(ray, 5f, colliders, 15f, 524288, QueryTriggerInteraction.Collide);

        for (var i = 0; i < 10; i++)
        {
            spheres[i].SetActive(false);
            entityRays[i].gameObject.SetActive(false);
            sphereRays[i].gameObject.SetActive(false);
            collisionSpheres[i].gameObject.SetActive(false);

            var entityRayColor = Color.green;

            // Don't activate any other objects if number of hits has been reached
            if (i >= hits) continue;

            // Set forward color to green if at least one entity was would be hit (in sphere and line cast)
            forwardRayColor = Color.green;

            if (!colliders[i].transform.TryGetComponent<EnemyAICollisionDetect>(out var collisionDetect))
            {
                entityRayColor = new Color(1f, 0.56f, 0.44f);
            }
            else if (collisionDetect.mainScript.isEnemyDead)
            {
                entityRayColor = Color.gray;
            }
            else if (Physics.Linecast(shotgunPosition, colliders[i].point, out _,
                         Imperium.StartOfRound.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                entityRayColor = Color.red;
            }

            entityRays[i].gameObject.SetActive(true);
            ImpUtils.Geometry.SetLineColor(entityRays[i], entityRayColor);
            ImpUtils.Geometry.SetLinePositions(entityRays[i], shotgunPosition, colliders[i].point);

            spheres[i].SetActive(true);
            collisionSpheres[i].SetActive(true);
            sphereRays[i].gameObject.SetActive(true);

            var closestPointToRay = ImpUtils.VectorMath.ClosestPointAlongRay(ray, colliders[i].transform.position);

            spheres[i].transform.position = closestPointToRay;
            collisionSpheres[i].transform.position = colliders[i].point;

            ImpUtils.Geometry.SetLinePositions(sphereRays[i], closestPointToRay, colliders[i].point);
        }

        ImpUtils.Geometry.SetLineColor(forwardRay, forwardRayColor);
        ImpUtils.Geometry.SetLinePositions(forwardRay, shotgunPosition, shotgunPosition + shotgunForward * 15f);
    }

    private static (Vector3, Vector3) GetShotgunPosition(ShotgunItem shotgun)
    {
        if (!shotgun.playerHeldBy) return (shotgun.shotgunRayPoint.position, shotgun.shotgunRayPoint.forward);

        var gameplayCamera = shotgun.playerHeldBy.gameplayCamera.transform;
        var position = gameplayCamera.position;
        var forward = gameplayCamera.forward;
        return (position - gameplayCamera.up * 0.45f, forward);
    }
}
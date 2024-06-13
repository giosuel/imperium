#region

using System.Collections.Generic;
using Imperium.API;
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

    private GameObject capsule;

    // Forward ray of the gun
    private LineRenderer forwardRay;

    private ShotgunItem shotgun;

    private bool isActivelyHolding;

    private const float CastLength = 25f;
    private const float CastRadius = 5f;

    private void Awake()
    {
        for (var i = 0; i < ImpConstants.ShotgunCollisionCount; i++)
        {
            spheres[i] = ImpGeometry.CreatePrimitive(
                PrimitiveType.Sphere, transform, Materials.WireframePurple, 10
            );

            collisionSpheres[i] = ImpGeometry.CreatePrimitive(
                PrimitiveType.Sphere, transform, Color.red, 0.2f
            );

            entityRays[i] = ImpGeometry.CreateLine(transform, useWorldSpace: true);
            sphereRays[i] = ImpGeometry.CreateLine(transform, useWorldSpace: true);
        }

        // capsule = ImpGeometry.CreatePrimitive(
        //     PrimitiveType.Capsule, null, API.Materials.WireframePurple
        // );
        //
        // capsule.transform.position = Vector3.zero;
        // var capsuleFilter = capsule.GetComponent<MeshFilter>();
        // capsuleFilter.mesh = ScaleMiddlePart(capsuleFilter.mesh, 25f, 5f);

        // capsule.transform.SetParent(transform);

        forwardRay = ImpGeometry.CreateLine(transform, useWorldSpace: true);
        ImpGeometry.SetLineColor(forwardRay, new Color(0.41f, 0.41f, 0.41f));
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

    private Mesh ScaleMiddlePart(Mesh mesh, float finalHeight, float capsuleRadius)
    {
        var vertices = mesh.vertices;

        // Calculate the height of the hemispheres
        var hemisphereHeight = capsuleRadius;
        var originalHeight = CalculateOriginalHeight(vertices);
        var originalMiddleHeight = originalHeight - 2 * hemisphereHeight;

        // Calculate the new middle height and scale factor
        var newMiddleHeight = finalHeight - 2 * hemisphereHeight;
        var scaleFactor = newMiddleHeight / originalMiddleHeight;

        // Adjust vertices: stretch middle section and move top hemisphere
        for (var i = 0; i < vertices.Length; i++)
        {
            if (vertices[i].y > -hemisphereHeight && vertices[i].y < hemisphereHeight)
            {
                // Scale middle section
                vertices[i].y *= scaleFactor;
            }
            else if (vertices[i].y >= hemisphereHeight)
            {
                // Move top hemisphere up
                vertices[i].y += (newMiddleHeight - originalMiddleHeight);
            }
        }

        // Update the mesh with the new vertices
        mesh.vertices = vertices;

        // Recalculate bounds and normals to ensure proper rendering
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    float CalculateOriginalHeight(Vector3[] vertices)
    {
        // Find the highest and lowest points in the mesh along the y-axis
        var minY = float.MaxValue;
        var maxY = float.MinValue;

        foreach (var vertex in vertices)
        {
            if (vertex.y < minY)
            {
                minY = vertex.y;
            }

            if (vertex.y > maxY)
            {
                maxY = vertex.y;
            }
        }

        return maxY - minY;
    }

    private void Update()
    {
        if (!isActivelyHolding) return;

        var (shotgunPosition, shotgunForward) = GetShotgunPosition(shotgun);

        forwardRay.gameObject.SetActive(true);

        /*
         * Players
         */
        foreach (var player in Imperium.ObjectManager.CurrentPlayers.Value)
        {
            if (!playerRays.TryGetValue(player.GetInstanceID(), out var lineRenderer))
            {
                // ReSharper disable Unity.PerformanceCriticalCodeInvocation
                // This is only executed when a new collider is detected
                lineRenderer = ImpGeometry.CreateLine(transform, useWorldSpace: true);
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

            ImpGeometry.SetLineColor(lineRenderer, playerRayColor);
            ImpGeometry.SetLinePositions(lineRenderer, shotgunPosition, closestPoint);
        }

        /*
         * Capsule
         */
        // var positionStart = shotgunPosition - shotgunForward * 10f;
        // var positionEnd = shotgunPosition + shotgunForward * 15f;

        // capsule.transform.position = positionStart + shotgunForward * (10f + 15f) / 2;
        // capsule.transform.rotation = Quaternion.LookRotation(shotgun.transform.up, shotgunForward);

        // var distance = Vector3.Distance(positionStart, positionEnd) + CastRadius * 2;
        // capsule.transform.localScale = new Vector3(CastRadius * 2, distance / 2, CastRadius * 2);
        // capsule.transform.localScale = new Vector3(1, 1, 1);

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
            ImpGeometry.SetLineColor(entityRays[i], entityRayColor);
            ImpGeometry.SetLinePositions(entityRays[i], shotgunPosition, colliders[i].point);

            spheres[i].SetActive(true);
            collisionSpheres[i].SetActive(true);
            sphereRays[i].gameObject.SetActive(true);

            var closestPointToRay = ImpMath.ClosestPointAlongRay(ray, colliders[i].transform.position);

            spheres[i].transform.position = closestPointToRay;
            collisionSpheres[i].transform.position = colliders[i].point;

            ImpGeometry.SetLinePositions(sphereRays[i], closestPointToRay, colliders[i].point);
        }

        ImpGeometry.SetLineColor(forwardRay, forwardRayColor);
        ImpGeometry.SetLinePositions(forwardRay, shotgunPosition, shotgunPosition + shotgunForward * 15f);
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
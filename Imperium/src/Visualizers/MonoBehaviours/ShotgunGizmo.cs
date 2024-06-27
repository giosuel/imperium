#region

using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Visualizers.MonoBehaviours;

public class ShotgunGizmo : MonoBehaviour
{
    // Lock-on rays for players
    private readonly Dictionary<int, LineRenderer> playerRays = [];

    // Line of sight visualization for entities
    private readonly LineRenderer[] entityRays = new LineRenderer[ImpConstants.ShotgunCollisionCount];

    // Ray origin point, moved to the location where shots originate
    private GameObject rayHolder;
	
    // Spherecast origin renderer - this area does not shoot entities within
    private LineRenderer spherecastOriginArea;

    private ShotgunItem shotgun;

    private bool isActivelyHolding;

    private const float SpherecastRadius = 5f;
    private const float SpherecastStartOffset = -10f;
    private const float SpherecastRange = 15f;
    private const float EntityMaxDmgRange = 3.7f;
    private const float EntityMidDmgRange = 6f;

    private const float PlayerDmgAngle = 30f;
    private const float PlayerKillRange = 15f;
    private const float PlayerMidDmgRange = 23f;
    private const float PlayerLowDmgRange = 30f;

    private const float CirclePointCount = 64;

    private static Color MinEntityDamageColor = new Color(0.6f, 1.0f, 0.6f);
    private static Color MidEntityDamageColor = new Color(0.3f, 1.0f, 0.3f);
    private static Color MaxEntityDamageColor = new Color(0.0f, 0.5f, 0.0f);
    private static Color MinPlayerDamageColor = Color.yellow;
    private static Color MidPlayerDamageColor = new Color(1f, 0.5f, 0f);
    private static Color MaxPlayerDamageColor = Color.red;
    private static Color SpherecastOriginColor = new Color(0.5f, 0f, 0.5f);

    private static Color InvalidTargetColor = new Color(0.3f, 0.3f, 0.3f);
    private static Color TargetObstructedColor = Color.white;

    // Multipler applied to visualization of the shotgun hitbox
    private static Color HitboxColorMultiplier = new Color(1f, 1f, 1f, 0.35f);

    private void Awake()
    {
        for (var i = 0; i < ImpConstants.ShotgunCollisionCount; i++)
        {
            entityRays[i] = ImpGeometry.CreateLine(transform, useWorldSpace: true);
        }
    }

    private void OnEnable()
    {
        Init(shotgun, isActivelyHolding);
    }

    private void OnDisable()
    {
        if (!rayHolder) return;
        rayHolder.transform.SetParent(transform);
        rayHolder.SetActive(false);
    }

    private void OnDestroy()
    {
        if(rayHolder) Destroy(rayHolder);
    }

    // Helper function to create a list of points along an arc defined by a Z offset, radius, and degrees, with the points clamped to a max X position.
    private static List<Vector3> GenerateArcPoints(float zOffset, float radius, float degreesArcStart, float degreesArcEnd, float maxWidth = Mathf.Infinity)
    {
        const float pi2 = Mathf.PI * 2f;

        float radArcStart = Mathf.Deg2Rad * degreesArcStart;
        float radArcEnd = Mathf.Deg2Rad * degreesArcEnd;

        float radDif = ((radArcEnd - radArcStart) % pi2 + pi2) % pi2;
        radArcEnd = radArcStart + radDif;

        const float increment = pi2 / CirclePointCount;

        List<Vector3> points = new();

        float radCurrent = radArcStart;
        points.Add(new(Mathf.Sin(radCurrent) * radius, 0, Mathf.Cos(radCurrent) * radius + zOffset));
        do
        {
            radCurrent = Mathf.Min(radCurrent + increment, radArcEnd);
            float xPos = Mathf.Max(Mathf.Min(Mathf.Sin(radCurrent) * radius, maxWidth), -maxWidth);
            float zPos = Mathf.Cos(radCurrent) * radius + zOffset;
            points.Add(new(xPos, 0, zPos));
        }
        while (radCurrent < radArcEnd);

        return points;
    }

    // Helper function to generate a list of points on a circle defined by a Z offset and radius, with the points clamped to a max X position.
    private static List<Vector3> Circle(float zOffset, float radius, float maxWidth = Mathf.Infinity)
    {
        const float pi2 = Mathf.PI * 2f;

        const float increment = pi2 / CirclePointCount;

        List<Vector3> points = new();

        float radCurrent = 0;
        points.Add(new(Mathf.Sin(radCurrent) * radius, 0, Mathf.Cos(radCurrent) * radius + zOffset));
        do
        {
            radCurrent = Mathf.Min(radCurrent + increment, pi2);
            float xPos = Mathf.Max(Mathf.Min(Mathf.Sin(radCurrent) * radius, maxWidth), -maxWidth);
            float zPos = Mathf.Cos(radCurrent) * radius + zOffset;
            points.Add(new(xPos, 0, zPos));
        }
        while (radCurrent < pi2);

        return points;
    }

    // Add a line renderer to the rayHolder defined by a list of points and color.
    private LineRenderer AddLineToRayHolder(Color lineColor, List<Vector3> points)
    {
        LineRenderer renderer = ImpGeometry.CreateLine(rayHolder.transform, startColor: lineColor, endColor: lineColor, positions:
            points.ToArray());

        return renderer;
    }

    // Add all the LineRenderers to the rayHolder GameObject, to be moved to wherever the shots originate
    private void SetupRendererObject()
    {
        // Do nothing if we've already got a rayHolder
        if (rayHolder != null) return;

        rayHolder = new GameObject("ImpShotgunRayHolder");
        rayHolder.transform.parent = transform;

        // The circle where the shotgun's SphereCast originates. This area does not damage entities but does count toward the 10 target limit
        spherecastOriginArea = AddLineToRayHolder(SpherecastOriginColor * HitboxColorMultiplier, Circle(SpherecastStartOffset, SpherecastRadius));

        // Generate the cross section of the minimum entity damage area. This area has a capsule shape with the SphereCast origin sphere chopped out
        List<Vector3> backSemicircle = GenerateArcPoints(SpherecastStartOffset, SpherecastRadius, 270, 90);
        List<Vector3> frontSemicircle = GenerateArcPoints(SpherecastStartOffset + SpherecastRange, SpherecastRadius, 270, 90);
        frontSemicircle.Reverse();
        frontSemicircle.Add(backSemicircle[0]);
        List<Vector3> spherecastRangeShape = backSemicircle.Concat(frontSemicircle).ToList();
        AddLineToRayHolder(MinEntityDamageColor * HitboxColorMultiplier, spherecastRangeShape);

        // Generate the cross section of the medium entity damage area. This area is a sphere contained within the minimum damage area
        // This math is to calculate the angle to the intersection point of the SphereCast origin circle and the medium damage circle
        float radius0 = EntityMidDmgRange;
        float radius1 = SpherecastRadius;
        float dist = Mathf.Abs(SpherecastStartOffset);
        float b = (Mathf.Pow(radius1, 2) - Mathf.Pow(radius0, 2) + Mathf.Pow(dist, 2)) / (2 * dist);
        float a = dist - b;
        float angle = Mathf.Acos(a / radius0);
        float oppAngle = Mathf.Acos(b / radius1);

        List<Vector3> midDmgCircleArc = GenerateArcPoints(0, radius0, -180 + angle * Mathf.Rad2Deg, 180 - angle * Mathf.Rad2Deg, SpherecastRadius);
        List<Vector3> midDmgOffsetArc = GenerateArcPoints(SpherecastStartOffset, radius1, -oppAngle * Mathf.Rad2Deg, oppAngle * Mathf.Rad2Deg, SpherecastRadius);
        midDmgOffsetArc.Reverse();
        midDmgOffsetArc.Add(midDmgCircleArc[0]);
        List<Vector3> midDmgArea = midDmgCircleArc.Concat(midDmgOffsetArc).ToList();
        AddLineToRayHolder(MidEntityDamageColor * HitboxColorMultiplier, midDmgArea);

        // Generate the circle cross section of the max entity damage sphere
        AddLineToRayHolder(MaxEntityDamageColor * HitboxColorMultiplier, Circle(0, EntityMaxDmgRange, SpherecastRadius));

        // Generate the triangle cross section of the player kill range. This is a cone
        List<Vector3> killCone = GenerateArcPoints(0, PlayerKillRange, -30, 30);
        killCone.Insert(0, Vector3.zero);
        killCone.Add(Vector3.zero);
        AddLineToRayHolder(MaxPlayerDamageColor * HitboxColorMultiplier, killCone);

        // Generate the cross section of the 40dmg range
        List<Vector3> midDmgCone = GenerateArcPoints(0, PlayerMidDmgRange, -30, 30);
        midDmgCone.Insert(0, killCone[1]);
        midDmgCone.Add(killCone[killCone.Count-2]);
        AddLineToRayHolder(MidPlayerDamageColor * HitboxColorMultiplier, midDmgCone);

        // Generate the cross section of the 20dmg range
        List<Vector3> lowDmgCone = GenerateArcPoints(0, PlayerLowDmgRange, -30, 30);
        lowDmgCone.Insert(0, midDmgCone[1]);
        lowDmgCone.Add(midDmgCone[killCone.Count - 2]);
        AddLineToRayHolder(MinPlayerDamageColor * HitboxColorMultiplier, lowDmgCone);
    }

    public void Init(ShotgunItem shotgunItem, bool isHolding)
    {
        isActivelyHolding = isHolding;
        shotgun = shotgunItem;
        SetupRendererObject();

        bool heldByPlayer = shotgunItem.playerHeldBy && isHolding;
        bool heldByNutcracker = shotgunItem.heldByEnemy && shotgunItem.heldByEnemy.TryGetComponent<NutcrackerEnemyAI>(out _);
        if (gameObject.activeSelf && (heldByPlayer || heldByNutcracker))
        {
            rayHolder.transform.parent = null;
            rayHolder.SetActive(true);
        }
        else
        {
            rayHolder.transform.SetParent(transform);
            rayHolder.SetActive(false);

            foreach (var (_, lineRenderer) in playerRays) lineRenderer.gameObject.SetActive(false);
            foreach (var lineRenderer in entityRays) lineRenderer.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (rayHolder == null || !rayHolder.activeSelf) return;

        var (shotgunPosition, shotgunForward) = GetShotgunPosition(shotgun);

        rayHolder.transform.position = shotgunPosition;
        rayHolder.transform.forward = shotgunForward;

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
            // Don't draw ray to players outside the cone
            if (Vector3.Angle(shotgunForward, closestPoint - shotgunPosition) >= PlayerDmgAngle) continue;

            var distance = Vector3.Distance(player.transform.position, shotgun.shotgunRayPoint.position);
            // Don't draw ray to players beyond damage range
            if (distance >= PlayerLowDmgRange) continue;

            lineRenderer.gameObject.SetActive(true);

            var playerRayColor = distance switch
            {
                < PlayerKillRange => MaxPlayerDamageColor,
                < PlayerMidDmgRange => MidPlayerDamageColor,
                _ => MinPlayerDamageColor
            };

            if (Physics.Linecast(shotgunPosition, closestPoint,
                    StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                playerRayColor = TargetObstructedColor;
            }

            ImpGeometry.SetLineColor(lineRenderer, playerRayColor, playerRayColor);
            ImpGeometry.SetLinePositions(lineRenderer, shotgunPosition, closestPoint);
        }


        /*
         * Entities
         */
        var colliders = new RaycastHit[10];
        var ray = new Ray(shotgunPosition + shotgunForward * SpherecastStartOffset, shotgunForward);
        var hits = Physics.SphereCastNonAlloc(ray, SpherecastRadius, colliders, SpherecastRange, 524288, QueryTriggerInteraction.Collide);

        spherecastOriginArea.gameObject.SetActive(false);

        for (var i = 0; i < 10; i++)
        {
            entityRays[i].gameObject.SetActive(false);

            // Don't activate any other objects if number of hits has been reached
            if (i >= hits) continue;

            // Don't draw these rays to players, they obstruct vision and provide little information
            if (colliders[i].transform.TryGetComponent<PlayerControllerB>(out var playerController)) continue;

            Vector3 startPoint = shotgunPosition;
            Vector3 endPoint = colliders[i].point;
            Color entityRayColor;

            if (colliders[i].distance == 0f)
            {
                // The target is inside the SpherecastOrigin area and will take no damage
                startPoint = shotgunPosition + shotgunForward * SpherecastStartOffset;
                endPoint = colliders[i].collider.ClosestPoint(startPoint);
                entityRayColor = SpherecastOriginColor;
                spherecastOriginArea.gameObject.SetActive(true);
            }
            else if (!colliders[i].transform.TryGetComponent<EnemyAICollisionDetect>(out var collisionDetect) || collisionDetect.onlyCollideWhenGrounded)
            {
                // The target is either not an enemy, or is an enemy hitbox that does not receive damage
                startPoint = colliders[i].point;
                endPoint = startPoint + Vector3.up * 5f;
                entityRayColor = InvalidTargetColor;
            }
            else if (Physics.Linecast(shotgunPosition, colliders[i].point, out _,
             Imperium.StartOfRound.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
            {
                // Line of sight to target is obstructed and will take no damage
                entityRayColor = TargetObstructedColor;
            }
            else
            {
                float distance = Vector3.Distance(shotgunPosition, colliders[i].point);
                entityRayColor = distance switch
                {
                    < 3.7f => MaxEntityDamageColor,
                    < 6f => MidEntityDamageColor,
                    _ => MinEntityDamageColor,
                };
            }

            entityRays[i].gameObject.SetActive(true);
            ImpGeometry.SetLineColor(entityRays[i], entityRayColor, entityRayColor);
            ImpGeometry.SetLinePositions(entityRays[i], startPoint, endPoint);
        }
    }

    private static (Vector3, Vector3) GetShotgunPosition(ShotgunItem shotgun)
    {
        NutcrackerEnemyAI nutcrackerAI = null;
        if (!shotgun.playerHeldBy && (!shotgun.heldByEnemy || !shotgun.heldByEnemy.TryGetComponent(out nutcrackerAI)))
        {
            return (shotgun.shotgunRayPoint.position, shotgun.shotgunRayPoint.forward);
        }
        else if (shotgun.heldByEnemy && nutcrackerAI)
        {
            if (nutcrackerAI.isInspecting || nutcrackerAI.aimingGun) return (shotgun.shotgunRayPoint.position, shotgun.shotgunRayPoint.forward);
            // If the nutcracker is not aiming the gun, use the nutcracker torso forward angle instead so the visualizer doesn't wobble with the animation
            return (shotgun.shotgunRayPoint.position, Vector3.Cross(nutcrackerAI.torsoContainer.right, Vector3.up));
        }

        // Gun held by player, use the player's camera as the game does
        var gameplayCamera = shotgun.playerHeldBy.gameplayCamera.transform;
        var position = gameplayCamera.position;
        var forward = gameplayCamera.forward;
        return (position - gameplayCamera.up * 0.45f, forward);
    }
}
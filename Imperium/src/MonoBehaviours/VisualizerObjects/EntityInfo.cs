#region

using System;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects;

public class EntityInfo : MonoBehaviour
{
    private EnemyAI entityController;

    private GameObject canvas;
    private TMP_Text nameText;
    private TMP_Text healthText;
    private TMP_Text stateText;
    private TMP_Text movementSpeedText;
    private TMP_Text stunTimeText;
    private TMP_Text targetText;
    private Image deathOverlay;

    private LineRenderer lookAtLine;

    private LineRenderer targetPlayer;

    private readonly LineRenderer[] pathLines = new LineRenderer[20];

    private void Awake()
    {
        canvas = transform.Find("Canvas").gameObject;
        nameText = transform.Find("Canvas/Name").GetComponent<TMP_Text>();
        healthText = transform.Find("Canvas/Health/Value").GetComponent<TMP_Text>();
        stateText = transform.Find("Canvas/State/Value").GetComponent<TMP_Text>();
        movementSpeedText = transform.Find("Canvas/MovementSpeed/Value").GetComponent<TMP_Text>();
        stunTimeText = transform.Find("Canvas/StunTime/Value").GetComponent<TMP_Text>();
        targetText = transform.Find("Canvas/Target/Value").GetComponent<TMP_Text>();

        deathOverlay = transform.Find("Canvas/Death").GetComponent<Image>();
    }

    internal void Init(EnemyAI entity)
    {
        entityController = entity;
        lookAtLine = ImpUtils.Geometry.CreateLine(entity.transform, 0.02f, true);

        targetPlayer = ImpUtils.Geometry.CreateLine(entity.transform, 0.02f, true);
        for (var i = 0; i < pathLines.Length; i++)
        {
            pathLines[i] = ImpUtils.Geometry.CreateLine(transform, 0.05f, true);
        }
    }

    private void Update()
    {
        canvas.SetActive(entityController);
        if (!entityController) return;

        deathOverlay.gameObject.SetActive(entityController.isEnemyDead);

        nameText.text = Imperium.ObjectManager.GetDisplayName(entityController.enemyType.enemyName);

        healthText.text = entityController.enemyHP.ToString();

        var state = entityController.currentBehaviourStateIndex.ToString();
        stateText.text = state;

        var movementSpeed = ImpUtils.Math.FormatFloatToThreeDigits(entityController.agent.speed);
        movementSpeedText.text = movementSpeed;

        var stunTime = $"{ImpUtils.Math.FormatFloatToThreeDigits(Math.Max(0, entityController.stunNormalizedTimer))}s";
        stunTimeText.text = stunTime;

        var target = entityController.targetPlayer ? entityController.targetPlayer.playerUsername : "-";
        targetText.text = target;

        canvas.transform.LookAt(Imperium.Freecam.IsFreecamEnabled.Value
            ? Imperium.Freecam.transform.position
            : Imperium.Player.gameplayCamera.transform.position);

        DrawLookingLine();
        DrawTargetPlayerLine();
        DrawPathLines();
    }

    private void DrawPathLines()
    {
        var corners = entityController.agent.path.corners;
        var previousCorner = entityController.transform.position;
        for (var i = 0; i < pathLines.Length; i++)
        {
            if (i < corners.Length)
            {
                pathLines[i].gameObject.SetActive(true);
                ImpUtils.Geometry.SetLinePositions(
                    pathLines[i],
                    previousCorner,
                    corners[i]
                );
                ImpUtils.Geometry.SetLineColor(pathLines[i], Color.white);
                previousCorner = corners[i];
            }
            else
            {
                pathLines[i].gameObject.SetActive(false);
            }
        }
    }

    private void DrawLookingLine()
    {
        Vector3? lookAtPosition = null;

        switch (entityController)
        {
            case HoarderBugAI hoarderBug when hoarderBug.lookTarget && hoarderBug.lookingAtPositionOfInterest:
                lookAtPosition = hoarderBug.lookTarget.position;
                break;
            case PufferAI puffer:
                lookAtPosition = Reflection.Get<PufferAI, Vector3>(puffer, "lookAtNoise");
                break;
        }

        if (lookAtPosition.HasValue && lookAtPosition != Vector3.zero)
        {
            lookAtLine.gameObject.SetActive(true);

            ImpUtils.Geometry.SetLinePositions(
                lookAtLine,
                entityController.transform.position,
                lookAtPosition.Value
            );
            ImpUtils.Geometry.SetLineColor(lookAtLine, new Color(0.47f, 0.66f, 0.35f));
        }
        else
        {
            lookAtLine.gameObject.SetActive(false);
        }
    }

    private void DrawTargetPlayerLine()
    {
        if (entityController.movingTowardsTargetPlayer && entityController.targetPlayer)
        {
            targetPlayer.gameObject.SetActive(true);

            ImpUtils.Geometry.SetLinePositions(
                targetPlayer,
                entityController.transform.position,
                entityController.targetPlayer.transform.position
            );
            ImpUtils.Geometry.SetLineColor(targetPlayer, Color.red);
        }
        else
        {
            targetPlayer.gameObject.SetActive(false);
        }
    }
}
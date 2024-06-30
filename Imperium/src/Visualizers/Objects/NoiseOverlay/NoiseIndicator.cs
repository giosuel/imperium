#region

using System;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Visualizers.Objects.NoiseOverlay;

public class NoiseIndicator : MonoBehaviour
{
    private float timer;
    private float totalTime;
    private int noiseId;

    private Vector3 worldPosition;
    private bool isDone;
    private TMP_Text distanceText;
    private TMP_Text noiseIdText;
    private Image image;

    private Canvas canvas;

    private RectTransform indicatorTransform;
    private RectTransform arrowTransform;

    private readonly Color indicatorColor = new(0.737f, 0.463f, 0.243f);

    internal void Init(Canvas parent)
    {
        canvas = parent;
        distanceText = transform.Find("Text").GetComponent<TMP_Text>();
        noiseIdText = transform.Find("NoiseId").GetComponent<TMP_Text>();
        image = transform.Find("Image").GetComponent<Image>();
        indicatorTransform = GetComponent<RectTransform>();
        arrowTransform = transform.Find("Arrow").GetComponent<RectTransform>();
    }

    internal void Activate(Vector3 position, float duration, float loudness, int noiseID)
    {
        timer = duration;
        totalTime = duration;
        noiseId = noiseID;

        worldPosition = position;
        isDone = false;
        distanceText.color = ImpUtils.Interface.ChangeAlpha(indicatorColor, 1);
        image.color = ImpUtils.Interface.ChangeAlpha(indicatorColor, 1);
        gameObject.SetActive(true);
        transform.localScale = Vector3.one * loudness * 2;
    }

    private void LateUpdate()
    {
        if (timer >= 0)
        {
            var camera = Imperium.Freecam.IsFreecamEnabled.Value
                ? Imperium.Freecam.FreecamCamera
                : Imperium.Player.hasBegunSpectating
                    ? Imperium.StartOfRound.spectateCamera
                    : Imperium.Player.gameplayCamera;

            var targetPos = camera.WorldToScreenPoint(worldPosition);
            if (targetPos.z < 0) targetPos *= -1;

            var activeTexture = camera.activeTexture;
            var scaleFactor = activeTexture.width / canvas.GetComponent<RectTransform>().sizeDelta.x;

            var textureX = activeTexture.width / scaleFactor;
            var textureY = activeTexture.height / scaleFactor;

            var margin = activeTexture.width / scaleFactor * 0.05f;
            var positionX = targetPos.x / scaleFactor;
            var positionY = targetPos.y / scaleFactor;
            var clamped = false;
            if (positionX < margin || positionX > textureX - margin)
            {
                positionX = Math.Clamp(positionX, margin, textureX - margin);
                clamped = true;
            }

            if (positionY < margin || positionY > textureY - margin * 2)
            {
                positionY = Math.Clamp(positionY, margin, textureY - margin * 2);
                clamped = true;
            }

            arrowTransform.gameObject.SetActive(clamped);
            indicatorTransform.anchoredPosition = new Vector2(positionX, positionY);
            var playerPosition = Imperium.Player.transform.position;
            transform.localScale = Vector3.one * Math.Clamp(
                5 / Vector3.Distance(playerPosition, worldPosition),
                0.5f, 1f
            );

            var angle = Vector2.SignedAngle(
                new Vector2(0, 1),
                new Vector2(textureX / 2f - positionX, textureY / 2f - positionY).normalized
            );
            arrowTransform.rotation = Quaternion.Euler(0, 0, angle + 180);

            timer -= Time.deltaTime;
            distanceText.text = $"{Vector3.Distance(playerPosition, worldPosition):0.0}m";
            noiseIdText.text = $"ID: {noiseId}";

            distanceText.color = ImpUtils.Interface.ChangeAlpha(
                indicatorColor, image.color.a - Time.deltaTime / totalTime
            );
            image.color = ImpUtils.Interface.ChangeAlpha(
                indicatorColor, image.color.a - Time.deltaTime / totalTime
            );
            noiseIdText.color = ImpUtils.Interface.ChangeAlpha(
                indicatorColor, image.color.a - Time.deltaTime / totalTime
            );
        }
        else if (!isDone)
        {
            isDone = true;
            gameObject.SetActive(false);
        }
    }
}
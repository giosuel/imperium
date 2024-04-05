#region

using System;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects;

public class SpawnIndicator : MonoBehaviour
{
    private float spawnTime = -1;
    private string entityName;
    private GameObject canvas;
    private TMP_Text timeText;
    private TMP_Text entityText;

    private void Awake()
    {
        canvas = transform.Find("Canvas").gameObject;
        timeText = transform.Find("Canvas/Time").GetComponent<TMP_Text>();
        entityText = transform.Find("Canvas/Entity").GetComponent<TMP_Text>();
    }

    internal void Init(string spawnEntityName, float time)
    {
        entityName = spawnEntityName;
        spawnTime = time;
    }

    private void Update()
    {
        var timeLeft = Math.Max(
            (spawnTime - Imperium.TimeOfDay.currentDayTime) / Imperium.GameManager.TimeSpeed.Value,
            0
        );

        // Remove indicator when entity is spawned
        if (timeLeft == 0) Destroy(gameObject);

        entityText.text = entityName;

        var minutesLeft = Mathf.RoundToInt(timeLeft) / 60;
        var secondsLeft = Mathf.RoundToInt(timeLeft) % 60;
        timeText.text = $"{minutesLeft}:{secondsLeft:00}";

        if (!Imperium.Player) return;
        canvas.transform.LookAt(Imperium.Player.transform);
    }
}
#region

using System;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Visualizers.MonoBehaviours;

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
            (spawnTime - Imperium.TimeOfDay.currentDayTime) / Imperium.MoonManager.TimeSpeed.Value,
            0
        );

        // Remove indicator when entity is spawned
        if (timeLeft == 0) Destroy(gameObject);

        entityText.text = entityName;

        timeText.text = Formatting.FormatMinutesSeconds(timeLeft);

        if (!Imperium.Player) return;
        canvas.transform.LookAt(Imperium.Player.transform);
    }
}
#region

using System;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Visualizers.MonoBehaviours;

public class VentTimer : MonoBehaviour
{
    public EnemyVent vent;
    private TMP_Text timeText;
    private TMP_Text entityText;

    private void Start()
    {
        timeText = transform.Find("Canvas/Time").GetComponent<TMP_Text>();
        entityText = transform.Find("Canvas/Entity").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        var timeLeft = Math.Max(
            (vent.spawnTime - Imperium.TimeOfDay.currentDayTime) / Imperium.MoonManager.TimeSpeed.Value,
            0
        );

        switch (timeLeft)
        {
            case 0:
                timeText.text = "xx:xx";
                entityText.text = "???";
                return;
            case -1:
                timeText.text = "";
                entityText.text = "";
                return;
            default:
                entityText.text = Imperium.ObjectManager.GetDisplayName(vent.enemyType.enemyName);
                timeText.text = Formatting.FormatMinutesSeconds(timeLeft);
                return;
        }
    }
}
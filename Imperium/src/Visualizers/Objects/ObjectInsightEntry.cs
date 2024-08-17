#region

using System;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Visualizers.Objects;

public class ObjectInsightEntry : MonoBehaviour
{
    private Component targetComponent;
    private Func<Component, string> insightGenerator;
    private TMP_Text insightValueText;

    private readonly ImpTimer entryUpdateTimer = ImpTimer.ForInterval(0.2f);

    public void Init(string insightName, Func<Component, string> generator, Component target)
    {
        insightGenerator = generator;
        targetComponent = target;

        insightValueText = transform.Find("Value").GetComponent<TMP_Text>();
        transform.Find("Title").GetComponent<TMP_Text>().text = insightName;

        insightValueText.text = insightGenerator(targetComponent);
    }

    private void Update()
    {
        /*
         * We only want to execute the insight generator function every so often to save frames.
         *
         * This is because this function is defined by the insight provider and could possibly be inefficient.
         */
        if (entryUpdateTimer.Tick()) insightValueText.text = insightGenerator(targetComponent);
    }
}
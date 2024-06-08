#region

using System;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Visualizers.MonoBehaviours;

public class ObjectInsightEntry : MonoBehaviour
{
    private Component targetComponent;
    private Func<Component, string> insightGenerator;
    private TMP_Text insightValueText;

    public void Init(string insightName, Func<Component, string> generator, Component target)
    {
        insightGenerator = generator;
        targetComponent = target;

        insightValueText = transform.Find("Value").GetComponent<TMP_Text>();
        transform.Find("Title").GetComponent<TMP_Text>().text = insightName;
    }

    private void Update()
    {
        insightValueText.text = insightGenerator(targetComponent);
    }
}
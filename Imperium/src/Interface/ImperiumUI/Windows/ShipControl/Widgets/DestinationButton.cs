#region

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ShipControl.Widgets;

public class DestinationButton : MonoBehaviour
{
    private Button button;
    private SelectableLevel level;

    public void Init(int levelIndex, Action navigateCallback)
    {
        level = Imperium.StartOfRound.levels[levelIndex];

        button = GetComponent<Button>();
        transform.Find("Text").GetComponent<TMP_Text>().text = level.PlanetName;

        button.onClick.AddListener(() => navigateCallback());
    }

    public void OnOpen()
    {
        button.interactable = Imperium.RoundManager.currentLevel != level && !Imperium.IsSceneLoaded.Value;
    }
}
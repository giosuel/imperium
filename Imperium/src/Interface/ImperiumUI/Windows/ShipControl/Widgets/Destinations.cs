#region

using System.Collections.Generic;
using Imperium.Core.Lifecycle;
using Imperium.Types;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ShipControl.Widgets;

public class Destinations : ImpWidget
{
    private GameObject templateButton;
    private Transform buttonContainer;

    private readonly List<GameObject> buttons = [];

    protected override void InitWidget()
    {
        buttonContainer = transform.Find("ScrollView/Viewport/Content");
        templateButton = buttonContainer.Find("Template").gameObject;
        templateButton.SetActive(false);

        for (var i = 0; i < Imperium.StartOfRound.levels.Length; i++) RegisterButton(i);
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("", Variant.DARKER),
            new StyleOverride("ScrollView/Scrollbar", Variant.DARKEST),
            new StyleOverride("ScrollView/Scrollbar/SlidingArea/Handle", Variant.FOREGROUND)
        );

        foreach (var button in buttons)
        {
            ImpThemeManager.Style(
                themeUpdate,
                button.transform,
                new StyleOverride("", Variant.DARKER)
            );
        }
    }

    private void RegisterButton(int levelIndex)
    {
        var navigationButtonObj = Instantiate(templateButton, buttonContainer);
        navigationButtonObj.SetActive(true);

        var navigationButton = navigationButtonObj.AddComponent<DestinationButton>();
        navigationButton.Init(levelIndex, () => Imperium.ShipManager.NavigateTo(levelIndex));
        onOpen += navigationButton.OnOpen;

        buttons.Add(navigationButtonObj);
    }
}
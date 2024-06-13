#region

using System.Collections.Generic;
using Imperium.Core;
using Imperium.Core.Lifecycle;
using Imperium.Types;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.NavigatorUI;

internal class NavigatorUI : SingleplexUI
{
    private GameObject templateButton;
    private Transform buttonContainer;

    private readonly List<GameObject> buttons = [];

    protected override void InitUI()
    {
        buttonContainer = content.Find("Buttons");
        templateButton = content.Find("Buttons/Template").gameObject;
        templateButton.SetActive(false);

        for (var i = 0; i < Imperium.StartOfRound.levels.Length; i++) RegisterButton(i);
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
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

        var navigationButton = navigationButtonObj.AddComponent<NavigatorButton>();
        navigationButton.Init(levelIndex, () =>
        {
            GameManager.NavigateTo(levelIndex);
            CloseUI();
        });
        onOpen += navigationButton.OnOpen;

        buttons.Add(navigationButtonObj);
    }
}
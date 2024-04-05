#region

using Imperium.Core;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.NavigatorUI;

internal class NavigatorUI : StandaloneUI
{
    private GameObject templateObject;
    private Transform buttonList;

    public override void Awake() => InitializeUI();

    protected override void InitUI()
    {
        templateObject = content.Find("Buttons/Template").gameObject;
        templateObject.SetActive(false);

        buttonList = content.Find("Buttons");

        for (var i = 0; i < Imperium.StartOfRound.levels.Length; i++) RegisterButton(i);
    }

    private void RegisterButton(int levelIndex)
    {
        var navigationButtonObj = Instantiate(templateObject, buttonList);
        navigationButtonObj.SetActive(true);
        var navigationButton = navigationButtonObj.AddComponent<NavigatorButton>();
        navigationButton.Init(levelIndex, () =>
        {
            GameManager.NavigateTo(levelIndex);
            CloseUI();
        });
        onOpen += navigationButton.OnOpen;
    }
}
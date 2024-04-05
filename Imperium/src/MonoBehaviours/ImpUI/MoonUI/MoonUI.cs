#region

using Imperium.MonoBehaviours.ImpUI.MoonUI.Windows;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI;

internal class MoonUI : MultiplexUI
{
    public override void Awake() => InitializeUI();

    protected override void InitUI()
    {
        RegisterWindow<ControlCenterWindow>("ControlCenter");
        RegisterWindow<MoonInfoWindow>("MoonInfo");
        RegisterWindow<ChallengeInfoWindow>("ChallengeMoonInfo");
        RegisterWindow<SpawnListsWindow>("SpawnLists");
    }
}
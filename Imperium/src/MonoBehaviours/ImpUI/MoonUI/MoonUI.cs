#region

using Imperium.MonoBehaviours.ImpUI.MoonUI.Windows;
using Imperium.Types;
using Imperium.Util.Binding;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI;

internal class MoonUI : MultiplexUI
{
    protected override void InitUI()
    {
        RegisterWindow<ControlCenterWindow>("ControlCenter", theme);
        RegisterWindow<MoonInfoWindow>("MoonInfo", theme);
        RegisterWindow<ChallengeInfoWindow>("ChallengeMoonInfo", theme);
        //RegisterWindow<SpawnListsWindow>("SpawnLists", theme);
    }
}
#region

using Imperium.MonoBehaviours.ImpUI.ImperiumUI.Windows;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI;

internal class ImperiumUI : MultiplexUI
{
    internal ObjectExplorerWindow ObjectExplorerWindow;

    public override void Awake() => InitializeUI();

    protected override void InitUI()
    {
        RegisterWindow<ControlCenterWindow>("ControlCenter", false);
        ObjectExplorerWindow = RegisterWindow<ObjectExplorerWindow>("ObjectExplorer", false);
    }
}
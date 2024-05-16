#region

using Imperium.MonoBehaviours.ImpUI.ImperiumUI.Windows;
using Imperium.Types;
using Imperium.Util.Binding;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ImperiumUI;

internal class ImperiumUI : MultiplexUI
{
    internal ObjectExplorerWindow ObjectExplorerWindow;

    protected override void InitUI()
    {
        RegisterWindow<ControlCenterWindow>("ControlCenter", theme);
        ObjectExplorerWindow = RegisterWindow<ObjectExplorerWindow>("ObjectExplorer", theme);
    }
}
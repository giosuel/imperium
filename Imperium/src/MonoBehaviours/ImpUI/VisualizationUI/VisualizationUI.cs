#region

using Imperium.MonoBehaviours.ImpUI.VisualizationUI.Windows;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.VisualizationUI;

internal class VisualizationUI : MultiplexUI
{
    protected override void InitUI()
    {
        RegisterWindow<VisualizersWindows>("VisualizersWindow", theme);
        RegisterWindow<ObjectVisualizersWindow>("ObjectVisualizersWindow", theme);
    }
}
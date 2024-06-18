#region

using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI;
using Imperium.MonoBehaviours.ImpUI.Common;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SaveUI;

internal class ConfirmationWindow : ImperiumWindow
{
    protected override void InitWindow()
    {
        ImpButton.Bind("Content/Back", transform, Close, theme);

        // Unthemed button, as it is red in all themes
        ImpButton.Bind("Content/Confirm", transform, () => Imperium.Interface.Open<SaveEditorWindow>());
    }

    protected override void OnOpen()
    {
        RoundManager.PlayRandomClip(Imperium.HUDManager.UIAudio, Imperium.HUDManager.warningSFX, randomize: false);
    }
}
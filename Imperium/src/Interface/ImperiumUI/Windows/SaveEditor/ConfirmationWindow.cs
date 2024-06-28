#region

using Imperium.Interface.Common;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.SaveEditor;

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
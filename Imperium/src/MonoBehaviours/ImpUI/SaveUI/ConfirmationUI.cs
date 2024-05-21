#region

using Imperium.MonoBehaviours.ImpUI.Common;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SaveUI;

internal class ConfirmationUI : SingleplexUI
{
    protected override void InitUI()
    {
        ImpButton.Bind("Back", content, CloseUI, theme);

        // Unthemed button, as it is red in all themes
        ImpButton.Bind("Confirm", content, () => Imperium.Interface.Open<SaveUI>());
    }

    protected override void OnOpen()
    {
        RoundManager.PlayRandomClip(Imperium.HUDManager.UIAudio, Imperium.HUDManager.warningSFX, randomize: false);
    }
}
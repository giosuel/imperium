#region

using Imperium.MonoBehaviours.ImpUI.Common;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SaveUI;

internal class ConfirmationUI : StandaloneUI
{
    public override void Awake() => InitializeUI();

    protected override void InitUI()
    {
        ImpButton.Bind("Confirm", content, () => Imperium.Interface.Open<SaveUI>());
        ImpButton.Bind("Back", content, CloseUI);
    }

    protected override void OnOpen()
    {
        RoundManager.PlayRandomClip(Imperium.HUDManager.UIAudio, Imperium.HUDManager.warningSFX, randomize: false);
    }
}
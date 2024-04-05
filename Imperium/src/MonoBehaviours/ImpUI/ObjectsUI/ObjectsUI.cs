#region

using System.Linq;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ObjectsUI;

internal class ObjectsUI : StandaloneUI
{
    public override void Awake() => InitializeUI();

    protected override void InitUI()
    {
        InitShotgun();
        InitShovel();
        InitJester();
    }

    private void InitShotgun()
    {
        ImpToggle.Bind("Shotgun/InfiniteAmmo", content, ImpSettings.Shotgun.InfiniteAmmo);
        ImpToggle.Bind("Shotgun/FullAuto", content, ImpSettings.Shotgun.FullAuto);
    }

    private void InitShovel()
    {
        ImpToggle.Bind("Shovel/Speedy", content, ImpSettings.Shovel.Speedy);
    }

    private void InitJester()
    {
        ImpButton.Bind("Jester/Pop", content, () =>
        {
            Imperium.ObjectManager.CurrentLevelEntities.Value
                .Where(obj => obj)
                .Where(obj => obj is JesterAI)
                .ToList()
                .ForEach(jester =>
                {
                    jester.creatureAnimator.SetBool("turningCrank", value: true);
                    jester.SwitchToBehaviourState(2);
                });
        });

        ImpButton.Bind("Jester/PopCursed", content, () =>
        {
            Imperium.ObjectManager.CurrentLevelEntities.Value
                .Where(obj => obj)
                .Where(obj => obj is JesterAI)
                .ToList()
                .ForEach(jester => jester.SwitchToBehaviourState(2));
        });

        ImpButton.Bind("Jester/Reset", content, () =>
        {
            Imperium.ObjectManager.CurrentLevelEntities.Value
                .Where(obj => obj)
                .Where(obj => obj is JesterAI)
                .ToList()
                .ForEach(jester => jester.SwitchToBehaviourState(0));
        });
    }
}
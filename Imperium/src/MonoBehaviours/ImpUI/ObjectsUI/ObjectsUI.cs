#region

using System.Linq;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.ObjectsUI;

internal class ObjectsUI : SingleplexUI
{
    protected override void InitUI()
    {
        InitShotgun();
        InitShovel();
        InitJester();
    }

    private void InitShotgun()
    {
        ImpToggle.Bind("Shotgun/InfiniteAmmo", content, ImpSettings.Shotgun.InfiniteAmmo, theme: theme);
        ImpToggle.Bind("Shotgun/FullAuto", content, ImpSettings.Shotgun.FullAuto, theme: theme);
    }

    private void InitShovel()
    {
        ImpToggle.Bind("Shovel/Speedy", content, ImpSettings.Shovel.Speedy, theme: theme);
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
        }, theme: theme);

        ImpButton.Bind("Jester/PopCursed", content, () =>
        {
            Imperium.ObjectManager.CurrentLevelEntities.Value
                .Where(obj => obj)
                .Where(obj => obj is JesterAI)
                .ToList()
                .ForEach(jester => jester.SwitchToBehaviourState(2));
        }, theme: theme);

        ImpButton.Bind("Jester/Reset", content, () =>
        {
            Imperium.ObjectManager.CurrentLevelEntities.Value
                .Where(obj => obj)
                .Where(obj => obj is JesterAI)
                .ToList()
                .ForEach(jester => jester.SwitchToBehaviourState(0));
        }, theme: theme);
    }
}
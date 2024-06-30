#region

using System.Linq;
using Imperium.Interface.Common;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectSettings;

internal class ObjectSettingsWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        InitShotgun();
        InitShovel();
        InitJester();
    }

    private void InitShotgun()
    {
        ImpToggle.Bind("Shotgun/InfiniteAmmo", content, Imperium.Settings.Shotgun.InfiniteAmmo, theme: theme);
        ImpToggle.Bind("Shotgun/FullAuto", content, Imperium.Settings.Shotgun.FullAuto, theme: theme);
    }

    private void InitShovel()
    {
        ImpToggle.Bind("Shovel/Speedy", content, Imperium.Settings.Shovel.Speedy, theme: theme);
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
#region

using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ShipControl.Widgets;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.CruiserControl;

internal class CruiserControlWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        InitSettings();
    }

    private void InitSettings()
    {
        ImpToggle.Bind(
            "Settings/Indestructible",
            content,
            Imperium.CruiserManager.Indestructible,
            theme: theme
        );
        ImpToggle.Bind(
            "Settings/InfiniteTurbo",
            content,
            Imperium.CruiserManager.InfiniteTurbo,
            theme: theme
        );
        ImpToggle.Bind(
            "Settings/SpawnFullTurbo",
            content,
            Imperium.CruiserManager.SpawnFullTurbo,
            theme: theme
        );
        ImpToggle.Bind(
            "Settings/InstantIgnite",
            content,
            Imperium.Settings.Cruiser.InstantIgnite,
            theme: theme
        );

        ImpSlider.Bind(
            path: "PushForce",
            container: content,
            theme: theme,
            debounceTime: 0.1f,
            valueBinding: Imperium.CruiserManager.PushForce
        );

        ImpSlider.Bind(
            path: "Acceleration",
            container: content,
            theme: theme,
            debounceTime: 0.1f,
            valueBinding: Imperium.CruiserManager.Acceleration
        );
    }
}
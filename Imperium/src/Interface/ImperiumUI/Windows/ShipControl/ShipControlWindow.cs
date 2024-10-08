#region

using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ShipControl.Widgets;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ShipControl;

internal class ShipControlWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        InitSettings();

        RegisterWidget<Destinations>(content, "Destinations");
    }

    private void InitSettings()
    {
        ImpToggle.Bind(
            "ShipSettings/InstantLanding",
            content,
            Imperium.ShipManager.InstantLanding,
            theme: theme
        );
        ImpToggle.Bind(
            "ShipSettings/InstantTakeoff",
            content,
            Imperium.ShipManager.InstantTakeoff,
            theme: theme
        );
        ImpToggle.Bind(
            "ShipSettings/OverwriteDoors",
            content,
            Imperium.Settings.Ship.OverwriteDoors,
            theme: theme
        );
        ImpToggle.Bind(
            "ShipSettings/PreventLeave",
            content,
            Imperium.ShipManager.PreventShipLeave,
            theme: theme
        );
        ImpToggle.Bind(
            "ShipSettings/DisableAbandoned",
            content,
            Imperium.ShipManager.DisableAbandoned,
            theme: theme
        );
        ImpToggle.Bind(
            "ShipSettings/MuteSpeaker",
            content,
            Imperium.Settings.Ship.MuteSpeaker,
            theme: theme
        );
    }
}
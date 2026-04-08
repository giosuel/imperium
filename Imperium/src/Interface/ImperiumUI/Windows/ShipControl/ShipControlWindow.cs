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
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Instant Landing",
                Description = "Skips the ship's landing animation.",
                Tooltip = tooltip
            }
        );
        ImpToggle.Bind(
            "ShipSettings/InstantTakeoff",
            content,
            Imperium.ShipManager.InstantTakeoff,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Instant Takeoff",
                Description = "Skips the ship's take-off animation.",
                Tooltip = tooltip
            }
        );
        ImpToggle.Bind(
            "ShipSettings/OverrideDoors",
            content,
            Imperium.Settings.Ship.OverwriteDoors,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Override Doors",
                Description = "Enables door ship controls when in space.",
                Tooltip = tooltip
            }
        );
        ImpToggle.Bind(
            "ShipSettings/PreventLeave",
            content,
            Imperium.ShipManager.PreventShipLeave,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Prevent Leave",
                Description = "Disables the ship's automatic leave timer.",
                Tooltip = tooltip
            }
        );
        ImpToggle.Bind(
            "ShipSettings/DisableAbandoned",
            content,
            Imperium.ShipManager.DisableAbandoned,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Disable Abandoned",
                Description =
                    "Prevents the game from killing abandoned players.\nAll players will be teleported into the ship instead.",
                Tooltip = tooltip
            }
        );
        ImpToggle.Bind(
            "ShipSettings/MuteSpeaker",
            content,
            Imperium.Settings.Ship.MuteSpeaker,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Mute Speaker",
                Description = "Please just shut up!",
                Tooltip = tooltip
            }
        );
    }
}
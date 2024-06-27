#region

using System;
using System.Collections.Generic;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.Teleportation.Widgets;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Teleportation;

internal class TeleportationWindow : ImperiumWindow
{
    private Button tpMainEntrance;
    private Button tpShip;
    private Button tpApparatus;
    private TMP_Dropdown fireExitsDropdown;

    private readonly List<Vector3> fireExits = [];

    private ImpBinding<float> coordinateX;
    private ImpBinding<float> coordinateY;
    private ImpBinding<float> coordinateZ;

    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        tpMainEntrance = ImpButton.Bind(
            "Presets/MainEntrance", content,
            () => TeleportTo(Imperium.PlayerManager.MainEntranceTPAnchor.Value),
            theme
        );
        tpShip = ImpButton.Bind(
            "Presets/Ship", content,
            () => TeleportTo(Imperium.PlayerManager.ShipTPAnchor.Value),
            theme
        );
        tpApparatus = ImpButton.Bind(
            "Presets/Apparatus", content,
            () => TeleportTo(Imperium.PlayerManager.ApparatusTPAnchor.Value),
            theme
        );
        ImpButton.Bind(
            "Presets/Freecam", content,
            () => TeleportTo(Imperium.Freecam.transform.position),
            theme
        );

        // We need to set the teleport function as sync callback as the game might teleport the player to different
        // coordinates due to OOB restrictions. That way, the input field would be out of sync with the actual position,
        // so we have to re-set the coords without invoking another teleport that would lead to a stack overflow.
        coordinateX = new ImpBinding<float>(0, onUpdateFromLocal: _ => TeleportToCoords());
        coordinateY = new ImpBinding<float>(0, onUpdateFromLocal: _ => TeleportToCoords());
        coordinateZ = new ImpBinding<float>(0, onUpdateFromLocal: _ => TeleportToCoords());

        ImpInput.Bind("Coords/CoordsX", content, coordinateX, theme, max: 10000f, min: -10000f);
        ImpInput.Bind("Coords/CoordsY", content, coordinateY, theme, max: 999f, min: -999f);
        ImpInput.Bind("Coords/CoordsZ", content, coordinateZ, theme, max: 10000f, min: -10000f);

        fireExitsDropdown = content.Find("FireExits").GetComponent<TMP_Dropdown>();
        fireExitsDropdown.onValueChanged.AddListener(_ => TeleportTo(fireExits[fireExitsDropdown.value]));

        ImpButton.Bind("Buttons/Interactive", content, OnInteractive, theme);

        RegisterWidget<Waypoints>(content, "Waypoints");

        Imperium.InputBindings.BaseMap.Teleport.performed += OnTeleport;
    }

    protected override void OnOpen()
    {
        tpShip.interactable = Imperium.PlayerManager.ShipTPAnchor.Value != null;
        tpMainEntrance.interactable = Imperium.PlayerManager.MainEntranceTPAnchor.Value != null
                                      && Imperium.IsSceneLoaded.Value;
        tpApparatus.interactable = Imperium.PlayerManager.ApparatusTPAnchor.Value != null
                                   && Imperium.IsSceneLoaded.Value;

        var position = Imperium.Player.transform.position;
        coordinateX.Set(MathF.Round(position.x, 2));
        coordinateY.Set(MathF.Round(position.y, 2));
        coordinateZ.Set(MathF.Round(position.z, 2));

        FillFireExitDropdown();
    }

    private static void OnInteractive()
    {
        Imperium.Freecam.IsFreecamEnabled.Set(false);
        Imperium.ImpPositionIndicator.Activate(Imperium.PlayerManager.TeleportLocalPlayer);
    }

    private void TeleportTo(Vector3? anchor)
    {
        if (anchor == null) return;
        Imperium.PlayerManager.TeleportLocalPlayer(anchor.Value);
        CloseParent();
    }

    private void TeleportToCoords()
    {
        Imperium.PlayerManager.TeleportLocalPlayer(new Vector3(
            coordinateX.Value,
            coordinateY.Value,
            coordinateZ.Value
        ));
        var playerPosition = Imperium.Player.transform.position;
        coordinateX.Set(playerPosition.x);
        coordinateY.Set(playerPosition.y);
        coordinateZ.Set(playerPosition.z);
    }

    private void FillFireExitDropdown()
    {
        fireExitsDropdown.options.Clear();
        fireExits.Clear();
        var objs = FindObjectsOfType<GameObject>();
        foreach (var obj in objs)
        {
            if (obj.name != "FireExitDoor" || obj.transform.position.y > -100) continue;

            var location = obj.transform.position.y > -100 ? "Outdoor" : "Indoor";

            fireExitsDropdown.options.Add(
                new TMP_Dropdown.OptionData($"Fire Exit #{fireExits.Count + 1} ({location})"));
            fireExits.Add(obj.transform.position);
        }

        fireExitsDropdown.interactable = fireExits.Count != 0;
    }

    private static void OnTeleport(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat) return;

        // Set origin of indicator to freecam if freecam is enabled
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;

        if (Imperium.ImpPositionIndicator.IsActive)
        {
            Imperium.ImpPositionIndicator.HideIndicator();
        }
        else
        {
            Imperium.ImpPositionIndicator.Activate(Imperium.PlayerManager.TeleportLocalPlayer, origin);
        }
    }
}
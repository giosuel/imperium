#region

using System;
using System.Collections.Generic;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.TeleportUI.Windows;

internal class TeleportWindow : BaseWindow
{
    private Button tpMainEntrance;
    private Button tpShip;
    private Button tpApparatus;
    private TMP_Dropdown fireExitsDropdown;

    private readonly List<Vector3> fireExits = [];

    private ImpBinding<float> coordinateX;
    private ImpBinding<float> coordinateY;
    private ImpBinding<float> coordinateZ;

    protected override void RegisterWindow()
    {
        tpMainEntrance = ImpButton.Bind(
            "Presets/MainEntrance", content,
            () => TeleportTo(Imperium.PlayerManager.MainEntranceTPAnchor.Value),
            themeBinding
        );
        tpShip = ImpButton.Bind(
            "Presets/Ship", content,
            () => TeleportTo(Imperium.PlayerManager.ShipTPAnchor.Value),
            themeBinding
        );
        tpApparatus = ImpButton.Bind(
            "Presets/Apparatus", content,
            () => TeleportTo(Imperium.PlayerManager.ApparatusTPAnchor.Value),
            themeBinding
        );
        ImpButton.Bind(
            "Presets/Freecam", content,
            () => TeleportTo(Imperium.Freecam.transform.position),
            themeBinding
        );

        // We need to set the teleport function as sync callback as the game might teleport the player to different
        // coordinates due to OOB restrictions. That way, the input field would be out of sync with the actual position,
        // so we have to re-set the coords without invoking another teleport that would lead to a stack overflow.
        coordinateX = new ImpBinding<float>(0, onUpdateFromLocal: _ => TeleportToCoords());
        coordinateY = new ImpBinding<float>(0, onUpdateFromLocal: _ => TeleportToCoords());
        coordinateZ = new ImpBinding<float>(0, onUpdateFromLocal: _ => TeleportToCoords());

        ImpInput.Bind("Coords/CoordsX", content, coordinateX, themeBinding, max: 10000f, min: -10000f);
        ImpInput.Bind("Coords/CoordsY", content, coordinateY, themeBinding, max: 999f, min: -999f);
        ImpInput.Bind("Coords/CoordsZ", content, coordinateZ, themeBinding, max: 10000f, min: -10000f);

        fireExitsDropdown = content.Find("FireExits").GetComponent<TMP_Dropdown>();
        fireExitsDropdown.onValueChanged.AddListener(_ => TeleportTo(fireExits[fireExitsDropdown.value]));

        ImpButton.Bind("Buttons/Interactive", content, OnInteractive, themeBinding);
    }

    protected override void OnOpen()
    {
        tpShip.interactable = Imperium.PlayerManager.ShipTPAnchor.Value != null;
        tpMainEntrance.interactable = Imperium.PlayerManager.MainEntranceTPAnchor.Value != null;
        tpApparatus.interactable = Imperium.PlayerManager.ApparatusTPAnchor.Value != null;

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
        CloseUI();
    }

    private void TeleportToCoords()
    {
        Imperium.PlayerManager.TeleportLocalPlayer(new Vector3(
            coordinateX.Value,
            coordinateY.Value,
            coordinateZ.Value
        ));
        var playerPosition = Imperium.Player.transform.position;
        coordinateX.Set(playerPosition.x, true);
        coordinateY.Set(playerPosition.y, true);
        coordinateZ.Set(playerPosition.z, true);
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
}
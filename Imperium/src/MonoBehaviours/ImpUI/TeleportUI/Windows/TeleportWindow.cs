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
            () => TeleportTo(Imperium.PlayerManager.MainEntranceTPAnchor.Value)
        );
        tpShip = ImpButton.Bind(
            "Presets/Ship", content,
            () => TeleportTo(Imperium.PlayerManager.ShipTPAnchor.Value)
        );
        tpApparatus = ImpButton.Bind(
            "Presets/Apparatus", content,
            () => TeleportTo(Imperium.PlayerManager.ApparatusTPAnchor.Value)
        );
        ImpButton.Bind(
            "Presets/Freecam", content,
            () => TeleportTo(Imperium.Freecam.transform.position)
        );
        
        coordinateX = new ImpBinding<float>(0, _ => TeleportToCoords());
        coordinateY = new ImpBinding<float>(0, _ => TeleportToCoords());
        coordinateZ = new ImpBinding<float>(0, _ => TeleportToCoords());

        ImpInput.Bind("Coords/CoordsX", content, coordinateX, max: 10000f, min: -10000f);
        ImpInput.Bind("Coords/CoordsY", content, coordinateY, max: 999f, min: -999f);
        ImpInput.Bind("Coords/CoordsZ", content, coordinateZ, max: 10000f, min: -10000f);

        // Every input field submit teleports player to combined coords
        coordinateX.onUpdate += _ => TeleportToCoords();
        coordinateY.onUpdate += _ => TeleportToCoords();
        coordinateZ.onUpdate += _ => TeleportToCoords();

        fireExitsDropdown = content.Find("FireExits").GetComponent<TMP_Dropdown>();
        fireExitsDropdown.onValueChanged.AddListener(_ => TeleportTo(fireExits[fireExitsDropdown.value]));

        ImpButton.Bind("Buttons/Interactive", content, OnInteractive);
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
        Imperium.ImpPositionIndicator.Activate(PlayerManager.TeleportTo);
    }

    private void TeleportTo(Vector3? anchor)
    {
        if (anchor == null) return;
        PlayerManager.TeleportTo(anchor.Value);
        CloseUI();
    }

    private void TeleportToCoords()
    {
        PlayerManager.TeleportTo(new Vector3(
            coordinateX.Value,
            coordinateY.Value,
            coordinateZ.Value
        ));
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
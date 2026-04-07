#region

using System;
using System.Collections;
using System.Collections.Generic;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Teleport.Widgets;

public class Teleportation : ImpWidget
{
    private Button tpMainEntrance;
    private Button tpShip;
    private Button tpApparatus;

    private Transform fireExitContainer;
    private GameObject fireExitTemplate;
    private TMP_Text fireExitsPlaceholder;
    private readonly List<Button> fireExits = [];

    private ImpBinding<float> coordinateX;
    private ImpBinding<float> coordinateY;
    private ImpBinding<float> coordinateZ;

    protected override void InitWidget()
    {
        tpMainEntrance = ImpButton.Bind(
            "Presets/MainEntrance", transform,
            () => TeleportTo(Imperium.PlayerManager.MainEntranceTPAnchor.Value),
            theme
        );
        tpShip = ImpButton.Bind(
            "Presets/Ship", transform,
            () => TeleportTo(Imperium.PlayerManager.ShipTPAnchor.Value),
            theme
        );
        tpApparatus = ImpButton.Bind(
            "Presets/Apparatus", transform,
            () => TeleportTo(Imperium.PlayerManager.ApparatusTPAnchor.Value),
            theme
        );
        ImpButton.Bind(
            "Presets/Freecam", transform,
            () => TeleportTo(Imperium.Freecam.transform.position),
            theme
        );

        // We need to set the teleport function as sync callback as the game might teleport the player to different
        // coordinates due to OOB restrictions. That way, the input field would be out of sync with the actual position,
        // so we have to re-set the coords without invoking another teleport that would lead to a stack overflow.
        coordinateX = new ImpBinding<float>(onUpdateSecondary: _ => TeleportToCoords());
        coordinateY = new ImpBinding<float>(onUpdateSecondary: _ => TeleportToCoords());
        coordinateZ = new ImpBinding<float>(onUpdateSecondary: _ => TeleportToCoords());

        ImpInput.Bind("Coords/CoordsX", transform, coordinateX, theme);
        ImpInput.Bind("Coords/CoordsY", transform, coordinateY, theme);
        ImpInput.Bind("Coords/CoordsZ", transform, coordinateZ, theme);

        ImpButton.Bind("Buttons/Interactive", transform, OnInteractive, theme);


        Imperium.InputBindings.BaseMap.Teleport.performed += OnInteractiveTeleport;

        Imperium.IsSceneLoaded.onTrigger += OnOpen;

        InitFireExits();
    }

    protected override void OnOpen()
    {
        tpShip.interactable = Imperium.PlayerManager.ShipTPAnchor.Value != null;
        tpMainEntrance.interactable = Imperium.PlayerManager.MainEntranceTPAnchor.Value != null
                                      && Imperium.IsSceneLoaded.Value;
        tpApparatus.interactable = Imperium.PlayerManager.ApparatusTPAnchor.Value != null
                                   && Imperium.IsSceneLoaded.Value;

        var position = Imperium.Player.transform.position;
        coordinateX.Set(MathF.Round(position.x, 2), invokeSecondary: false);
        coordinateY.Set(MathF.Round(position.y, 2), invokeSecondary: false);
        coordinateZ.Set(MathF.Round(position.z, 2), invokeSecondary: false);

        InitFireExits();
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

    private void TeleportToCoords() => StartCoroutine(teleportToCoordsAndUpdate());

    private IEnumerator teleportToCoordsAndUpdate()
    {
        Imperium.PlayerManager.TeleportLocalPlayer(new Vector3(
            coordinateX.Value,
            coordinateY.Value,
            coordinateZ.Value
        ));

        /*
         * Account for waiting for approximate round-trip to server before updating coords.
         *
         * This is for the case the game restricts the player to teleport to the desired coords (e.g. OOB)
         */
        yield return new WaitForSeconds(0.2f);

        coordinateX.Set(MathF.Round(Imperium.Player.transform.position.x, 2), invokeSecondary: false);
        coordinateY.Set(MathF.Round(Imperium.Player.transform.position.y, 2), invokeSecondary: false);
        coordinateZ.Set(MathF.Round(Imperium.Player.transform.position.z, 2), invokeSecondary: false);
    }

    private void InitFireExits()
    {
        fireExitContainer = transform.Find("FireExits/ScrollView/Viewport/Content");
        fireExitsPlaceholder = transform.Find("FireExits/Placeholder").GetComponent<TMP_Text>();
        fireExitTemplate = fireExitContainer.Find("Template").gameObject;
        fireExitTemplate.gameObject.SetActive(false);

        Imperium.IsSceneLoaded.onTrigger += SetFireExits;
        SetFireExits();
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdated)
    {
        ImpThemeManager.Style(
            themeUpdated,
            transform,
            new StyleOverride("FireExits", Variant.DARKER)
        );

        foreach (var fireExit in fireExits)
        {
            ImpThemeManager.Style(
                themeUpdated,
                fireExit.transform,
                new StyleOverride("", Variant.DARKER)
            );
        }
    }

    private void SetFireExits()
    {
        var objs = FindObjectsOfType<GameObject>();
        var fireExitCounter = 0;

        foreach (var fireExit in fireExits) Destroy(fireExit.gameObject);
        fireExits.Clear();

        foreach (var obj in objs)
        {
            if (obj.name != "FireExitDoor" || obj.transform.position.y > -100) continue;

            var newFireExit = Instantiate(fireExitTemplate, fireExitContainer);
            var newFireExitText = newFireExit.transform.Find("Text").GetComponent<TMP_Text>();
            newFireExitText.text = $"Exit #{fireExitCounter + 1}";

            var newFireExitButton = newFireExit.transform.GetComponent<Button>();
            newFireExitButton.onClick.AddListener(() => TeleportTo(obj.transform.position));
            ImpThemeManager.Style(
                theme.Value,
                newFireExitButton.transform,
                new StyleOverride("", Variant.DARKER)
            );

            newFireExit.gameObject.SetActive(true);

            fireExits.Add(newFireExit.GetComponent<Button>());
            fireExitCounter++;
        }

        if (fireExitCounter > 0)
        {
            fireExitsPlaceholder.gameObject.SetActive(false);
            fireExitContainer.gameObject.SetActive(true);
        }
        else
        {
            fireExitsPlaceholder.gameObject.SetActive(true);
            fireExitContainer.gameObject.SetActive(false);
        }
    }

    private static void OnInteractiveTeleport(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat) return;

        // Set origin of indicator to freecam if freecam is enabled
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;

        if (Imperium.ImpPositionIndicator.IsActive)
        {
            Imperium.ImpPositionIndicator.Deactivate();
        }
        else
        {
            Imperium.ImpPositionIndicator.Activate(Imperium.PlayerManager.TeleportLocalPlayer, origin);
        }
    }
}
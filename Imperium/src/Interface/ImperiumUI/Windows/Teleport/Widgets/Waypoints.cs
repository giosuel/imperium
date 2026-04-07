#region

using System.Collections.Generic;
using Imperium.Interface.Common;
using Imperium.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Teleport.Widgets;

internal class Waypoints : ImpWidget
{
    private RectTransform waypointListContainerRect;
    private TMP_InputField waypointNameInput;
    private GameObject waypointListTemplate;
    private GameObject waypointTitleTemplate;
    private GameObject waypointTemplate;
    private Transform waypointAddContainer;

    private readonly Dictionary<string, Transform> waypointListMap = new();
    private readonly Dictionary<string, Transform> waypointTitleMap = new();

    private Transform waypointContainer;
    private Transform waypointSettings;

    protected override void InitWidget()
    {
        waypointSettings = transform.Find("WaypointSettings");
        waypointContainer = transform.Find("WaypointList/ScrollView/Viewport/Content");

        waypointListContainerRect = waypointContainer.GetComponent<RectTransform>();
        waypointAddContainer = waypointContainer.Find("Add");

        waypointNameInput = waypointAddContainer.Find("Name").GetComponent<TMP_InputField>();
        waypointNameInput.onSubmit.AddListener(OnWaypointAdd);

        waypointTemplate = waypointContainer.Find("TemplateList/Item").gameObject;
        waypointListTemplate = waypointContainer.Find("TemplateList").gameObject;
        waypointTitleTemplate = waypointContainer.Find("TemplateTitle").gameObject;

        waypointTemplate.SetActive(false);
        waypointListTemplate.SetActive(false);
        waypointTitleTemplate.SetActive(false);

        ImpToggle.Bind(
            "EnableBeacons",
            waypointSettings,
            Imperium.Settings.Waypoint.EnableBeacons,
            theme: theme
        );

        ImpToggle.Bind(
            "EnableOverlay",
            waypointSettings,
            Imperium.Settings.Waypoint.EnableOverlay,
            theme: theme
        );

        ImpButton.Bind(
            "Add/Submit",
            waypointContainer,
            () => OnWaypointAdd(waypointNameInput.text),
            theme: theme
        );
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("", Variant.DARKER),
            new StyleOverride("WaypointList", Variant.DARKER),
            new StyleOverride("ScrollView/Scrollbar", Variant.DARKEST),
            new StyleOverride("ScrollView/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );

        ImpThemeManager.Style(
            themeUpdate,
            waypointContainer,
            new StyleOverride("Add/Name", Variant.LIGHTER),
            new StyleOverride("TemplateTitle", Variant.DARKER)
        );

        foreach (var waypointTitle in waypointTitleMap.Values)
        {
            ImpThemeManager.Style(
                themeUpdate,
                waypointTitle,
                new StyleOverride("", Variant.DARKER)
            );
        }
    }

    private (Transform, Transform) CreateLocationList(string locationName)
    {
        var waypointTitle = Instantiate(waypointTitleTemplate, waypointContainer).transform;
        waypointTitle.gameObject.SetActive(true);
        waypointTitle.transform.Find("Text").GetComponent<TMP_Text>().text = locationName;
        waypointTitleMap[locationName] = waypointTitle;

        var waypointList = Instantiate(waypointListTemplate, waypointContainer).transform;
        waypointList.gameObject.SetActive(true);
        waypointListMap[locationName] = waypointList;

        waypointList.SetAsFirstSibling();
        waypointTitle.SetAsFirstSibling();

        ImpButton.CreateCollapse("Arrow", waypointTitle, waypointList);

        return (waypointTitle, waypointList);
    }

    private void OnWaypointAdd(string waypointName)
    {
        if (string.IsNullOrEmpty(waypointName)) return;

        var currentMoonName = Imperium.RoundManager.currentLevel.PlanetName;

        Transform waypointList;
        Transform waypointTitle;

        if (!Imperium.WaypointManager.GetLocationWaypoints(currentMoonName, out var locationWaypointMap))
        {
            (waypointTitle, waypointList) = CreateLocationList(currentMoonName);
        }
        else
        {
            waypointList = waypointListMap[currentMoonName];
            waypointTitle = waypointTitleMap[currentMoonName];
        }

        var waypointEntry = Instantiate(waypointTemplate, waypointList);
        waypointEntry.SetActive(true);

        var waypoint = Imperium.WaypointManager.CreateWaypoint(
            waypointName, currentMoonName,
            Imperium.Player.transform.position,
            Imperium.PlayerManager.PlayerInCruiser.Value,
            DeleteWaypointEntry
        );
        locationWaypointMap.Add(waypoint);

        waypointEntry.transform.Find("Name/Name").GetComponent<TMP_Text>().text = waypoint.Name;
        waypointEntry.transform.Find("Name/Cruiser").gameObject.SetActive(waypoint.IsCruiser);

        // Disable teleportation for non-cruiser waypoints when player is in cruiser
        var teleportInteractiveBinding = !waypoint.IsCruiser ? Imperium.PlayerManager.PlayerInCruiser : null;

        ImpToggle.Bind(
            "Active",
            waypointEntry.transform,
            waypoint.IsShown,
            theme: theme
        );

        ImpButton.Bind(
            "Teleport",
            waypointEntry.transform,
            () => Imperium.PlayerManager.TeleportLocalPlayer(waypoint.Position),
            theme: theme,
            isIconButton: true,
            interactableInvert: true,
            interactableBindings: teleportInteractiveBinding
        );

        ImpButton.Bind(
            "Remove",
            waypointEntry.transform,
            () =>
            {
                DeleteWaypointEntry();
                Imperium.WaypointManager.DeleteWaypoint(waypoint);
            }
        );

        waypointNameInput.text = "";

        return;

        // Removes the waypoint's entry in the list and removes the location list if it was the last one
        void DeleteWaypointEntry()
        {
            Destroy(waypointEntry);

            if (locationWaypointMap.Count == 0)
            {
                Imperium.WaypointManager.DeleteLocation(currentMoonName);

                Destroy(waypointList.gameObject);
                Destroy(waypointTitle.gameObject);
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    // Although this used in Update(), it is only called when a key is pressed
    public void Update()
    {
        // Added for updating waypoint list, nothing else worked and this does not seem to affect performance much
        LayoutRebuilder.ForceRebuildLayoutImmediate(waypointListContainerRect);
    }
}
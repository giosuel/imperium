#region

using System.Collections.Generic;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.TeleportUI.Windows;

internal class WaypointWindow : BaseWindow
{
    private RectTransform waypointListContainerRect;
    private TMP_InputField waypointNameInput;
    private GameObject waypointListTemplate;
    private GameObject waypointTitleTemplate;
    private GameObject waypointTemplate;
    private Transform waypointAddContainer;

    private readonly Dictionary<string, Transform> waypointListMap = new();
    private readonly Dictionary<string, Transform> waypointTitleMap = new();

    private readonly Dictionary<string, Dictionary<string, Vector3>> waypointCoordinatesMap = new();

    protected override void RegisterWindow()
    {
        waypointListContainerRect = content.GetComponent<RectTransform>();
        waypointAddContainer = content.Find("Add");
        waypointNameInput = waypointAddContainer.Find("Name").GetComponent<TMP_InputField>();

        waypointNameInput.onSubmit.AddListener(OnWaypointAdd);

        waypointTemplate = content.Find("TemplateList/Item").gameObject;
        waypointListTemplate = content.Find("TemplateList").gameObject;
        waypointTitleTemplate = content.Find("TemplateTitle").gameObject;

        waypointTemplate.SetActive(false);
        waypointListTemplate.SetActive(false);
        waypointTitleTemplate.SetActive(false);
    }

    private void OnWaypointAdd(string waypointName)
    {
        if (string.IsNullOrEmpty(waypointName)) return;

        var currentMoonName = Imperium.RoundManager.currentLevel.PlanetName;

        waypointCoordinatesMap.TryGetValue(currentMoonName, out var moonWaypointMap);
        Transform waypointList;
        Transform waypointTitle;

        if (moonWaypointMap == null)
        {
            moonWaypointMap = new Dictionary<string, Vector3>();
            waypointCoordinatesMap[currentMoonName] = moonWaypointMap;

            // Create title, set title and init expand button
            waypointTitle = Instantiate(waypointTitleTemplate, content).transform;
            waypointTitle.gameObject.SetActive(true);
            waypointTitle.transform.Find("Text").GetComponent<TMP_Text>().text = currentMoonName;
            waypointTitleMap[currentMoonName] = waypointTitle;

            waypointList = Instantiate(waypointListTemplate, content).transform;
            waypointList.gameObject.SetActive(true);
            waypointListMap[currentMoonName] = waypointList;

            waypointList.SetAsFirstSibling();
            waypointTitle.SetAsFirstSibling();

            ImpButton.CreateCollapse("Arrow", waypointTitle, waypointList);
        }
        else
        {
            // Waypoint name already exists
            if (moonWaypointMap.ContainsKey(waypointName)) return;

            waypointList = waypointListMap[currentMoonName];
            waypointTitle = waypointTitleMap[currentMoonName];
        }

        var newWaypoint = Instantiate(waypointTemplate, waypointList);
        newWaypoint.SetActive(true);

        moonWaypointMap[waypointName] = Imperium.Player.transform.position;

        newWaypoint.transform.Find("Name").GetComponent<TMP_Text>().text = waypointName;
        newWaypoint.transform.Find("Teleport").GetComponent<Button>().onClick.AddListener(
            () => PlayerManager.TeleportTo(moonWaypointMap[waypointName]));
        newWaypoint.transform.Find("Remove").GetComponent<Button>().onClick.AddListener(() =>
        {
            Destroy(newWaypoint);
            moonWaypointMap.Remove(waypointName);

            // Remove moon if was last waypoint
            if (moonWaypointMap.Count == 0)
            {
                Destroy(waypointList.gameObject);
                Destroy(waypointTitle.gameObject);
            }
        });

        waypointNameInput.text = "";
    }

    // ReSharper disable Unity.PerformanceAnalysis
    // Although this used in Update(), it is only called when a key is pressed
    public void Update()
    {
        // Added for updating waypoint list, nothing else worked and this does not seem to affect performance much
        LayoutRebuilder.ForceRebuildLayoutImmediate(waypointListContainerRect);
    }
}
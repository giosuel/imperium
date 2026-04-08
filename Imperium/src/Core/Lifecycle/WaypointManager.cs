#region

using System;
using System.Collections.Generic;
using HarmonyLib;
using Imperium.Core.Scripts;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Lifecycle;

public class WaypointManager : ImpLifecycleObject
{
    private readonly Dictionary<string, HashSet<Waypoint>> locationWaypointMap = [];
    private readonly Dictionary<Waypoint, Action> deleteHandlers = [];

    private Waypoint currentHoverWaypoint;

    protected override void Init()
    {
        Imperium.InputBindings.BaseMap.TeleportWaypoint.performed += OnWaypointTeleport;
        Imperium.InputBindings.BaseMap.DeleteWaypoint.performed += OnWaypointDelete;
    }

    private void OnDestroy()
    {
        Imperium.InputBindings.BaseMap.TeleportWaypoint.performed -= OnWaypointTeleport;
        Imperium.InputBindings.BaseMap.DeleteWaypoint.performed -= OnWaypointDelete;
    }

    protected override void OnSceneLoad()
    {
    }

    protected override void OnSceneUnload()
    {
    }

    /// <summary>
    ///     Gets the list of waypoints for a certain location.
    ///     Returns true if the list has already existed.
    /// </summary>
    internal bool GetLocationWaypoints(string locationName, out HashSet<Waypoint> waypoints)
    {
        if (locationWaypointMap.TryGetValue(locationName, out waypoints)) return true;

        waypoints = [];
        locationWaypointMap.Add(locationName, waypoints);
        return false;
    }

    internal void DeleteLocation(string locationName)
    {
        locationWaypointMap[locationName].Do(DeleteWaypoint);
        locationWaypointMap.Remove(locationName);
    }

    internal Waypoint CreateWaypoint(
        string waypointName,
        string location,
        Vector3 position,
        bool isCruiser,
        Action onDelete
    )
    {
        var isShownBinding = new ImpBinding<bool>(true);
        var waypointObject = new GameObject("Imp_Waypoint").AddComponent<ImpWaypoint>();
        waypointObject.transform.SetParent(transform, true);

        var waypoint = new Waypoint
        {
            Name = waypointName,
            Location = location,
            Position = position,
            BeaconPosition = position + Imperium.Player.gameplayCamera.transform.forward * 1.5f,
            IsCruiser = isCruiser,
            IsShown = isShownBinding,
            WaypointObject = waypointObject,
        };

        waypointObject.Init(waypoint, () => currentHoverWaypoint = waypoint);
        locationWaypointMap[location].Add(waypoint);

        deleteHandlers.Add(waypoint, onDelete);

        return waypoint;
    }

    private void OnWaypointDelete(InputAction.CallbackContext _)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat ||
            currentHoverWaypoint == null) return;

        if (deleteHandlers.TryGetValue(currentHoverWaypoint, out var handler))
        {
            handler?.Invoke();
            deleteHandlers.Remove(currentHoverWaypoint);
        }

        DeleteWaypoint(currentHoverWaypoint);
    }

    private void OnWaypointTeleport(InputAction.CallbackContext _)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat ||
            currentHoverWaypoint == null) return;

        Imperium.PlayerManager.TeleportLocalPlayer(currentHoverWaypoint.Position);
    }

    internal void DeleteWaypoint(Waypoint waypoint)
    {
        Destroy(waypoint.WaypointObject.gameObject);
        locationWaypointMap[waypoint.Location].Remove(waypoint);
    }
}

internal record Waypoint
{
    internal string Name { get; init; }
    internal string Location { get; init; }
    internal Vector3 Position { get; init; }
    internal Vector3 BeaconPosition { get; init; }
    internal bool IsCruiser { get; init; }

    internal IBinding<bool> IsShown { get; init; }
    internal ImpWaypoint WaypointObject { get; init; }
}
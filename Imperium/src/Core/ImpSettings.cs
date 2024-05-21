#region

using System;
using System.Linq;
using System.Reflection;
using Imperium.Netcode;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Core;

/// <summary>
///     Contains all the bindings of the persistent settings of Imperium.
/// </summary>
public abstract class ImpSettings
{
    // Indication if settings are currently being loaded (to skip notifications and other things during loading)
    internal static bool IsLoading { get; private set; }

    internal abstract class Player
    {
        internal static readonly ImpConfig<bool> InfiniteSprint = new("Player", "InfiniteSprint", false);
        internal static readonly ImpConfig<bool> DisableLocking = new("Player", "DisableLocking", false);
        internal static readonly ImpConfig<bool> InfiniteBattery = new("Player", "InfiniteBattery", false);
        internal static readonly ImpConfig<bool> Invisibility = new("Player", "Invisibility", false);
        internal static readonly ImpConfig<bool> Muted = new("Player", "Muted", false);
        internal static readonly ImpConfig<bool> PickupOverwrite = new("Player", "PickupOverwrite", false);
        internal static readonly ImpConfig<bool> DisableOOB = new("Player", "DisableOOB", false);

        internal static readonly ImpConfig<float> CustomFieldOfView = new(
            "Player",
            "FieldOfView",
            ImpConstants.DefaultFOV
        );

        internal static readonly ImpConfig<bool> GodMode = new(
            "Player",
            "GodMode",
            false,
            value => Imperium.StartOfRound.allowLocalPlayerDeath = value
        );

        internal static readonly ImpConfig<float> MovementSpeed = new(
            "Player",
            "MovementSpeed",
            ImpConstants.DefaultMovementSpeed,
            value => Imperium.Player.movementSpeed = value
        );

        internal static readonly ImpConfig<float> JumpForce = new(
            "Player",
            "JumpForce",
            ImpConstants.DefaultJumpForce,
            value => Imperium.Player.jumpForce = value
        );

        internal static readonly ImpConfig<float> NightVision = new(
            "Player",
            "NightVision",
            0
        );
    }

    internal abstract class Shotgun
    {
        internal static readonly ImpConfig<bool> InfiniteAmmo = new("Items.Shotgun", "InfiniteAmmo", false);
        internal static readonly ImpConfig<bool> FullAuto = new("Items.Shotgun", "FullAuto", false);
    }

    internal abstract class Shovel
    {
        internal static readonly ImpConfig<bool> Speedy = new("Items.Shovel", "Speedy", false);
    }

    internal abstract class Time
    {
        internal static readonly ImpConfig<bool> RealtimeClock = new("Time", "RealtimeClock", true);
        internal static readonly ImpConfig<bool> PermanentClock = new("Time", "PermanentClock", true);
    }

    internal abstract class Game
    {
        internal static readonly ImpConfig<bool> OverwriteShipDoors = new(
            "Game.Ship",
            "OverwriteShipDoors",
            false
        );

        internal static readonly ImpConfig<bool> MuteShipSpeaker = new(
            "Game.Ship",
            "MuteShipSpeaker",
            true,
            value => Imperium.StartOfRound.speakerAudioSource.mute = value
        );

        internal static readonly ImpConfig<bool> PreventShipLeave = new(
            "Game.Ship",
            "PreventShipLeave",
            false,
            syncUpdate: value => ImpNetTime.Instance.SetShipLeaveAutomaticallyServerRpc(value)
        );

        internal static readonly ImpConfig<bool> UnlockShop = new(
            "Game.Terminal",
            "UnlockShop",
            false,
            onUpdate: value =>
            {
                // Reset selection when locking shop
                if (!value) Imperium.Terminal.RotateShipDecorSelection();
            }
        );
    }

    internal abstract class Visualizations
    {
        /// <summary>
        ///     Visualizer preferences
        /// </summary>
        internal static readonly ImpConfig<bool> SmoothAnimations = new(
            "Preferences.Visualizers",
            "SmoothAnimations",
            true
        );

        internal static readonly ImpConfig<bool> SSAlwaysOnTop = new(
            "Preferences.Visualizers.ScreenSpace",
            "AlwaysOnTop",
            true
        );

        internal static readonly ImpConfig<bool> SSAutoScale = new(
            "Preferences.Visualizers.ScreenSpace",
            "AutoScale",
            true
        );

        internal static readonly ImpConfig<bool> SSHideInactive = new(
            "Preferences.Visualizers.ScreenSpace",
            "HideInactive",
            false
        );

        internal static readonly ImpConfig<float> SSOverlayScale = new(
            "Preferences.Visualizers.ScreenSpace",
            "OverlayScale",
            1
        );

        /// <summary>
        ///     Colliders
        /// </summary>
        internal static readonly ImpConfig<bool> Employees = new(
            "Colliders",
            "Employees",
            false,
            value => Imperium.Visualization.Collider("Player", IdentifierType.TAG)(value)
        );

        internal static readonly ImpConfig<bool> Entities = new(
            "Colliders",
            "Entities",
            true,
            value => Imperium.Visualization.Collider("Enemies", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> MapHazards = new(
            "Colliders",
            "MapHazards",
            false,
            value => Imperium.Visualization.Collider("MapHazards", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> Props = new(
            "Colliders",
            "Props",
            false,
            value => Imperium.Visualization.Collider("PhysicsProp", IdentifierType.TAG)(value)
        );

        internal static readonly ImpConfig<bool> Foliage = new(
            "Colliders",
            "Foliage",
            false,
            value => Imperium.Visualization.Collider("EnemySpawn", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> InteractTriggers = new(
            "Colliders",
            "InteractTriggers",
            false,
            value => Imperium.Visualization.Collider("InteractTrigger", IdentifierType.TAG)(value)
        );

        internal static readonly ImpConfig<bool> TileBorders = new(
            "Colliders",
            "TileBorders",
            false,
            value => Imperium.Visualization.Collider("Ignore Raycast", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> Room = new(
            "Colliders",
            "Room",
            false,
            value => Imperium.Visualization.Collider("Room", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> Colliders = new(
            "Colliders",
            "Colliders",
            false,
            value => Imperium.Visualization.Collider("Colliders", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> Triggers = new(
            "Colliders",
            "Triggers",
            false,
            value => Imperium.Visualization.Collider("Triggers", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> PhysicsObject = new(
            "Colliders",
            "PhysicsObject",
            false,
            value => Imperium.Visualization.Collider("PhysicsObject", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> NavigationSurface = new(
            "Colliders",
            "NavigationSurface",
            false,
            value => Imperium.Visualization.Collider("NavigationSurface", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> RoomLight = new(
            "Colliders",
            "RoomLight",
            false,
            value => Imperium.Visualization.Collider("RoomLight", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> Anomaly = new(
            "Colliders",
            "Anomaly",
            false,
            value => Imperium.Visualization.Collider("Anomaly", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> Railing = new(
            "Colliders",
            "Railing",
            false,
            value => Imperium.Visualization.Collider("Railing", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> PlacementBlocker = new(
            "Colliders",
            "PlacementBlocker",
            false,
            value => Imperium.Visualization.Collider("PlacementBlocker", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> Terrain = new(
            "Colliders",
            "Terrain",
            false,
            value => Imperium.Visualization.Collider("Terrain", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> PlaceableShipObjects = new(
            "Colliders",
            "PlaceableShipObjects",
            false,
            value => Imperium.Visualization.Collider("PlaceableShipObjects", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> MiscLevelGeometry = new(
            "Colliders",
            "MiscLevelGeometry",
            false,
            value => Imperium.Visualization.Collider("MiscLevelGeometry", IdentifierType.LAYER)(value)
        );

        internal static readonly ImpConfig<bool> ScanNode = new(
            "Colliders",
            "ScanNode",
            false,
            value => Imperium.Visualization.Collider("ScanNode", IdentifierType.LAYER)(value)
        );

        /// <summary>
        ///     Overlays
        /// </summary>
        internal static readonly ImpConfig<bool> Vents = new(
            "Overlays",
            "Vents",
            false,
            value => Imperium.Visualization.Point(
                "EnemySpawn",
                IdentifierType.TAG,
                material: ImpAssets.XrayMaterial
            )(value)
        );

        internal static readonly ImpConfig<bool> AINodesIndoor = new(
            "Overlays",
            "AINodesIndoor",
            false,
            value => Imperium.Visualization.Point(
                "AINode",
                IdentifierType.TAG,
                size: 0.5f,
                material: ImpAssets.FresnelWhiteMaterial
            )(value)
        );

        internal static readonly ImpConfig<bool> AINodesOutdoor = new(
            "Overlays",
            "AINodesOutdoor",
            false,
            value => Imperium.Visualization.Point(
                "OutsideAINode",
                IdentifierType.TAG,
                size: 0.8f,
                material: ImpAssets.FresnelWhiteMaterial
            )(value)
        );

        internal static readonly ImpConfig<bool> SpawnDenialPoints = new(
            "Overlays",
            "SpawnDenialPoints",
            false,
            value => Imperium.Visualization.Point(
                "SpawnDenialPoint",
                IdentifierType.TAG,
                size: 16,
                material: ImpAssets.FresnelRedMaterial
            )(value)
        );

        internal static readonly ImpConfig<bool> BeeSpawns = new(
            "Overlays",
            "BeeSpawns",
            false,
            value => Imperium.Visualization.Point(
                "OutsideAINode",
                IdentifierType.TAG,
                size: 14,
                material: ImpAssets.FresnelYellowMaterial
            )(value)
        );

        /// <summary>
        ///     Gizmos
        /// </summary>
        internal static readonly ImpConfig<bool> SpawnIndicators = new(
            "Gizmos",
            "SpawnIndicators",
            false
        );

        internal static readonly ImpConfig<bool> VentTimers = new(
            "Gizmos",
            "VentTimers",
            false
        );

        internal static readonly ImpConfig<bool> NoiseIndicators = new(
            "Gizmos",
            "NoiseIndicators",
            false
        );

        internal static readonly ImpConfig<bool> ScrapSpawns = new(
            "Gizmos",
            "ScrapSpawns",
            false
        );

        internal static readonly ImpConfig<bool> ShotgunIndicators = new(
            "Gizmos",
            "ShotgunIndicators",
            false
        );

        internal static readonly ImpConfig<bool> ShovelIndicators = new(
            "Gizmos",
            "ShovelIndicators",
            false
        );

        internal static readonly ImpConfig<bool> KnifeIndicators = new(
            "Gizmos",
            "KnifeIndicators",
            false
        );

        internal static readonly ImpConfig<bool> LandmineIndicators = new(
            "Gizmos",
            "LandmineIndicators",
            false
        );

        internal static readonly ImpConfig<bool> SpikeTrapIndicators = new(
            "Gizmos",
            "SpikeTrapIndicators",
            false
        );
    }

    internal abstract class Rendering
    {
        internal static readonly ImpConfig<float> ResolutionMultiplier = new(
            "Rendering.General",
            "ResolutionMultiplier",
            1,
            _ => PlayerManager.UpdateCameras(),
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> GlobalVolume = new(
            "Rendering.Volumes",
            "GlobalVolume",
            true,
            value => Imperium.ObjectManager.ToggleObject("VolumeMain", value)
        );

        internal static readonly ImpConfig<bool> VolumetricFog = new(
            "Rendering.Volumes",
            "VolumetricFog",
            true,
            value => Imperium.ObjectManager.ToggleObject("Local Volumetric Fog", value)
        );

        internal static readonly ImpConfig<bool> GroundFog = new(
            "Rendering.Volumes",
            "GroundFog",
            true,
            value => Imperium.ObjectManager.ToggleObject("GroundFog", value)
        );

        internal static readonly ImpConfig<bool> StormyVolume = new(
            "Rendering.Volumes",
            "StormyVolume",
            true,
            value => Imperium.ObjectManager.ToggleObject("StormVolume", value)
        );

        internal static readonly ImpConfig<bool> SkyboxVolume = new(
            "Rendering.Volumes",
            "SkyboxVolume",
            true,
            value => Imperium.ObjectManager.ToggleObject("Sky and Fog Global Volume", value)
        );

        internal static readonly ImpConfig<bool> Steamleaks = new(
            "Rendering.Volumes",
            "Steamleaks",
            true,
            value => Imperium.ObjectManager?.CurrentLevelSteamleaks.Value?.ToList()
                .ForEach(leak => leak.SetActive(value))
        );

        internal static readonly ImpConfig<bool> SpaceSun = new(
            "Rendering.Lighting",
            "SpaceSun",
            true,
            value =>
            {
                // Disable space sun when on moon
                if (Imperium.IsSceneLoaded.Value) value = false;

                Imperium.ObjectManager.ToggleObject("Sun", value);
                var sun = Imperium.ObjectManager.FindObject("Sun");
                if (sun) sun.GetComponent<Light>().enabled = value;
            }
        );

        internal static readonly ImpConfig<bool> Sunlight = new(
            "Rendering.Lighting",
            "Sunlight",
            true,
            value =>
            {
                Imperium.ObjectManager.ToggleObject("SunTexture", value);
                Imperium.ObjectManager.ToggleObject("SunWithShadows", value);
            }
        );

        internal static readonly ImpConfig<bool> IndirectLighting = new(
            "Rendering.Lighting",
            "IndirectLighting",
            true,
            value => Imperium.ObjectManager.ToggleObject("Indirect", value)
        );

        internal static readonly ImpConfig<bool> DecalLayers = new(
            "Rendering.FrameSettings",
            "DecalLayers",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> SSGI = new(
            "Rendering.FrameSettings",
            "SSGI",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> RayTracing = new(
            "Rendering.FrameSettings",
            "RayTracing",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> VolumetricClouds = new(
            "Rendering.FrameSettings",
            "VolumetricClouds",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> SSS = new(
            "Rendering.FrameSettings",
            "SubsurfaceScattering",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> VolumeReprojection = new(
            "Rendering.FrameSettings",
            "VolumeReprojection",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> TransparentPrepass = new(
            "Rendering.FrameSettings",
            "TransparentPrepass",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> TransparentPostpass = new(
            "Rendering.FrameSettings",
            "TransparentPostpass",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal static readonly ImpConfig<bool> CelShading = new(
            "Rendering.PostProcessing",
            "CelShading",
            true,
            value => Imperium.ObjectManager.ToggleObject("CustomPass", value)
        );

        internal static readonly ImpConfig<bool> StarsOverlay = new(
            "Rendering.Overlays",
            "StarsOverlay",
            false,
            value =>
            {
                // Disable space sun when on moon
                if (Imperium.IsSceneLoaded.Value) value = false;

                Imperium.ObjectManager.ToggleObject("StarsSphere", value);
            }
        );

        internal static readonly ImpConfig<bool> HUDVisor = new(
            "Rendering.Overlays",
            "HUDVisor",
            true,
            value => Imperium.ObjectManager.ToggleObject("PlayerHUDHelmetModel", value)
        );

        internal static readonly ImpConfig<bool> PlayerHUD = new(
            "Rendering.Overlays",
            "PlayerHUD",
            true,
            value => Imperium.HUDManager.HideHUD(!value)
        );

        internal static readonly ImpConfig<bool> FearFilter = new(
            "Rendering.Filters",
            "Fear",
            true,
            value => Imperium.ObjectManager.ToggleObject("InsanityFilter", value)
        );

        internal static readonly ImpConfig<bool> FlashbangFilter = new(
            "Rendering.Filters",
            "Flashbang",
            true,
            value => Imperium.ObjectManager.ToggleObject("FlashbangFilter", value)
        );

        internal static readonly ImpConfig<bool> UnderwaterFilter = new(
            "Rendering.Filters",
            "Underwater",
            true,
            value => Imperium.ObjectManager.ToggleObject("UnderwaterFilter", value)
        );

        internal static readonly ImpConfig<bool> DrunknessFilter = new(
            "Rendering.Filters",
            "Drunkness",
            true,
            value => Imperium.ObjectManager.ToggleObject("DrunknessFilter", value)
        );

        internal static readonly ImpConfig<bool> ScanSphere = new(
            "Rendering.Filters",
            "ScanSphere",
            true,
            value => Imperium.ObjectManager.ToggleObject("ScanSphere", value)
        );
    }

    internal abstract class Preferences
    {
        internal static readonly ImpConfig<bool> GeneralLogging = new("Preferences.General", "GeneralLogging", true);
        internal static readonly ImpConfig<bool> OracleLogging = new("Preferences.General", "OracleLogging", false);
        internal static readonly ImpConfig<bool> LeftHandedMode = new("Preferences.General", "LeftHandedMode", false);
        internal static readonly ImpConfig<bool> OptimizeLogs = new("Preferences.General", "OptimizeLogsToggle", true);
        internal static readonly ImpConfig<bool> CustomWelcome = new("Preferences.General", "CustomWelcome", true);

        internal static readonly ImpConfig<string> Theme = new(
            "Preferences.Appearance",
            "Theme",
            "Imperium"
        );

        internal static readonly ImpConfig<bool> AllowClients = new(
            "Preferences.Host",
            "AllowClients",
            true
        );

        internal static readonly ImpConfig<bool> UnityExplorerMouseFix = new(
            "Preferences.General",
            "UnityExplorerMouseFix",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsGodMode = new(
            "Preferences.Notifications",
            "GodMode",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsOracle = new(
            "Preferences.Notifications",
            "Oracle",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsSpawnReports = new(
            "Preferences.Notifications",
            "SpawnReports",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsConfirmation = new(
            "Preferences.Notifications",
            "Confirmation",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsEntities = new(
            "Preferences.Notifications",
            "Entities",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsSpawning = new(
            "Preferences.Notifications",
            "Spawning",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsAccessControl = new(
            "Preferences.Notifications",
            "AccessControl",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsServer = new(
            "Preferences.Notifications",
            "Server",
            true
        );

        internal static readonly ImpConfig<bool> NotificationsOther = new(
            "Preferences.Notifications",
            "Other",
            true
        );

        internal static readonly ImpConfig<bool> QuickloadSkipStart = new("Preferences.Quickload", "SkipStart", false);
        internal static readonly ImpConfig<bool> QuickloadSkipMenu = new("Preferences.Quickload", "SkipMenu", false);
        internal static readonly ImpConfig<bool> QuickloadOnQuit = new("Preferences.Quickload", "OnQuit", false);
        internal static readonly ImpConfig<bool> QuickloadCleanFile = new("Preferences.Quickload", "CleanFile", false);
        internal static readonly ImpConfig<int> QuickloadSaveNumber = new("Preferences.Quickload", "SaveFileNumber", 4);
    }

    internal abstract class Map
    {
        internal static readonly ImpConfig<bool> MinimapEnabled = new(
            "Preferences.Map",
            "Minimap",
            true
        );

        internal static readonly ImpConfig<bool> CompassEnabled = new(
            "Preferences.Map",
            "Compass",
            true
        );

        internal static readonly ImpConfig<bool> RotationLock = new(
            "Preferences.Map",
            "RotationLock",
            true
        );

        internal static readonly ImpConfig<bool> UnlockView = new(
            "Preferences.Map",
            "UnlockView",
            false
        );

        internal static readonly ImpConfig<int> CameraLayerMask = new(
            "Preferences.Map",
            "LayerMask",
            -272672930
        );

        internal static readonly ImpConfig<float> CameraZoom = new(
            "Preferences.Map",
            "Zoom",
            ImpConstants.DefaultMapCameraScale
        );

        internal static readonly ImpConfig<bool> AutoClipping = new(
            "Preferences.Map",
            "AutoClipping",
            true
        );

        internal static readonly ImpConfig<float> MinimapWidth = new(
            "Preferences.Minimap",
            "Width",
            1
        );

        internal static readonly ImpConfig<float> MinimapHeight = new(
            "Preferences.Minimap",
            "Height",
            1
        );

        internal static readonly ImpConfig<bool> MinimapInfoPanel = new(
            "Preferences.Minimap",
            "InfoPanel",
            true
        );
    }

    internal abstract class Freecam
    {
        internal static readonly ImpConfig<bool> LayerSelector = new(
            "Preferences.Freecam",
            "LayerSelector",
            true
        );

        internal static readonly ImpConfig<int> FreecamLayerMask = new(
            "Preferences.Freecam",
            "LayerMask",
            ~LayerMask.GetMask("HelmetVisor")
        );

        internal static readonly ImpConfig<float> FreecamMovementSpeed = new(
            "Preferences.Freecam",
            "MovementSpeed",
            20
        );

        internal static readonly ImpConfig<float> FreecamFieldOfView = new(
            "Preferences.Freecam",
            "FieldOfView",
            ImpConstants.DefaultFOV
        );
    }

    internal static void Load<T>()
    {
        IsLoading = true;
        typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .ToList()
            .ForEach(field => ((IRefreshable)field.GetValue(null)).Refresh());
        IsLoading = false;
    }

    private static void Reset<T>()
    {
        IsLoading = true;
        typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Static)
            .ToList()
            .ForEach(field => ((IResettable)field.GetValue(null)).Reset());
        IsLoading = false;
    }

    // Since the Imperium settings are defined in abstract, static classes for simplicity, in order to emulate
    // an object life-cycle, we have to re-initialize the static fields when re-launching Imperium.
    // We need to reload because ImpSetting holds ImpBindings that have registered callbacks that might end up
    // throwing a NullReferenceException if their host objects have been destroyed.
    private static void Reinstantiate<T>()
    {
        typeof(T).GetConstructor(
            BindingFlags.Static | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null
        )!.Invoke(null, null);
    }

    internal static void LoadAll()
    {
        Load<Player>();
        Load<Shotgun>();
        Load<Shovel>();
        Load<Time>();
        Load<Game>();
        Load<Visualizations>();
        Load<Rendering>();
        Load<Preferences>();
        Load<Map>();
        Load<Freecam>();
    }

    internal static void FactoryReset()
    {
        Reset<Player>();
        Reset<Shotgun>();
        Reset<Shovel>();
        Reset<Time>();
        Reset<Game>();
        Reset<Visualizations>();
        Reset<Rendering>();
        Reset<Preferences>();
        Reset<Map>();
        Reset<Freecam>();

        Imperium.Reload();
    }

    internal static void Reinstantiate()
    {
        Reinstantiate<Player>();
        Reinstantiate<Shotgun>();
        Reinstantiate<Shovel>();
        Reinstantiate<Time>();
        Reinstantiate<Game>();
        Reinstantiate<Map>();
        Reinstantiate<Visualizations>();
        Reinstantiate<Rendering>();
        Reinstantiate<Preferences>();
        Reinstantiate<Freecam>();
    }
}
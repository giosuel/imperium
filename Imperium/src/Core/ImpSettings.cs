#region

using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using Imperium.Core.Lifecycle;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Core;

/// <summary>
///     Contains all the bindings of the persistent settings of Imperium.
/// </summary>
public class ImpSettings(ConfigFile config)
{
    // Indication if settings are currently being loaded (to skip notifications and other things during loading)
    internal bool IsLoading { get; private set; }

    internal readonly PlayerSettings Player = new(config);
    internal readonly ShotgunSettings Shotgun = new(config);
    internal readonly ShovelSettings Shovel = new(config);
    internal readonly TimeSettings Time = new(config);
    internal readonly EventLogSettings EventLog = new(config);
    internal readonly ShipSettings Ship = new(config);
    internal readonly AnimationSkippingSettings AnimationSkipping = new(config);
    internal readonly VisualizationSettings Visualization = new(config);
    internal readonly RenderSettings Rendering = new(config);
    internal readonly MapSettings Map = new(config);
    internal readonly PreferenceSettings Preferences = new(config);
    internal readonly FreecamSettings Freecam = new(config);

    internal class PlayerSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> InfiniteSprint = new(config, "Player", "InfiniteSprint", false);
        internal readonly ImpConfig<bool> DisableLocking = new(config, "Player", "DisableLocking", false);
        internal readonly ImpConfig<bool> InfiniteBattery = new(config, "Player", "InfiniteBattery", false);
        internal readonly ImpConfig<bool> Invisibility = new(config, "Player", "Invisibility", false);
        internal readonly ImpConfig<bool> Untargetable = new(config, "Player", "Untargetable", false);
        internal readonly ImpConfig<bool> Muted = new(config, "Player", "Muted", false);
        internal readonly ImpConfig<bool> PickupOverwrite = new(config, "Player", "PickupOverwrite", false);
        internal readonly ImpConfig<bool> DisableOOB = new(config, "Player", "DisableOOB", false);

        internal readonly ImpConfig<bool> EnableFlying = new(
            config,
            "Player",
            "EnableFlying",
            false,
            onUpdate: value =>
            {
                if (!value) Imperium.PlayerManager.IsFlying.SetFalse();
            }
        );

        internal readonly ImpConfig<bool> FlyingNoClip = new(config, "Player", "FlyingNoClip", false);
        internal readonly ImpConfig<bool> Permadrunk = new(config, "Player", "Permadrunk", false);

        internal readonly ImpConfig<float> CustomFieldOfView = new(
            config,
            "Player",
            "FieldOfView",
            ImpConstants.DefaultFOV
        );

        internal readonly ImpConfig<bool> GodMode = new(
            config,
            "Player",
            "GodMode",
            false,
            value =>
            {
                // Restore health to full when turning on god mode
                if (value) PlayerManager.RestoreLocalPlayerHealth(Imperium.Player);
            }
        );

        internal readonly ImpConfig<float> MovementSpeed = new(
            config,
            "Player",
            "MovementSpeed",
            ImpConstants.DefaultMovementSpeed,
            value => Imperium.Player.movementSpeed = value
        );

        internal readonly ImpConfig<float> JumpForce = new(
            config,
            "Player",
            "JumpForce",
            ImpConstants.DefaultJumpForce,
            value => Imperium.Player.jumpForce = value
        );

        internal readonly ImpConfig<float> NightVision = new(
            config,
            "Player",
            "NightVision",
            0
        );

        [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<float> PushForce = new(
            config,
            "Player",
            "PushForce",
            ImpConstants.DefaultCarPushForceMultiplier
        );
    }

    internal class ShotgunSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> InfiniteAmmo = new(config, "Items.Shotgun", "InfiniteAmmo", false);
        internal readonly ImpConfig<bool> FullAuto = new(config, "Items.Shotgun", "FullAuto", false);
    }

    internal class ShovelSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> Speedy = new(config, "Items.Shovel", "Speedy", false);
    }

    internal class TimeSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> RealtimeClock = new(config, "Game.Time", "RealtimeClock", true);
        internal readonly ImpConfig<bool> PermanentClock = new(config, "Game.Time", "PermanentClock", true);
    }

    internal class EventLogSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> EntityLogs = new(config, "EventLog", "Entity", true);
        internal readonly ImpConfig<bool> PlayerLogs = new(config, "EventLog", "Player", true);
        internal readonly ImpConfig<bool> GameLogs = new(config, "EventLog", "Game", true);
        internal readonly ImpConfig<bool> CustomLogs = new(config, "EventLog", "Custom", true);
    }

    internal class ShipSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> OverwriteDoors = new(
            config,
            "Game.Ship",
            "OverwriteDoors",
            false
        );

        internal readonly ImpConfig<bool> MuteSpeaker = new(
            config,
            "Game.Ship",
            "MuteSpeaker",
            true,
            value => Imperium.StartOfRound.speakerAudioSource.mute = value
        );

        [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> DisableAbandoned = new(
            config,
            "Game.Ship",
            "DisableAbandoned",
            false
        );

        [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> PreventLeave = new(
            config,
            "Game.Ship",
            "PreventLeave",
            false
        );

        [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> InstantLanding = new(
            config,
            "Game.Ship",
            "InstantLanding",
            false
        );

        [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> InstantTakeoff = new(
            config,
            "Game.Ship",
            "InstantTakeoff",
            false
        );

        [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> UnlockShop = new(
            config,
            "Game.Terminal",
            "UnlockShop",
            false
        );
    }

    internal class AnimationSkippingSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> Scoreboard = new(
            config,
            "AnimationSkipping",
            "DisableAbandoned",
            false
        );

        internal readonly ImpConfig<bool> PlayerSpawn = new(
            config,
            "AnimationSkipping",
            "PlayerSpawn",
            false
        );

        internal readonly ImpConfig<bool> InteractHold = new(
            config,
            "AnimationSkipping",
            "InteractHold",
            false
        );

        internal readonly ImpConfig<bool> Interact = new(
            config,
            "AnimationSkipping",
            "Interact",
            false
        );
    }

    internal class VisualizationSettings(ConfigFile config) : SettingBase(config)
    {
        /// <summary>
        ///     Visualization preferences
        /// </summary>
        internal readonly ImpConfig<bool> SmoothAnimations = new(
            config,
            "Visualization.Visualizers",
            "SmoothAnimations",
            true
        );

        internal readonly ImpConfig<bool> RealtimeUpdates = new(
            config,
            "Visualization.Visualizers",
            "RealtimeUpdates",
            false
        );

        internal readonly ImpConfig<bool> SSAlwaysOnTop = new(
            config,
            "Visualization.Insights",
            "AlwaysOnTop",
            true
        );

        internal readonly ImpConfig<bool> SSAutoScale = new(
            config,
            "Visualization.Insights",
            "AutoScale",
            true
        );

        internal readonly ImpConfig<bool> SSHideInactive = new(
            config,
            "Visualization.Insights",
            "HideInactive",
            false
        );

        internal readonly ImpConfig<float> SSOverlayScale = new(
            config,
            "Visualization.Insights",
            "OverlayScale",
            1
        );

        /// <summary>
        ///     Colliders
        /// </summary>
        internal readonly ImpConfig<bool> Employees = new(
            config,
            "Visualization.Colliders",
            "Employees",
            false,
            value => Imperium.Visualization.Collider(value, "Player", IdentifierType.TAG)
        );

        internal readonly ImpConfig<bool> Entities = new(
            config,
            "Visualization.Colliders",
            "Entities",
            false,
            value => Imperium.Visualization.Collider(value, "Enemies", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> MapHazards = new(
            config,
            "Visualization.Colliders",
            "MapHazards",
            false,
            value => Imperium.Visualization.Collider(value, "MapHazards", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> Props = new(
            config,
            "Visualization.Colliders",
            "Props",
            false,
            value => Imperium.Visualization.Collider(value, "PhysicsProp", IdentifierType.TAG)
        );

        internal readonly ImpConfig<bool> Foliage = new(
            config,
            "Visualization.Colliders",
            "Foliage",
            false,
            value => Imperium.Visualization.Collider(value, "EnemySpawn", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> InteractTriggers = new(
            config,
            "Visualization.Colliders",
            "InteractTriggers",
            false,
            value => Imperium.Visualization.Collider(value, "InteractTrigger", IdentifierType.TAG)
        );

        internal readonly ImpConfig<bool> VainShrouds = new(
            config,
            "Visualization.Colliders",
            "VainShrouds",
            false,
            value => Imperium.Visualization.Collider(value, "MoldSpore", IdentifierType.TAG)
        );

        internal readonly ImpConfig<bool> MoldAttractionPoints = new(
            config,
            "Visualization.Colliders",
            "MoldAttractionPoints",
            false,
            value => Imperium.Visualization.Point(value, "MoldAttractionPoint", IdentifierType.TAG)
        );

        internal readonly ImpConfig<bool> TileBorders = new(
            config,
            "Visualization.Colliders",
            "TileBorders",
            false,
            value => Imperium.Visualization.Collider(value, "Ignore Raycast", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> Room = new(
            config,
            "Visualization.Colliders",
            "Room",
            false,
            value => Imperium.Visualization.Collider(value, "Room", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> Colliders = new(
            config,
            "Visualization.Colliders",
            "Visualization.Colliders",
            false,
            value => Imperium.Visualization.Collider(
                value, "Visualization.Colliders", IdentifierType.LAYER, material: ImpAssets.TriggerMaterial
            )
        );

        internal readonly ImpConfig<bool> Triggers = new(
            config,
            "Visualization.Colliders",
            "Triggers",
            false,
            value => Imperium.Visualization.Collider(
                value, "Triggers", IdentifierType.LAYER, material: ImpAssets.TriggerMaterial
            )
        );

        internal readonly ImpConfig<bool> PhysicsObject = new(
            config,
            "Visualization.Colliders",
            "PhysicsObject",
            false,
            value => Imperium.Visualization.Collider(value, "PhysicsObject", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> RoomLight = new(
            config,
            "Visualization.Colliders",
            "RoomLight",
            false,
            value => Imperium.Visualization.Collider(value, "RoomLight", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> Anomaly = new(
            config,
            "Visualization.Colliders",
            "Anomaly",
            false,
            value => Imperium.Visualization.Collider(value, "Anomaly", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> Railing = new(
            config,
            "Visualization.Colliders",
            "Railing",
            false,
            value => Imperium.Visualization.Collider(value, "Railing", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> PlacementBlocker = new(
            config,
            "Visualization.Colliders",
            "PlacementBlocker",
            false,
            value => Imperium.Visualization.Collider(value, "PlacementBlocker", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> Terrain = new(
            config,
            "Visualization.Colliders",
            "Terrain",
            false,
            value => Imperium.Visualization.Collider(value, "Terrain", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> PlaceableShipObjects = new(
            config,
            "Visualization.Colliders",
            "PlaceableShipObjects",
            false,
            value => Imperium.Visualization.Collider(value, "PlaceableShipObjects", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> MiscLevelGeometry = new(
            config,
            "Visualization.Colliders",
            "MiscLevelGeometry",
            false,
            value => Imperium.Visualization.Collider(value, "MiscLevelGeometry", IdentifierType.LAYER)
        );

        internal readonly ImpConfig<bool> ScanNode = new(
            config,
            "Visualization.Colliders",
            "ScanNode",
            false,
            value => Imperium.Visualization.Collider(value, "ScanNode", IdentifierType.LAYER)
        );

        /// <summary>
        ///     Overlays
        /// </summary>
        internal readonly ImpConfig<bool> Vents = new(
            config,
            "Visualization.Overlays",
            "Vents",
            false,
            value => Imperium.Visualization.Point(
                value,
                "EnemySpawn",
                IdentifierType.TAG,
                material: ImpAssets.XRay
            )
        );

        internal readonly ImpConfig<bool> AINodesIndoor = new(
            config,
            "Visualization.Overlays",
            "AINodesIndoor",
            false,
            value => Imperium.Visualization.Point(
                value,
                "AINode",
                IdentifierType.TAG,
                size: 0.5f,
                material: ImpAssets.FresnelWhite
            )
        );

        internal readonly ImpConfig<bool> AINodesOutdoor = new(
            config,
            "Visualization.Overlays",
            "AINodesOutdoor",
            false,
            value => Imperium.Visualization.Point(
                value,
                "OutsideAINode",
                IdentifierType.TAG,
                size: 0.8f,
                material: ImpAssets.FresnelWhite
            )
        );

        internal readonly ImpConfig<bool> SpawnDenialPoints = new(
            config,
            "Visualization.Overlays",
            "SpawnDenialPoints",
            false,
            value => Imperium.Visualization.Point(
                value,
                "SpawnDenialPoint",
                IdentifierType.TAG,
                size: 16,
                material: ImpAssets.FresnelRed
            )
        );

        internal readonly ImpConfig<bool> BeeSpawns = new(
            config,
            "Visualization.Overlays",
            "BeeSpawns",
            false,
            value => Imperium.Visualization.Point(
                value,
                "OutsideAINode",
                IdentifierType.TAG,
                size: 20f,
                material: ImpAssets.FresnelYellow
            )
        );

        internal readonly ImpConfig<bool> OutsideEntitySpawns = new(
            config,
            "Visualization.Overlays",
            "OutsideEntitySpawns",
            false,
            value => Imperium.Visualization.Point(
                value,
                "OutsideAINode",
                IdentifierType.TAG,
                size: 10f,
                material: ImpAssets.FresnelGreen
            )
        );

        internal readonly ImpConfig<bool> NavMeshSurfaces = new(
            config,
            "Visualization.Overlays",
            "NavMeshSurfaces",
            false
        );

        /// <summary>
        ///     Gizmos
        /// </summary>
        internal readonly ImpConfig<bool> SpawnIndicators = new(
            config,
            "Visualization.Gizmos",
            "SpawnIndicators",
            false
        );

        internal readonly ImpConfig<bool> VentTimers = new(
            config,
            "Visualization.Gizmos",
            "VentTimers",
            false
        );

        internal readonly ImpConfig<bool> NoiseIndicators = new(
            config,
            "Visualization.Gizmos",
            "NoiseIndicators",
            false
        );

        internal readonly ImpConfig<bool> ScrapSpawns = new(
            config,
            "Visualization.Gizmos",
            "ScrapSpawns",
            false
        );

        internal readonly ImpConfig<bool> HazardSpawns = new(
            config,
            "Visualization.Gizmos",
            "HazardSpawns",
            false
        );

        internal readonly ImpConfig<bool> ShotgunIndicators = new(
            config,
            "Visualization.Gizmos",
            "ShotgunIndicators",
            false
        );

        internal readonly ImpConfig<bool> ShovelIndicators = new(
            config,
            "Visualization.Gizmos",
            "ShovelIndicators",
            false
        );

        internal readonly ImpConfig<bool> KnifeIndicators = new(
            config,
            "Visualization.Gizmos",
            "KnifeIndicators",
            false
        );

        internal readonly ImpConfig<bool> LandmineIndicators = new(
            config,
            "Visualization.Gizmos",
            "LandmineIndicators",
            false
        );

        internal readonly ImpConfig<bool> SpikeTrapIndicators = new(
            config,
            "Visualization.Gizmos",
            "SpikeTrapIndicators",
            false
        );
    }

    internal class RenderSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<float> ResolutionMultiplier = new(
            config,
            "Rendering.General",
            "ResolutionMultiplier",
            1,
            _ => PlayerManager.UpdateCameras(),
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> GlobalVolume = new(
            config,
            "Rendering.Volumes",
            "GlobalVolume",
            true,
            value => Imperium.ObjectManager.ToggleObject("VolumeMain", value)
        );

        internal readonly ImpConfig<bool> VolumetricFog = new(
            config,
            "Rendering.Volumes",
            "VolumetricFog",
            true,
            value => Imperium.ObjectManager.ToggleObject("Local Volumetric Fog", value)
        );

        internal readonly ImpConfig<bool> GroundFog = new(
            config,
            "Rendering.Volumes",
            "GroundFog",
            true,
            value => Imperium.ObjectManager.ToggleObject("GroundFog", value)
        );

        internal readonly ImpConfig<bool> StormyVolume = new(
            config,
            "Rendering.Volumes",
            "StormyVolume",
            true,
            value => Imperium.ObjectManager.ToggleObject("StormVolume", value)
        );

        internal readonly ImpConfig<bool> SkyboxVolume = new(
            config,
            "Rendering.Volumes",
            "SkyboxVolume",
            true,
            value => Imperium.ObjectManager.ToggleObject("Sky and Fog Global Volume", value)
        );

        internal readonly ImpConfig<bool> SpaceSun = new(
            config,
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

        internal readonly ImpConfig<bool> Sunlight = new(
            config,
            "Rendering.Lighting",
            "Sunlight",
            true,
            value =>
            {
                Imperium.ObjectManager.ToggleObject("SunTexture", value);
                Imperium.ObjectManager.ToggleObject("SunWithShadows", value);
            }
        );

        internal readonly ImpConfig<bool> IndirectLighting = new(
            config,
            "Rendering.Lighting",
            "IndirectLighting",
            true,
            value => Imperium.ObjectManager.ToggleObject("Indirect", value)
        );

        internal readonly ImpConfig<bool> DecalLayers = new(
            config,
            "Rendering.FrameSettings",
            "DecalLayers",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> SSGI = new(
            config,
            "Rendering.FrameSettings",
            "SSGI",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> RayTracing = new(
            config,
            "Rendering.FrameSettings",
            "RayTracing",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> VolumetricClouds = new(
            config,
            "Rendering.FrameSettings",
            "VolumetricClouds",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> SSS = new(
            config,
            "Rendering.FrameSettings",
            "SubsurfaceScattering",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> VolumeReprojection = new(
            config,
            "Rendering.FrameSettings",
            "VolumeReprojection",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> TransparentPrepass = new(
            config,
            "Rendering.FrameSettings",
            "TransparentPrepass",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> TransparentPostpass = new(
            config,
            "Rendering.FrameSettings",
            "TransparentPostpass",
            true,
            PlayerManager.UpdateCameras,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> CelShading = new(
            config,
            "Rendering.PostProcessing",
            "CelShading",
            true,
            value => Imperium.ObjectManager.ToggleObject("CustomPass", value)
        );

        internal readonly ImpConfig<bool> StarsOverlay = new(
            config,
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

        internal readonly ImpConfig<bool> HUDVisor = new(
            config,
            "Rendering.Overlays",
            "HUDVisor",
            true,
            value => Imperium.ObjectManager.ToggleObject("PlayerHUDHelmetModel", value)
        );

        internal readonly ImpConfig<bool> PlayerHUD = new(
            config,
            "Rendering.Overlays",
            "PlayerHUD",
            true,
            value => Imperium.HUDManager.HideHUD(!value)
        );

        internal readonly ImpConfig<bool> FearFilter = new(
            config,
            "Rendering.Filters",
            "Fear",
            true,
            value => Imperium.ObjectManager.ToggleObject("InsanityFilter", value)
        );

        internal readonly ImpConfig<bool> FlashbangFilter = new(
            config,
            "Rendering.Filters",
            "Flashbang",
            true,
            value => Imperium.ObjectManager.ToggleObject("FlashbangFilter", value)
        );

        internal readonly ImpConfig<bool> UnderwaterFilter = new(
            config,
            "Rendering.Filters",
            "Underwater",
            true,
            value => Imperium.ObjectManager.ToggleObject("UnderwaterFilter", value)
        );

        internal readonly ImpConfig<bool> DrunknessFilter = new(
            config,
            "Rendering.Filters",
            "Drunkness",
            true,
            value => Imperium.ObjectManager.ToggleObject("DrunknessFilter", value)
        );

        internal readonly ImpConfig<bool> ScanSphere = new(
            config,
            "Rendering.Filters",
            "ScanSphere",
            true,
            value => Imperium.ObjectManager.ToggleObject("ScanSphere", value)
        );
    }

    internal class PreferenceSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> EnableImperium = new(
            config,
            "Preferences.General",
            "EnableImperium",
            true,
            allowWhenDisabled: true
        );

        internal readonly ImpConfig<bool> GeneralLogging = new(config, "Preferences.General", "GeneralLogging", true);
        internal readonly ImpConfig<bool> OracleLogging = new(config, "Preferences.General", "OracleLogging", false);
        internal readonly ImpConfig<bool> LeftHandedMode = new(config, "Preferences.General", "LeftHandedMode", false);
        internal readonly ImpConfig<bool> OptimizeLogs = new(config, "Preferences.General", "OptimizeLogsToggle", true);
        internal readonly ImpConfig<bool> CustomWelcome = new(config, "Preferences.General", "CustomWelcome", true);
        internal readonly ImpConfig<bool> ShowTooltips = new(config, "Preferences.Tooltips", "CustomWelcome", true);

        internal readonly ImpConfig<string> ImperiumWindowLayout = new(
            config,
            "Preferences.Appearance",
            "WindowLayout",
            ""
        );

        internal readonly ImpConfig<string> Theme = new(
            config,
            "Preferences.Appearance",
            "Theme",
            "Imperium"
        );

        internal readonly ImpConfig<bool> AllowClients = new(
            config,
            "Preferences.Host",
            "AllowClients",
            true,
            ignoreRefresh: true
        );

        internal readonly ImpConfig<bool> UnityExplorerMouseFix = new(
            config,
            "Preferences.General",
            "UnityExplorerMouseFix",
            true
        );

        internal readonly ImpConfig<bool> NotificationsGodMode = new(
            config,
            "Preferences.Notifications",
            "GodMode",
            true
        );

        internal readonly ImpConfig<bool> NotificationsOracle = new(
            config,
            "Preferences.Notifications",
            "Oracle",
            true
        );

        internal readonly ImpConfig<bool> NotificationsSpawnReports = new(
            config,
            "Preferences.Notifications",
            "SpawnReports",
            true
        );

        internal readonly ImpConfig<bool> NotificationsConfirmation = new(
            config,
            "Preferences.Notifications",
            "Confirmation",
            true
        );

        internal readonly ImpConfig<bool> NotificationsEntities = new(
            config,
            "Preferences.Notifications",
            "Entities",
            true
        );

        internal readonly ImpConfig<bool> NotificationsSpawning = new(
            config,
            "Preferences.Notifications",
            "Spawning",
            true
        );

        internal readonly ImpConfig<bool> NotificationsAccessControl = new(
            config,
            "Preferences.Notifications",
            "AccessControl",
            true
        );

        internal readonly ImpConfig<bool> NotificationsServer = new(
            config,
            "Preferences.Notifications",
            "Server",
            true
        );

        internal readonly ImpConfig<bool> NotificationsOther = new(
            config,
            "Preferences.Notifications",
            "Other",
            true
        );

        internal readonly ImpConfig<bool> QuickloadSkipStart = new(
            config,
            "Preferences.Quickload",
            "SkipStart",
            false,
            allowWhenDisabled: true
        );

        internal readonly ImpConfig<bool> QuickloadSkipMenu = new(config, "Preferences.Quickload", "SkipMenu", false);
        internal readonly ImpConfig<bool> QuickloadOnQuit = new(config, "Preferences.Quickload", "OnQuit", false);
        internal readonly ImpConfig<bool> QuickloadCleanFile = new(config, "Preferences.Quickload", "CleanFile", false);
        internal readonly ImpConfig<int> QuickloadSaveNumber = new(config, "Preferences.Quickload", "SaveFileNumber", 4);
    }

    internal class MapSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> MinimapEnabled = new(
            config,
            "Preferences.Map",
            "Minimap",
            true
        );

        internal readonly ImpConfig<bool> CompassEnabled = new(
            config,
            "Preferences.Map",
            "Compass",
            true
        );

        internal readonly ImpConfig<bool> RotationLock = new(
            config,
            "Preferences.Map",
            "RotationLock",
            true
        );

        internal readonly ImpConfig<bool> UnlockView = new(
            config,
            "Preferences.Map",
            "UnlockView",
            false
        );

        internal readonly ImpConfig<int> CameraLayerMask = new(
            config,
            "Preferences.Map",
            "LayerMask",
            -272672930
        );

        internal readonly ImpConfig<float> CameraZoom = new(
            config,
            "Preferences.Map",
            "Zoom",
            ImpConstants.DefaultMapCameraScale
        );

        internal readonly ImpConfig<bool> AutoClipping = new(
            config,
            "Preferences.Map",
            "AutoClipping",
            true
        );

        internal readonly ImpConfig<float> MinimapScale = new(
            config,
            "Preferences.Minimap",
            "Scale",
            1
        );

        internal readonly ImpConfig<bool> MinimapInfoPanel = new(
            config,
            "Preferences.Minimap",
            "InfoPanel",
            true
        );

        internal readonly ImpConfig<bool> MinimapLocationPanel = new(
            config,
            "Preferences.Minimap",
            "LocationPanel",
            true
        );
    }

    internal class FreecamSettings(ConfigFile config) : SettingBase(config)
    {
        internal readonly ImpConfig<bool> LayerSelector = new(
            config,
            "Preferences.Freecam",
            "LayerSelector",
            true
        );

        internal readonly ImpConfig<int> FreecamLayerMask = new(
            config,
            "Preferences.Freecam",
            "LayerMask",
            ~LayerMask.GetMask("HelmetVisor")
        );

        internal readonly ImpConfig<float> FreecamMovementSpeed = new(
            config,
            "Preferences.Freecam",
            "MovementSpeed",
            20
        );

        internal readonly ImpConfig<float> FreecamFieldOfView = new(
            config,
            "Preferences.Freecam",
            "FieldOfView",
            ImpConstants.DefaultFOV
        );
    }

    private void Load<T>(T settings)
    {
        IsLoading = true;
        typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .ToList()
            .ForEach(field => (field.GetValue(settings) as IRefreshable)?.Refresh());
        IsLoading = false;
    }

    private void Reset<T>(T settings)
    {
        IsLoading = true;
        typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .ToList()
            .ForEach(field => (field.GetValue(settings) as IResettable)?.Reset());
        IsLoading = false;
    }

    internal void LoadAll()
    {
        Load(Player);
        Load(Shotgun);
        Load(Shovel);
        Load(Time);
        Load(Ship);
        Load(EventLog);
        Load(AnimationSkipping);
        Load(Visualization);
        Load(Rendering);
        Load(Preferences);
        Load(Map);
        Load(Freecam);
    }

    internal void FactoryReset()
    {
        Reset(Player);
        Reset(Shotgun);
        Reset(Shovel);
        Reset(Time);
        Reset(Ship);
        Reset(EventLog);
        Reset(AnimationSkipping);
        Reset(Visualization);
        Reset(Rendering);
        Reset(Preferences);
        Reset(Map);
        Reset(Freecam);

        Imperium.Reload();
    }
}
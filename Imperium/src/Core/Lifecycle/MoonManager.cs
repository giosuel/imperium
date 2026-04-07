#region

using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Integration;
using Imperium.Netcode;
using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Core.Lifecycle;

internal class MoonManager : ImpLifecycleObject
{
    public int ScrapAmount;
    public int ChallengeScrapAmount;
    public bool FogEnabledThisRound;

    private readonly ImpEvent WeatherEvent = new();

    private readonly ImpNetMessage<ChangeWeatherRequest> changeWeatherMessage = new(
        "ChangeWeather", Imperium.Networking
    );

    private readonly ImpNetMessage<ToggleDoorLocksRequest> toggleDoorLocksMessage = new(
        "ToggleDoorLocks", Imperium.Networking
    );

    private readonly ImpNetEvent flickerLightsEvent = new(
        "FlickerLights", Imperium.Networking
    );

    private readonly ImpNetEvent cleanFloorEvent = new(
        "CleanFloor", Imperium.Networking
    );

    protected override void Init()
    {
        changeWeatherMessage.OnServerReceive += OnWeatherChangeServer;
        changeWeatherMessage.OnClientRecive += OnWeatherChangeClient;
        flickerLightsEvent.OnClientRecive += OnFlickerLightsEvent;
        cleanFloorEvent.OnClientRecive += OnCleanFloorEvent;
        toggleDoorLocksMessage.OnClientRecive += OnToggleDoorsLocks;
    }

    internal void ChangeWeather(ChangeWeatherRequest request) => changeWeatherMessage.DispatchToServer(request);

    internal readonly ImpNetworkBinding<bool> IndoorSpawningPaused = new(
        "IndoorSpawningPaused",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<bool> OutdoorSpawningPaused = new(
        "OutdoorSpawningPaused",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<bool> WeedSpawningPaused = new(
        "WeedSpawningPaused",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<bool> DaytimeSpawningPaused = new(
        "DaytimeSpawningPaused",
        Imperium.Networking
    );

    internal readonly ImpNetworkBinding<bool> ForceIndoorFog = new(
        "ForceIndoorFog",
        Imperium.Networking,
        onUpdateClient: value =>
        {
            RoundManager.Instance.indoorFog.gameObject.SetActive(value || Imperium.MoonManager.FogEnabledThisRound);
        }
    );

    internal readonly ImpNetworkBinding<bool> TimeIsPaused = new(
        "TimeIsPaused",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Time.TimeIsPaused,
        onUpdateClient: isPaused =>
        {
            Imperium.TimeOfDay.globalTimeSpeedMultiplier = isPaused ? 0 : ImpConstants.DefaultTimeSpeed;
        }
    );

    internal readonly ImpNetworkBinding<float> TimeSpeed = new(
        "TimeSpeed",
        Imperium.Networking,
        ImpConstants.DefaultTimeSpeed
    );

    internal readonly ImpNetworkBinding<float> MaxIndoorPower = new(
        "MaxIndoorPower",
        Imperium.Networking,
        Imperium.RoundManager.currentLevel.maxEnemyPowerCount,
        onUpdateClient: value =>
        {
            if (Imperium.IsSceneLoaded.Value) Imperium.RoundManager.currentLevel.maxEnemyPowerCount = (int)value;
            Imperium.RoundManager.currentMaxInsidePower = value;
        }
    );

    internal readonly ImpNetworkBinding<float> MaxOutdoorPower = new(
        "MaxOutdoorPower",
        Imperium.Networking,
        Imperium.RoundManager.currentLevel.maxOutsideEnemyPowerCount,
        onUpdateClient: value =>
        {
            if (Imperium.IsSceneLoaded.Value) Imperium.RoundManager.currentLevel.maxOutsideEnemyPowerCount = (int)value;
            Imperium.RoundManager.currentMaxOutsidePower = value;
        });

    internal readonly ImpNetworkBinding<int> MaxDaytimePower = new(
        "MaxDaytimePower",
        Imperium.Networking,
        Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount,
        onUpdateClient: value => Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount = value
    );

    internal readonly ImpNetworkBinding<float> IndoorDeviation = new(
        "IndoorDeviation",
        Imperium.Networking,
        Imperium.RoundManager.currentLevel.spawnProbabilityRange,
        onUpdateClient: value => Imperium.RoundManager.currentLevel.spawnProbabilityRange = value
    );

    internal readonly ImpNetworkBinding<float> DaytimeDeviation = new(
        "DaytimeDeviation",
        Imperium.Networking,
        Imperium.RoundManager.currentLevel.daytimeEnemiesProbabilityRange,
        onUpdateClient: value => Imperium.RoundManager.currentLevel.daytimeEnemiesProbabilityRange = value
    );

    internal readonly ImpNetworkBinding<int> MinIndoorSpawns = new(
        "MinIndoorSpawns",
        Imperium.Networking,
        Imperium.RoundManager.minEnemiesToSpawn,
        onUpdateClient: value => Imperium.RoundManager.minEnemiesToSpawn = value
    );

    internal readonly ImpNetworkBinding<int> MinOutdoorSpawns = new(
        "MinOutdoorSpawns",
        Imperium.Networking,
        Imperium.RoundManager.minOutsideEnemiesToSpawn,
        onUpdateClient: value => Imperium.RoundManager.minOutsideEnemiesToSpawn = value
    );

    internal readonly ImpNetworkBinding<float> WeatherVariable1 = new(
        "WeatherVariable1",
        Imperium.Networking,
        Imperium.TimeOfDay.currentWeatherVariable,
        onUpdateClient: value =>
        {
            if (Imperium.RoundManager.currentLevel.currentWeather == LevelWeatherType.Eclipsed)
            {
                Imperium.RoundManager.minEnemiesToSpawn = (int)value;
                Imperium.RoundManager.minOutsideEnemiesToSpawn = (int)value;
            }

            Imperium.TimeOfDay.currentWeatherVariable = value;
        }
    );

    internal readonly ImpNetworkBinding<float> WeatherVariable2 = new(
        "WeatherVariable2",
        Imperium.Networking,
        Imperium.TimeOfDay.currentWeatherVariable,
        onUpdateClient: value => Imperium.TimeOfDay.currentWeatherVariable = value
    );

    protected override void OnSceneLoad()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        MaxIndoorPower.Sync(Imperium.RoundManager.currentMaxInsidePower);
        MaxOutdoorPower.Sync(Imperium.RoundManager.currentMaxOutsidePower);
        MaxDaytimePower.Sync(Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount);

        IndoorDeviation.Sync(Imperium.RoundManager.currentLevel.spawnProbabilityRange);
        DaytimeDeviation.Sync(Imperium.RoundManager.currentLevel.daytimeEnemiesProbabilityRange);

        MinIndoorSpawns.Sync(Imperium.RoundManager.minEnemiesToSpawn);
        MinOutdoorSpawns.Sync(Imperium.RoundManager.minOutsideEnemiesToSpawn);

        WeatherVariable1.Sync(Imperium.TimeOfDay.currentWeatherVariable);
        WeatherVariable2.Sync(Imperium.TimeOfDay.currentWeatherVariable2);
    }

    [ImpAttributes.RemoteMethod]
    internal static void ToggleDoors(bool isOpen)
    {
        Imperium.ObjectManager.CurrentLevelDoors.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(door => ToggleDoor(door, isOpen));
    }

    [ImpAttributes.RemoteMethod]
    internal static void ToggleDoor(DoorLock door, bool isOpen)
    {
        if (door.isDoorOpened != isOpen)
        {
            door.gameObject.GetComponent<AnimatedObjectTrigger>().TriggerAnimation(Imperium.Player);
        }
    }

    [ImpAttributes.RemoteMethod]
    internal void ToggleDoorLocks(bool isLocked)
    {
        toggleDoorLocksMessage.DispatchToClients(new ToggleDoorLocksRequest
        {
            Lock = isLocked
        });
    }

    [ImpAttributes.RemoteMethod]
    internal static void ToggleSecurityDoors(bool isOn)
    {
        Imperium.ObjectManager.CurrentLevelSecurityDoors.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(door => ToggleSecurityDoor(door, isOn, true));
    }

    [ImpAttributes.RemoteMethod]
    internal static void ToggleBreakers(bool isOn)
    {
        Imperium.ObjectManager.CurrentLevelBreakerBoxes.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(box => ToggleBreaker(box, isOn, true));
    }

    [ImpAttributes.RemoteMethod]
    internal static void ToggleSecurityDoor(TerminalAccessibleObject door, bool isOn, bool toggleDisabled)
    {
        door.SetDoorOpenServerRpc(isOn);

        if (toggleDisabled)
        {
            var doorNetObj = door.GetComponent<NetworkObject>();

            if (isOn)
            {
                Imperium.ObjectManager.DisabledObjects.Value.Remove(doorNetObj);
            }
            else
            {
                Imperium.ObjectManager.DisabledObjects.Value.Add(doorNetObj);
            }

            Imperium.ObjectManager.DisabledObjects.Set(Imperium.ObjectManager.DisabledObjects.Value);
        }
    }

    [ImpAttributes.RemoteMethod]
    internal static void ToggleBreaker(BreakerBox box, bool isOn, bool toggleDisabled)
    {
        foreach (var breakerSwitch in box.breakerSwitches)
        {
            var animation = breakerSwitch.gameObject.GetComponent<AnimatedObjectTrigger>();
            if (animation.boolValue != isOn)
            {
                animation.boolValue = isOn;
                animation.setInitialState = isOn;
                // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
                breakerSwitch.SetBool("turnedLeft", isOn);
                box.SwitchBreaker(isOn);
            }
        }

        if (toggleDisabled)
        {
            var boxNetObj = box.GetComponent<NetworkObject>();

            if (isOn)
            {
                Imperium.ObjectManager.DisabledObjects.Value.Remove(boxNetObj);
            }
            else
            {
                Imperium.ObjectManager.DisabledObjects.Value.Add(boxNetObj);
            }

            Imperium.ObjectManager.DisabledObjects.Set(Imperium.ObjectManager.DisabledObjects.Value);
        }
    }

    [ImpAttributes.RemoteMethod]
    internal void FlickerLights() => flickerLightsEvent.DispatchToClients();

    [ImpAttributes.RemoteMethod]
    internal void CleanFloor() => cleanFloorEvent.DispatchToClients();

    [ImpAttributes.HostOnly]
    private void OnWeatherChangeServer(ChangeWeatherRequest request, ulong clientId)
    {
        changeWeatherMessage.DispatchToClients(request);

        if (WeatherRegistryIntegration.IsEnabled)
        {
            WeatherRegistryIntegration.ChangeWeather(Imperium.StartOfRound.levels[request.LevelIndex], request.WeatherType);
        }
    }

    [ImpAttributes.LocalMethod]
    private static void OnFlickerLightsEvent()
    {
        Imperium.RoundManager.FlickerLights(true);
    }

    [ImpAttributes.LocalMethod]
    private static void OnCleanFloorEvent()
    {
        // copied from StartOfRound.ResetPooledObjects
        if (Imperium.StartOfRound.slimeDecals != null)
        {
            for (var i = Imperium.StartOfRound.slimeDecals.Count - 1; i >= 0; i--)
            {
                if (Imperium.StartOfRound.slimeDecals[i] != null)
                {
                    Destroy(Imperium.StartOfRound.slimeDecals[i]);
                }

                Imperium.StartOfRound.slimeDecals.RemoveAt(i);
            }

            Imperium.StartOfRound.slimeFadingInDecalIndex = 0;
        }

        // clean up screen filters. See HUDManager.DisplaySpitOnHelmet and HUDManager.SetScreenFilters
        Imperium.HUDManager.helmetGoop.SetActive(value: false);
        // let vanilla deal with spitOnCameraAlpha
    }

    [ImpAttributes.LocalMethod]
    private void OnWeatherChangeClient(ChangeWeatherRequest request)
    {
        Imperium.StartOfRound.levels[request.LevelIndex].currentWeather = request.WeatherType;

        var planetName = Imperium.StartOfRound.levels[request.LevelIndex].PlanetName;
        var weatherName = request.WeatherType.ToString();
        Imperium.IO.Send(
            $"Successfully changed the weather on {planetName} to {weatherName}",
            type: NotificationType.Confirmation
        );

        Imperium.StartOfRound.SetMapScreenInfoToCurrentLevel();
        RefreshWeatherEffects();
        WeatherEvent.Trigger();
    }

    [ImpAttributes.LocalMethod]
    private static void OnToggleDoorsLocks(ToggleDoorLocksRequest locksRequest)
    {
        Imperium.ObjectManager.CurrentLevelDoors.Value
            .Where(obj => obj)
            .ToList()
            .ForEach(door =>
            {
                if (locksRequest.Lock)
                {
                    door.LockDoor();
                }
                else
                {
                    door.UnlockDoor();
                }
            });
    }

    [ImpAttributes.LocalMethod]
    private static void RefreshWeatherEffects()
    {
        if (!Imperium.IsSceneLoaded.Value) return;

        Imperium.RoundManager.SetToCurrentLevelWeather();
        for (var i = 0; i < Imperium.TimeOfDay.effects.Length; i++)
        {
            var weatherEffect = Imperium.TimeOfDay.effects[i];
            var isEnabled = (int)Imperium.StartOfRound.currentLevel.currentWeather == i;
            weatherEffect.effectEnabled = isEnabled;
            if (weatherEffect.effectPermanentObject)
            {
                weatherEffect.effectPermanentObject.SetActive(value: isEnabled);
            }

            if (weatherEffect.effectObject)
            {
                weatherEffect.effectObject.SetActive(value: isEnabled);
            }

            if (Imperium.TimeOfDay.sunAnimator)
            {
                if (isEnabled && !string.IsNullOrEmpty(weatherEffect.sunAnimatorBool))
                {
                    Imperium.TimeOfDay.sunAnimator.SetBool(weatherEffect.sunAnimatorBool, value: true);
                }
                else
                {
                    Imperium.TimeOfDay.sunAnimator.Rebind();
                    Imperium.TimeOfDay.sunAnimator.Update(0);
                }
            }
        }

        // This prevents the player from permanently getting stuck in the underwater effect when turning
        // off flooded weather while being underwater
        if (Imperium.StartOfRound.currentLevel.currentWeather != LevelWeatherType.Flooded)
        {
            Imperium.Player.isUnderwater = false;
            Imperium.Player.sourcesCausingSinking = Mathf.Clamp(Imperium.Player.sourcesCausingSinking - 1, 0, 100);
            Imperium.Player.isMovementHindered = Mathf.Clamp(Imperium.Player.isMovementHindered - 1, 0, 100);
            Imperium.Player.hinderedMultiplier = 1;
        }
    }
}
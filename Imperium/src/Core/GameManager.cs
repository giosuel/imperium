#region

using Imperium.Netcode;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;

#endregion

namespace Imperium.Core;

internal class GameManager : ImpLifecycleObject
{
    internal GameManager(ImpBinaryBinding sceneLoaded, ImpBinding<int> playersConnected)
        : base(sceneLoaded, playersConnected)
    {
        TimeIsPaused = new ImpBinding<bool>(
            false,
            syncUpdate: value =>
                ImpNetTime.Instance.UpdateTimeServerRpc(TimeSpeed?.Value ?? ImpConstants.DefaultTimeSpeed, value)
        );
        TimeSpeed = new ImpBinding<float>(
            ImpConstants.DefaultTimeSpeed,
            syncUpdate: value =>
                ImpNetTime.Instance.UpdateTimeServerRpc(value, TimeIsPaused?.Value ?? false)
        );
    }

    internal readonly ImpBinding<int> CustomSeed = new(-1);

    internal readonly ImpBinding<bool> IndoorSpawningPaused = new(
        false,
        value =>
        {
            ImpOutput.Send(
                value ? "Indoor spawning has been paused!" : "Indoor spawning has been resumed!",
                type: NotificationType.Confirmation
            );
        }
    );

    internal readonly ImpBinding<bool> OutdoorSpawningPaused = new(
        false,
        value =>
        {
            ImpOutput.Send(
                value ? "Outdoor spawning has been paused!" : "Outdoor spawning has been resumed!",
                type: NotificationType.Confirmation
            );
        }
    );

    internal readonly ImpBinding<bool> DaytimeSpawningPaused = new(
        false,
        value =>
        {
            ImpOutput.Send(
                value ? "Daytime spawning has been paused!" : "Daytime spawning has been resumed!",
                type: NotificationType.Confirmation
            );
        }
    );

    internal readonly ImpBinding<bool> TimeIsPaused;
    internal readonly ImpBinding<float> TimeSpeed;

    internal readonly ImpBinding<float> MaxIndoorPower = new(
        Imperium.RoundManager.currentMaxInsidePower,
        value => Imperium.RoundManager.currentMaxInsidePower = value,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetMaxIndoorPowerServerRpc(value);
            ImpOutput.Send($"Indoor Power set to {value}!", type: NotificationType.Confirmation);
        });

    internal readonly ImpBinding<float> MaxOutdoorPower = new(
        Imperium.RoundManager.currentMaxOutsidePower,
        value => Imperium.RoundManager.currentMaxOutsidePower = value,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetMaxOutdoorPowerServerRpc(value);
            ImpOutput.Send($"Outdoor Power set to {value}!", type: NotificationType.Confirmation);
        });

    internal readonly ImpBinding<int> MaxDaytimePower = new(
        Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount,
        value => Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount = value,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetMaxDaytimePowerServerRpc(value);
            ImpOutput.Send($"Daytime Power set to {value}!", type: NotificationType.Confirmation);
        });

    internal readonly ImpBinding<float> IndoorDeviation = new(
        Imperium.RoundManager.currentLevel.spawnProbabilityRange,
        value => Imperium.RoundManager.currentLevel.spawnProbabilityRange = value,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetIndoorDeviationServerRpc(value);
            ImpOutput.Send($"Indoor deviation set to {value}!", type: NotificationType.Confirmation);
        });

    internal readonly ImpBinding<float> DaytimeDeviation = new(
        Imperium.RoundManager.currentLevel.daytimeEnemiesProbabilityRange,
        value => Imperium.RoundManager.currentLevel.daytimeEnemiesProbabilityRange = value,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetDaytimeDeviationServerRpc(value);
            ImpOutput.Send($"Daytime deviation set to {value}!", type: NotificationType.Confirmation);
        });

    internal readonly ImpBinding<int> MinIndoorSpawns = new(
        Imperium.RoundManager.minEnemiesToSpawn,
        value => Imperium.RoundManager.minEnemiesToSpawn = value,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetMinIndoorEntitiesServerRpc(value);
            ImpOutput.Send($"Daytime deviation set to {value}!", type: NotificationType.Confirmation);
        });

    internal readonly ImpBinding<int> MinOutdoorSpawns = new(
        Imperium.RoundManager.minOutsideEnemiesToSpawn,
        value => Imperium.RoundManager.minOutsideEnemiesToSpawn = value,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetMinOutdoorEntitiesServerRpc(value);
            ImpOutput.Send(
                $"Minimum outdoor spawn has been set to {value}!",
                type: NotificationType.Confirmation
            );
        });

    internal readonly ImpBinding<float> WeatherVariable1 = new(
        Imperium.TimeOfDay.currentWeatherVariable,
        value =>
        {
            if (Imperium.RoundManager.currentLevel.currentWeather == LevelWeatherType.Eclipsed)
            {
                Imperium.RoundManager.minEnemiesToSpawn = (int)value;
                Imperium.RoundManager.minOutsideEnemiesToSpawn = (int)value;
            }

            Imperium.TimeOfDay.currentWeatherVariable = value;
        },
        ignoreRefresh: true,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetWeatherVariable1ServerRpc(value);
            ImpOutput.Send($"Weather variable 1 set to {value}!", type: NotificationType.Confirmation);
        });

    internal readonly ImpBinding<float> WeatherVariable2 = new(
        Imperium.TimeOfDay.currentWeatherVariable2,
        value => Imperium.TimeOfDay.currentWeatherVariable2 = value,
        ignoreRefresh: true,
        syncOnUpdate: value =>
        {
            ImpNetSpawning.Instance.SetWeatherVariable2ServerRpc(value);
            ImpOutput.Send($"Weather variable 2 set to {value}!", type: NotificationType.Confirmation);
        });

    internal readonly ImpBinding<int> GroupCredits = new(
        Imperium.Terminal.groupCredits,
        ignoreRefresh: true,
        syncOnUpdate: value =>
        {
            ImpNetQuota.Instance.SetGroupCreditsServerRpc(value);
            ImpOutput.Send(
                $"Successfully changed group credits to {value}!",
                type: NotificationType.Confirmation
            );
        });

    internal readonly ImpBinding<int> ProfitQuota = new(
        Imperium.TimeOfDay.profitQuota,
        ignoreRefresh: true,
        syncOnUpdate: value =>
        {
            ImpNetQuota.Instance.SetProfitQuotaServerRpc(value);
            ImpOutput.Send(
                $"Successfully changed profit quota to {value}!",
                type: NotificationType.Confirmation
            );
        });

    internal readonly ImpBinding<int> QuotaDeadline = new(
        Imperium.TimeOfDay.daysUntilDeadline,
        ignoreRefresh: true,
        syncOnUpdate: value =>
        {
            ImpNetQuota.Instance.SetDeadlineDaysServerRpc(value);
            ImpOutput.Send(
                $"Successfully changed quota deadline to {value}!",
                type: NotificationType.Confirmation
            );
        });

    internal readonly ImpBinding<bool> DisableQuota = new(
        false,
        update: value =>
        {
            if (value) Imperium.TimeOfDay.timeUntilDeadline = Imperium.TimeOfDay.totalTime * 3f;
        }
    );

    internal readonly ImpBinding<bool> AllPlayersDead = new(
        false,
        ignoreRefresh: true,
        update: value => Imperium.StartOfRound.allPlayersDead = value,
        syncUpdate: value => ImpNetPlayer.Instance.SetAllPlayersDeadServerRpc(value)
    );

    [ImpAttributes.RemoteMethod]
    internal void FulfillQuota()
    {
        Imperium.TimeOfDay.daysUntilDeadline = -1;
        Imperium.TimeOfDay.quotaFulfilled = Imperium.TimeOfDay.profitQuota;
        Imperium.TimeOfDay.SetNewProfitQuota();
        ProfitQuota.Set(Imperium.TimeOfDay.profitQuota);
    }

    [ImpAttributes.RemoteMethod]
    internal void ResetQuota()
    {
        Imperium.TimeOfDay.SyncNewProfitQuotaClientRpc(
            Imperium.TimeOfDay.quotaVariables.startingQuota, 0, 0
        );
        ProfitQuota.Set(Imperium.TimeOfDay.quotaVariables.startingQuota);
    }

    protected override void OnSceneLoad()
    {
        MaxIndoorPower.Refresh(Imperium.RoundManager.currentMaxInsidePower);
        MaxOutdoorPower.Refresh(Imperium.RoundManager.currentMaxOutsidePower);
        MaxDaytimePower.Refresh(Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount);

        IndoorDeviation.Refresh(Imperium.RoundManager.currentLevel.spawnProbabilityRange);
        DaytimeDeviation.Refresh(Imperium.RoundManager.currentLevel.daytimeEnemiesProbabilityRange);

        MinIndoorSpawns.Refresh(Imperium.RoundManager.minEnemiesToSpawn);
        MinOutdoorSpawns.Refresh(Imperium.RoundManager.minOutsideEnemiesToSpawn);

        WeatherVariable1.Refresh(Imperium.TimeOfDay.currentWeatherVariable);
        WeatherVariable2.Refresh(Imperium.TimeOfDay.currentWeatherVariable2);

        ProfitQuota.Refresh(Imperium.TimeOfDay.profitQuota);
        QuotaDeadline.Refresh(Imperium.TimeOfDay.daysUntilDeadline);
        GroupCredits.Refresh(Imperium.Terminal.groupCredits);
    }

    [ImpAttributes.LocalMethod]
    internal static void ChangeWeather(int levelIndex, LevelWeatherType weatherType)
    {
        Imperium.StartOfRound.levels[levelIndex].currentWeather = weatherType;

        RefreshWeather();

        var planetName = Imperium.StartOfRound.levels[levelIndex].PlanetName;
        var weatherName = weatherType.ToString();
        ImpOutput.Send($"Successfully changed the weather on {planetName} to {weatherName}",
            type: NotificationType.Confirmation);
    }

    [ImpAttributes.LocalMethod]
    private static void RefreshWeather()
    {
        Reflection.Invoke(Imperium.RoundManager, "SetToCurrentLevelWeather");
        Imperium.StartOfRound.SetMapScreenInfoToCurrentLevel();
        for (var i = 0; i < Imperium.TimeOfDay.effects.Length; i++)
        {
            var weatherEffect = Imperium.TimeOfDay.effects[i];
            var isEnabled = (int)Imperium.StartOfRound.currentLevel.currentWeather == i;
            weatherEffect.effectEnabled = isEnabled;
            if (weatherEffect.effectPermanentObject != null)
            {
                weatherEffect.effectPermanentObject.SetActive(value: isEnabled);
            }

            if (weatherEffect.effectObject != null)
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

    internal static void NavigateTo(int levelIndex)
    {
        Imperium.StartOfRound.ChangeLevelServerRpc(levelIndex, Imperium.Terminal.groupCredits);

        // Send scene refresh so moon related data is refreshed
        Imperium.IsSceneLoaded.Refresh();
    }

    internal static void PlayClip(AudioClip audioClip, bool randomize = false)
    {
        RoundManager.PlayRandomClip(Imperium.HUDManager.UIAudio, [audioClip], randomize);
    }
}
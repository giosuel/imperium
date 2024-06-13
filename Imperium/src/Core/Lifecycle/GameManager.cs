#region

using System.Collections.Generic;
using Imperium.API.Types;
using Imperium.API.Types.Networking;
using Imperium.Netcode;
using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Core.Lifecycle;

internal class GameManager : ImpLifecycleObject
{
    internal readonly ImpNetEvent FulfillQuotaEvent = new("FulfillQuota");

    internal GameManager(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected)
        : base(sceneLoaded, playersConnected)
    {
        if (NetworkManager.Singleton.IsHost) FulfillQuotaEvent.OnServerReceive += FulfillQuota;
    }

    internal readonly ImpBinding<int> CustomSeed = new(-1);

    internal readonly ImpNetworkBinding<int> GroupCredits = new(
        "GroupCredits",
        Imperium.Terminal.groupCredits,
        onUpdateClient: value => Imperium.Terminal.groupCredits = value
    );

    internal readonly ImpNetworkBinding<int> ProfitQuota = new(
        "ProfitQuota",
        Imperium.TimeOfDay.profitQuota,
        Imperium.TimeOfDay.quotaVariables.startingQuota,
        onUpdateClient: value => Imperium.TimeOfDay.profitQuota = value,
        onUpdateServer: value =>
        {
            Imperium.TimeOfDay.SyncNewProfitQuotaClientRpc(
                value, 0, Imperium.TimeOfDay.timesFulfilledQuota
            );
        }
    );

    internal readonly ImpNetworkBinding<int> QuotaDeadline = new(
        "QuotaDeadline",
        Imperium.TimeOfDay.daysUntilDeadline,
        onUpdateClient: value =>
        {
            var startMatchLever = Object.FindObjectOfType<StartMatchLever>();
            startMatchLever.hasDisplayedTimeWarning = false;
            startMatchLever.triggerScript.timeToHold = 0.7f;

            Imperium.TimeOfDay.timesFulfilledQuota--;
            Imperium.TimeOfDay.quotaVariables.deadlineDaysAmount = value;
            Imperium.TimeOfDay.timeUntilDeadline = Imperium.TimeOfDay.totalTime * value;
            Imperium.TimeOfDay.UpdateProfitQuotaCurrentTime();
            Imperium.TimeOfDay.SetBuyingRateForDay();
            Imperium.HUDManager.DisplayDaysLeft(value);
        }
    );

    internal readonly ImpNetworkBinding<bool> DisableQuota = new(
        "DisableQuota",
        false,
        onUpdateClient: value =>
        {
            if (value) Imperium.TimeOfDay.timeUntilDeadline = Imperium.TimeOfDay.totalTime * 3f;
        }
    );

    [ImpAttributes.HostOnly]
    private static void FulfillQuota(ulong clientId)
    {
        Imperium.TimeOfDay.daysUntilDeadline = -1;
        Imperium.TimeOfDay.quotaFulfilled = Imperium.TimeOfDay.profitQuota;
        Imperium.TimeOfDay.SetNewProfitQuota();
    }

    protected override void OnSceneLoad()
    {
        if (!NetworkManager.Singleton.IsHost) return;

        ProfitQuota.Sync(Imperium.TimeOfDay.profitQuota);
        QuotaDeadline.Sync(Imperium.TimeOfDay.daysUntilDeadline);
        GroupCredits.Sync(Imperium.Terminal.groupCredits);
    }

    [ImpAttributes.LocalMethod]
    internal static void ChangeWeather(int levelIndex, LevelWeatherType weatherType)
    {
        Imperium.StartOfRound.levels[levelIndex].currentWeather = weatherType;

        RefreshWeather();

        var planetName = Imperium.StartOfRound.levels[levelIndex].PlanetName;
        var weatherName = weatherType.ToString();
        Imperium.IO.Send($"Successfully changed the weather on {planetName} to {weatherName}",
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

    internal static void NavigateTo(int levelIndex)
    {
        Imperium.StartOfRound.ChangeLevelServerRpc(levelIndex, Imperium.Terminal.groupCredits);

        // Send scene refresh so moon related data is refreshed
        Imperium.IsSceneLoaded.Refresh();
    }
}
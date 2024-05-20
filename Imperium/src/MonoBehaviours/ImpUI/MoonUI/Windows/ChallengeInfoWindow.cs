#region

using System;
using Imperium.Core;
using TMPro;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI.Windows;

internal class ChallengeInfoWindow : BaseWindow
{
    private TMP_Text seed;
    private TMP_Text startingCredits;
    private TMP_Text indoorPowerIncrease;
    private TMP_Text outdoorPowerIncrease;
    private TMP_Text scrapSpawnIncrease;
    private TMP_Text weather1Multiplier;
    private TMP_Text weather2Multiplier;

    private TMP_Text indoorEntity;
    private TMP_Text outdoorEntity;
    private TMP_Text scrap;
    private TMP_Text mapObject;
    private TMP_Text mapHazard;

    protected override void RegisterWindow()
    {
        seed = content.Find("General/SeedValue").GetComponent<TMP_Text>();
        startingCredits = content.Find("General/StartingCreditsValue").GetComponent<TMP_Text>();
        indoorPowerIncrease = content.Find("General/IndoorPowerIncreaseValue").GetComponent<TMP_Text>();
        outdoorPowerIncrease = content.Find("General/OutdoorPowerIncreaseValue").GetComponent<TMP_Text>();
        scrapSpawnIncrease = content.Find("General/ScrapSpawnIncreaseValue").GetComponent<TMP_Text>();
        weather1Multiplier = content.Find("General/Weather1MultiplierValue").GetComponent<TMP_Text>();
        weather2Multiplier = content.Find("General/Weather2MultiplierValue").GetComponent<TMP_Text>();

        indoorEntity = content.Find("Spawning/IndoorEntityValue").GetComponent<TMP_Text>();
        outdoorEntity = content.Find("Spawning/OutdoorEntityValue").GetComponent<TMP_Text>();
        scrap = content.Find("Spawning/ScrapValue").GetComponent<TMP_Text>();
        mapObject = content.Find("Spawning/MapObjectValue").GetComponent<TMP_Text>();
        mapHazard = content.Find("Spawning/MapHazardValue").GetComponent<TMP_Text>();

        Imperium.IsSceneLoaded.onUpdate += _ => OnLanding();
    }

    public void OnLanding()
    {
        seed.text = Imperium.StartOfRound.randomMapSeed.ToString();
        startingCredits.text = Imperium.Terminal.groupCredits + "$";

        var powerIncreaseRandom = new Random(StartOfRound.Instance.randomMapSeed + 5781);
        indoorPowerIncrease.text =
            (Imperium.StartOfRound.currentLevel.maxEnemyPowerCount + powerIncreaseRandom.Next(0, 8)).ToString();
        outdoorPowerIncrease.text =
            (Imperium.StartOfRound.currentLevel.maxOutsideEnemyPowerCount + powerIncreaseRandom.Next(0, 8)).ToString();
        scrapSpawnIncrease.text = $"+{MoonManager.Current.ChallengeScrapAmount - MoonManager.Current.ScrapAmount}";

        var weatherRandom = new Random(StartOfRound.Instance.randomMapSeed);
        weather1Multiplier.text = $"x{(weatherRandom.Next(0, 100) < 20 ? weatherRandom.Next(20, 80) * 0.02f : 1)}";
        weather2Multiplier.text = $"x{(weatherRandom.Next(0, 100) < 20 ? weatherRandom.Next(20, 80) * 0.02f : 1)}";

        var increasedIndoorIndex = Imperium.RoundManager.increasedInsideEnemySpawnRateIndex;
        indoorEntity.text = increasedIndoorIndex != -1
            ? Imperium.StartOfRound.currentLevel.Enemies[increasedIndoorIndex].enemyType.enemyName
            : "-";

        var increasedOutdoorIndex = Imperium.RoundManager.increasedOutsideEnemySpawnRateIndex;
        outdoorEntity.text = increasedOutdoorIndex != -1
            ? Imperium.StartOfRound.currentLevel.OutsideEnemies[increasedOutdoorIndex].enemyType.enemyName
            : "-";

        var increasedScrapIndex = Imperium.RoundManager.increasedScrapSpawnRateIndex;
        scrap.text = increasedScrapIndex != -1
            ? Imperium.StartOfRound.currentLevel.spawnableScrap[increasedScrapIndex].spawnableItem.itemName
            : "-";

        var increasedMapObjectIndex = Imperium.RoundManager.increasedMapPropSpawnRateIndex;
        mapObject.text = increasedMapObjectIndex != -1
            ? Imperium.StartOfRound.currentLevel.spawnableOutsideObjects[increasedMapObjectIndex].spawnableObject.name
            : "-";

        var increasedMapHazardIndex = Imperium.RoundManager.increasedMapHazardSpawnRateIndex;
        mapHazard.text = increasedMapHazardIndex != -1
            ? Imperium.StartOfRound.currentLevel.spawnableMapObjects[increasedMapHazardIndex].prefabToSpawn.name
            : "-";
    }
}
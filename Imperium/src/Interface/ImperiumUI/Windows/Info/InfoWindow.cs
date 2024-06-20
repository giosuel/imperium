#region

using System.Linq;
using Imperium.Core;
using TMPro;
using UnityEngine;
using Random = System.Random;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Info;

internal class InfoWindow : ImperiumWindow
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

    private TMP_Text scrapAmount;
    private TMP_Text weather;
    private TMP_Text mapObjects;
    private TMP_Text turrets;
    private TMP_Text landmines;
    private TMP_Text steamleaks;
    private TMP_Text doors;
    private TMP_Text securityDoors;

    private TMP_Text maxIndoorPower;
    private TMP_Text maxOutdoorPower;
    private TMP_Text maxDaytimePower;

    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        seed = content.Find("Left/General/SeedValue").GetComponent<TMP_Text>();
        scrapAmount = content.Find("Left/General/ScrapAmountValue").GetComponent<TMP_Text>();
        weather = content.Find("Left/General/WeatherValue").GetComponent<TMP_Text>();
        mapObjects = content.Find("Left/General/MapObjectsValue").GetComponent<TMP_Text>();
        turrets = content.Find("Left/General/TurretsValue").GetComponent<TMP_Text>();
        landmines = content.Find("Left/General/LandminesValue").GetComponent<TMP_Text>();
        steamleaks = content.Find("Left/General/SteamleaksValue").GetComponent<TMP_Text>();
        doors = content.Find("Left/General/DoorsValue").GetComponent<TMP_Text>();
        securityDoors = content.Find("Left/General/SecurityDoorsValue").GetComponent<TMP_Text>();

        maxIndoorPower = content.Find("Left/PowerLevels/MaxIndoorPowerValue").GetComponent<TMP_Text>();
        maxOutdoorPower = content.Find("Left/PowerLevels/MaxOutdoorPowerValue").GetComponent<TMP_Text>();
        maxDaytimePower = content.Find("Left/PowerLevels/MaxDaytimePowerValue").GetComponent<TMP_Text>();

        startingCredits = content.Find("Right/ChallengeMoon/StartingCreditsValue").GetComponent<TMP_Text>();
        indoorPowerIncrease = content.Find("Right/ChallengeMoon/IndoorPowerIncreaseValue").GetComponent<TMP_Text>();
        outdoorPowerIncrease = content.Find("Right/ChallengeMoon/OutdoorPowerIncreaseValue").GetComponent<TMP_Text>();
        scrapSpawnIncrease = content.Find("Right/ChallengeMoon/ScrapSpawnIncreaseValue").GetComponent<TMP_Text>();
        weather1Multiplier = content.Find("Right/ChallengeMoon/Weather1MultiplierValue").GetComponent<TMP_Text>();
        weather2Multiplier = content.Find("Right/ChallengeMoon/Weather2MultiplierValue").GetComponent<TMP_Text>();

        indoorEntity = content.Find("Right/Spawning/IndoorEntityValue").GetComponent<TMP_Text>();
        outdoorEntity = content.Find("Right/Spawning/OutdoorEntityValue").GetComponent<TMP_Text>();
        scrap = content.Find("Right/Spawning/ScrapValue").GetComponent<TMP_Text>();
        mapObject = content.Find("Right/Spawning/MapObjectValue").GetComponent<TMP_Text>();
        mapHazard = content.Find("Right/Spawning/MapHazardValue").GetComponent<TMP_Text>();

        Imperium.IsSceneLoaded.onUpdate += _ => OnSceneChange();
    }

    protected override void OnOpen()
    {
        OnSceneChange();
    }

    public void OnSceneChange()
    {
        seed.text = Imperium.StartOfRound.randomMapSeed.ToString();
        startingCredits.text = Imperium.Terminal.groupCredits + "$";

        var powerIncreaseRandom = new Random(StartOfRound.Instance.randomMapSeed + 5781);
        indoorPowerIncrease.text =
            (Imperium.StartOfRound.currentLevel.maxEnemyPowerCount + powerIncreaseRandom.Next(0, 8)).ToString();
        outdoorPowerIncrease.text =
            (Imperium.StartOfRound.currentLevel.maxOutsideEnemyPowerCount + powerIncreaseRandom.Next(0, 8)).ToString();
        scrapSpawnIncrease.text = $"+{Imperium.MoonManager.ChallengeScrapAmount - Imperium.MoonManager.ScrapAmount}";

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

        scrapAmount.text = Imperium.ObjectManager.CurrentLevelItems.Value.Count(item => item.itemProperties.isScrap)
            .ToString();
        weather.text = (int)Imperium.StartOfRound.currentLevel.currentWeather >= 0
            ? ImpConstants.MoonWeathers[(int)Imperium.StartOfRound.currentLevel.currentWeather]
            : "Clear";
        mapObjects.text = "?";
        turrets.text = Imperium.ObjectManager.CurrentLevelTurrets.Value.Count.ToString();
        landmines.text = Imperium.ObjectManager.CurrentLevelLandmines.Value.Count.ToString();
        steamleaks.text = Imperium.ObjectManager.CurrentLevelSteamValves.Value.Count.ToString();
        doors.text = Imperium.ObjectManager.CurrentLevelDoors.Value.Count.ToString();
        securityDoors.text = Imperium.ObjectManager.CurrentLevelSecurityDoors.Value.Count.ToString();

        maxIndoorPower.text = Imperium.RoundManager.currentLevel.maxEnemyPowerCount.ToString();
        maxOutdoorPower.text = Imperium.RoundManager.currentLevel.maxOutsideEnemyPowerCount.ToString();
        maxDaytimePower.text = Imperium.RoundManager.currentLevel.maxDaytimeEnemyPowerCount.ToString();
    }
}
#region

using System.Linq;
using Imperium.Core;
using TMPro;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MoonUI.Windows;

internal class MoonInfoWindow : BaseWindow
{
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

    protected override void RegisterWindow()
    {
        scrapAmount = content.Find("General/ScrapAmountValue").GetComponent<TMP_Text>();
        weather = content.Find("General/WeatherValue").GetComponent<TMP_Text>();
        mapObjects = content.Find("General/MapObjectsValue").GetComponent<TMP_Text>();
        turrets = content.Find("General/TurretsValue").GetComponent<TMP_Text>();
        landmines = content.Find("General/LandminesValue").GetComponent<TMP_Text>();
        steamleaks = content.Find("General/SteamleaksValue").GetComponent<TMP_Text>();
        doors = content.Find("General/DoorsValue").GetComponent<TMP_Text>();
        securityDoors = content.Find("General/SecurityDoorsValue").GetComponent<TMP_Text>();

        maxIndoorPower = content.Find("Spawning/MaxIndoorPowerValue").GetComponent<TMP_Text>();
        maxOutdoorPower = content.Find("Spawning/MaxOutdoorPowerValue").GetComponent<TMP_Text>();
        maxDaytimePower = content.Find("Spawning/MaxDaytimePowerValue").GetComponent<TMP_Text>();

        Imperium.IsSceneLoaded.onTrigger += OnSceneLoaded;
    }

    private void OnSceneLoaded()
    {
        scrapAmount.text = Imperium.ObjectManager.CurrentLevelItems.Value.Count(item => item.itemProperties.isScrap)
            .ToString();
        weather.text = (int)Imperium.StartOfRound.currentLevel.currentWeather >= 0
            ? ImpConstants.MoonWeathers[(int)Imperium.StartOfRound.currentLevel.currentWeather]
            : "Clear";
        mapObjects.text = "?";
        turrets.text = Imperium.ObjectManager.CurrentLevelTurrets.Value.Count.ToString();
        landmines.text = Imperium.ObjectManager.CurrentLevelLandmines.Value.Count.ToString();
        steamleaks.text = Imperium.ObjectManager.CurrentLevelSteamleaks.Value.Count.ToString();
        doors.text = Imperium.ObjectManager.CurrentLevelDoors.Value.Count.ToString();
        securityDoors.text = Imperium.ObjectManager.CurrentLevelSecurityDoors.Value.Count.ToString();

        maxIndoorPower.text = MoonManager.Current.OriginalMoonData.maxIndoorPower.ToString();
        maxOutdoorPower.text = MoonManager.Current.OriginalMoonData.maxOutdoorPower.ToString();
        maxDaytimePower.text = MoonManager.Current.OriginalMoonData.maxDaytimePower.ToString();
    }
}
#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Netcode;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.WeatherUI;

internal class WeatherUI : StandaloneUI
{
    private GameObject templateEntry;
    private readonly Dictionary<int, TMP_Dropdown> dropdowns = [];

    public override void Awake() => InitializeUI();

    protected override void InitUI()
    {
        templateEntry = content.Find("Template").gameObject;
        templateEntry.SetActive(false);

        for (var i = 0; i < Imperium.StartOfRound.levels.Length; i++)
        {
            var level = Imperium.StartOfRound.levels[i];
            var dropdownObj = Instantiate(templateEntry, content);
            dropdownObj.SetActive(true);
            dropdownObj.transform.Find("Title").GetComponent<TMP_Text>().text = level.PlanetName;
            dropdowns[i] = dropdownObj.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
        }
    }

    protected override void OnOpen()
    {
        var levels = Imperium.StartOfRound.levels;
        var options = ImpConstants.MoonWeathers
            .Select(weather => new TMP_Dropdown.OptionData(weather))
            .ToList();

        foreach (var (levelIndex, dropdown) in dropdowns)
        {
            dropdown.options = options;
            dropdown.value = (int)levels[levelIndex].currentWeather + 1;

            dropdown.onValueChanged.AddListener(_ =>
            {
                ImpNetWeather.Instance.ChangeWeatherServerRpc(levelIndex, (LevelWeatherType)(dropdown.value - 1));
            });
        }
    }
}
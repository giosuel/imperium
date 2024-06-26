#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Types;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.MoonControl.Widgets;

public class WeatherForecaster : ImpWidget
{
    private GameObject template;
    private Transform container;
    private readonly Dictionary<int, TMP_Dropdown> dropdowns = [];

    protected override void InitWidget()
    {
        container = transform.Find("ScrollView/Viewport/Content/WeatherGrid");
        template = container.Find("Template").gameObject;
        template.SetActive(false);

        for (var i = 0; i < Imperium.StartOfRound.levels.Length; i++)
        {
            var level = Imperium.StartOfRound.levels[i];
            var dropdownObj = Instantiate(template, container);
            dropdownObj.SetActive(true);
            dropdownObj.transform.Find("Title").GetComponent<TMP_Text>().text = level.PlanetName;
            dropdowns[i] = dropdownObj.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
        }
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("", Variant.DARKER),
            new StyleOverride("ScrollView/Scrollbar", Variant.DARKEST),
            new StyleOverride("ScrollView/Scrollbar/SlidingArea/Handle", Variant.FOREGROUND)
        );

        // Update template and real entries
        ImpThemeManager.Style(
            themeUpdate,
            template.transform,
            new StyleOverride("", Variant.FOREGROUND),
            new StyleOverride("Arrow", Variant.FOREGROUND),
            new StyleOverride("Template", Variant.DARKER),
            new StyleOverride("Template/Viewport/Content/Item/Background", Variant.DARKER),
            new StyleOverride("Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );

        foreach (var dropdown in dropdowns)
        {
            ImpThemeManager.Style(
                themeUpdate,
                dropdown.Value.transform,
                new StyleOverride("", Variant.FOREGROUND),
                new StyleOverride("Arrow", Variant.FOREGROUND),
                new StyleOverride("Template", Variant.DARKER),
                new StyleOverride("Template/Viewport/Content/Item/Background", Variant.DARKER),
                new StyleOverride("Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
            );
        }
    }

    protected override void OnOpen()
    {
        var levels = Imperium.StartOfRound.levels;
        var options = Enum.GetValues(typeof(LevelWeatherType)).Cast<LevelWeatherType>()
            .Select(enumValue => Enum.GetName(typeof(LevelWeatherType), enumValue))
            .Select(weather => new TMP_Dropdown.OptionData(weather))
            .ToList();

        foreach (var (levelIndex, dropdown) in dropdowns)
        {
            dropdown.options = options;
            dropdown.value = (int)levels[levelIndex].currentWeather + 1;

            dropdown.onValueChanged.AddListener(_ =>
            {
                Imperium.MoonManager.ChangeWeather(new ChangeWeatherRequest
                {
                    LevelIndex = levelIndex,
                    WeatherType = (LevelWeatherType)dropdown.value
                });
            });
        }
    }
}
#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Netcode;
using Imperium.Types;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.WeatherUI;

internal class WeatherUI : SingleplexUI
{
    private GameObject templateEntry;
    private readonly Dictionary<int, TMP_Dropdown> dropdowns = [];

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

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        // Update template and real entries
        ImpThemeManager.Style(
            themeUpdate,
            templateEntry.transform,
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
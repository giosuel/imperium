using System;
using System.Collections.Generic;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Imperium.Types;

public struct ImpTheme
{
    internal Color backgroundColor;
    internal Color primaryColor;
    internal Color? secondaryColor;
    internal Color textColor;
}

public enum Variant
{
    // Window background
    BACKGROUND,

    // Checkbox borders, Scrollbar Handles (Replaced by secondary color if present)
    FOREGROUND,

    // Checkmarks, Icon buttons (Replaced by secondary color if present)
    LIGHTER,

    // Currently unused
    LIGHTEST,

    // Borders, Text Buttons
    DARKER,

    // Dropdown templates
    DARKEST,

    // Selection
    FADED,

    // Faded text
    FADED_TEXT
}

public readonly struct StyleOverride(string componentPath, Variant styleVariant)
{
    /// <summary>
    /// Path to the child element relative to the provided root.
    /// </summary>
    public string Path { get; } = componentPath;

    /// <summary>
    /// The variant of style to use for the given component.
    /// </summary>
    public Variant Variant { get; } = styleVariant;
}

public static class ImpThemeManager
{
    /// <summary>
    /// Applies styles to a UI component and it's children based on overrides.
    ///
    /// </summary>
    /// <param name="theme">The theme to use for the styling</param>
    /// <param name="container">The root container that contians all the UI components</param>
    /// <param name="colorOverrides">A list of style overrides</param>
    public static void Style(
        ImpTheme theme,
        Transform container,
        params StyleOverride[] colorOverrides
    )
    {
        foreach (var colorOverride in colorOverrides)
        {
            var image = string.IsNullOrEmpty(colorOverride.Path)
                ? container?.GetComponent<Image>()
                : container?.Find(colorOverride.Path)?.GetComponent<Image>();

            if (!image) continue;
            image.color = GetColor(theme, colorOverride.Variant);
        }
    }

    public static void StyleText(
        ImpTheme theme,
        Transform container,
        params StyleOverride[] colorOverrides
    )
    {
        foreach (var colorOverride in colorOverrides)
        {
            var image = string.IsNullOrEmpty(colorOverride.Path)
                ? container?.GetComponent<TMP_Text>()
                : container?.Find(colorOverride.Path)?.GetComponent<TMP_Text>();

            if (!image) continue;
            image.color = GetColor(theme, colorOverride.Variant);
        }
    }

    private static Color GetColor(ImpTheme theme, Variant variant)
    {
        Color.RGBToHSV(theme.primaryColor, out var primaryHue, out var primarySaturation, out var primaryValue);
        float secondaryHue = 0;
        float secondarySaturation = 0;
        float secondaryValue = 0;
        if (theme.secondaryColor.HasValue)
        {
            Color.RGBToHSV(theme.secondaryColor.Value, out secondaryHue, out secondarySaturation, out secondaryValue);
        }

        var textColor = theme.textColor;
        return variant switch
        {
            Variant.BACKGROUND => theme.backgroundColor,
            Variant.FOREGROUND => theme.secondaryColor ?? theme.primaryColor,
            Variant.LIGHTEST => Color.HSVToRGB(primarySaturation, primarySaturation, primaryValue * 1.6f),
            Variant.LIGHTER => theme.secondaryColor.HasValue
                ? Color.HSVToRGB(secondaryHue, secondarySaturation, secondaryValue * 1.2f)
                : Color.HSVToRGB(primaryHue, primarySaturation, primaryValue * 1.2f),
            Variant.DARKER => Color.HSVToRGB(primaryHue, primarySaturation, primaryValue * 0.8f),
            Variant.DARKEST => Color.HSVToRGB(primaryHue, primarySaturation, primaryValue * 0.4f),
            Variant.FADED => Color.HSVToRGB(primaryHue, primarySaturation, primaryValue * 0.8f),
            Variant.FADED_TEXT => new Color(textColor.r, textColor.g, textColor.b, 0.8f),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public static void BindTheme(ImpBinding<string> themeConfig, ImpBinding<ImpTheme> themeBinding)
    {
        if (Themes.TryGetValue(themeConfig.Value, out var theme))
        {
            themeBinding.Set(theme);
        }
        else
        {
            // Reset theme to default if config was invalid
            themeBinding.Set(DefaultTheme);
            themeConfig.Set("Imperium");
        }

        themeConfig.onUpdate += value => themeBinding.Set(Themes[value]);
    }

    private static Color HEXToRGB(string hexColor)
    {
        ColorUtility.TryParseHtmlString(hexColor, out var rgbColor);
        return rgbColor;
    }

    public static ImpTheme DefaultTheme => Themes["Imperium"];

    private static readonly Dictionary<string, ImpTheme> Themes = new()
    {
        {
            "Imperium", new ImpTheme
            {
                backgroundColor = HEXToRGB("#4F0505F4"),
                primaryColor = HEXToRGB("#D63300"),
                textColor = HEXToRGB("#FFFFFF")
            }
        },
        {
            "Prismarine", new ImpTheme
            {
                backgroundColor = HEXToRGB("#061C14F4"),
                primaryColor = HEXToRGB("#24C69E"),
                textColor = HEXToRGB("#FFFFFF")
            }
        },
        {
            "Forest", new ImpTheme
            {
                backgroundColor = HEXToRGB("#0D170CF4"),
                primaryColor = HEXToRGB("#2A9130"),
                textColor = HEXToRGB("#FFFFFF")
            }
        },
        {
            "Dunes", new ImpTheme
            {
                backgroundColor = HEXToRGB("#2A241EF4"),
                primaryColor = HEXToRGB("#DE8735"),
                textColor = HEXToRGB("#FFFFFF")
            }
        },
        {
            "Nordic", new ImpTheme
            {
                backgroundColor = HEXToRGB("#12131CF4"),
                primaryColor = HEXToRGB("#84A7BC"),
                textColor = HEXToRGB("#FFFFFF")
            }
        },
        {
            "Breeze", new ImpTheme
            {
                backgroundColor = HEXToRGB("#090B1CF4"),
                primaryColor = HEXToRGB("#3088BE"),
                textColor = HEXToRGB("#FFFFFF")
            }
        },
        {
            "Nebula", new ImpTheme
            {
                backgroundColor = HEXToRGB("#2D082DFA"),
                primaryColor = HEXToRGB("#84168E"),
                textColor = HEXToRGB("#FFFFFF")
            }
        },
        {
            "Vertex", new ImpTheme
            {
                backgroundColor = HEXToRGB("#23020AF4"),
                primaryColor = HEXToRGB("#B21845"),
                textColor = HEXToRGB("#FFFFFF")
            }
        }
    };
}
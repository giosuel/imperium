#region

using System;
using System.Globalization;
using UnityEngine;

#endregion

namespace Imperium.Util;

public static class Formatting
{
    /// <summary>
    ///     Formats daytime like RoundManager.currentDayTime or TimeOfDay.globalTime
    /// </summary>
    /// <param name="dayTime">Absolute timestamp</param>
    /// <returns></returns>
    public static string FormatDayTime(float dayTime) => FormatTime(TimeToNormalized(dayTime));

    /// <summary>
    ///     Generates a formatted string of a fraction; '(num1, num2)'
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <param name="ignoreEmpty">If the function should return an empty string if both parameters are zero</param>
    /// <returns></returns>
    public static string FormatFraction(int num1, int num2, bool ignoreEmpty = true)
    {
        if (ignoreEmpty && num1 == 0 && num2 == 0) return "";
        return $"({num1}/{num2})";
    }

    /// <summary>
    ///     Formats a normalized timestamp (<see cref="TimeToNormalized" />) to a readable time string.
    ///     Length of hours from <see cref="TimeOfDay.lengthOfHours" />.
    ///     Number of hour from <see cref="TimeOfDay.numberOfHours" />.
    ///     Format: "hh:mm a"
    /// </summary>
    /// <param name="normalizedTime"></param>
    /// <returns></returns>
    public static string FormatTime(float normalizedTime)
    {
        var time = (int)(normalizedTime * Imperium.TimeOfDay.lengthOfHours * Imperium.TimeOfDay.numberOfHours) + 360;
        var minutes = time % 60;
        var hours = time / 60;
        var suffix = hours < 12 ? "AM" : "PM";
        hours %= 12;
        if (hours == 0) hours = 12;

        return $"{hours:00}:{minutes:00} {suffix}";
    }

    public static string FormatMinutesSeconds(float seconds)
    {
        var minutesLeft = Mathf.RoundToInt(seconds) / 60;
        var secondsLeft = Mathf.RoundToInt(seconds) % 60;
        return $"{minutesLeft}:{secondsLeft:00}";
    }

    /// <summary>
    ///     Creates a formatted string from a unity <see cref="Vector3" />.
    ///     If a unit is provided, the unit will be appended to each scalar.
    ///     Format: "(x[unit](separator)y[unit](separator)z[unit])"
    /// </summary>
    /// <param name="input"></param>
    /// <param name="roundDigits">To how many digits the scalars should be rounded</param>
    /// <param name="separator">Scalar separator</param>
    /// <param name="unit">Optional scalar unit</param>
    /// <returns></returns>
    public static string FormatVector(
        Vector3 input,
        int roundDigits = -1,
        string separator = "/",
        string unit = ""
    )
    {
        var x = roundDigits > -1 ? MathF.Round(input.x, roundDigits) : input.x;
        var y = roundDigits > -1 ? MathF.Round(input.y, roundDigits) : input.y;
        var z = roundDigits > -1 ? MathF.Round(input.z, roundDigits) : input.z;
        return $"({x}{unit}{separator}{y}{unit}{separator}{z}{unit})";
    }

    /// <summary>
    ///     Converts an absolute timestamp to a normalized one (between 0 and 1).
    ///     Total time is provided by <see cref="TimeOfDay.totalTime" />.
    /// </summary>
    public static float TimeToNormalized(float currentTime) => currentTime / Imperium.TimeOfDay.totalTime;

    /// <summary>
    ///     Formats a normalized float to a percentage chance.
    /// </summary>
    /// <param name="chance"></param>
    /// <returns></returns>
    public static string FormatChance(float chance) => ImpMath.NormalizeFloat(
        MathF.Round(chance * 100, 2)
    ).ToString(CultureInfo.InvariantCulture) + "%";

    /// <summary>
    ///     Limits a float to 3 digits.
    ///     e.g. 100.01 => 100, 14.23 => 12.3, 1.22 => 1.22, 0.1 => 0.1
    ///     Note: This only works for positive numbers smaller than 999
    /// </summary>
    public static string FormatFloatToThreeDigits(float value) =>
        value switch
        {
            >= 100 => Mathf.RoundToInt(value).ToString(),
            >= 10 => MathF.Round(value, 1).ToString(CultureInfo.InvariantCulture),
            _ => MathF.Round(value, 2).ToString(CultureInfo.InvariantCulture)
        };
}
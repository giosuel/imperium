#region

using System;
using System.Globalization;
using UnityEngine;

#endregion

namespace Imperium.Util;

public static class ImpMath
{
    public static float SampleQuadraticBezier(float start, float end, float control, float t)
    {
        return (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * end;
    }

    /// <summary>
    ///     Removes trailing zeros from float if decimals are equal to zero
    ///     e.g. 100.00 => 100
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static float NormalizeFloat(float value)
    {
        var parsed = value.ToString(CultureInfo.InvariantCulture).Split('.');
        if (parsed.Length == 1) return value;

        if (int.Parse(parsed[1]) == 0)
        {
            return (int)value;
        }

        return MathF.Round(value);
    }

    internal static Vector3 ClosestPointAlongRay(Ray ray, Vector3 point)
    {
        var a = ray.origin;
        var b = ray.origin + ray.direction;

        var ab = b - a;

        var distance = Vector3.Dot(point - a, ab);
        distance = Mathf.Max(distance, 0f);

        return ray.origin + ray.direction * distance;
    }
}
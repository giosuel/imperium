#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Logging;
using Imperium.Core;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

#endregion

namespace Imperium.Util;

internal abstract class ImpUtils
{
    internal static int RandomItemValue(Item item)
    {
        var random = Imperium.RoundManager.AnomalyRandom != null
            ? CloneRandom(Imperium.RoundManager.AnomalyRandom)
            : new Random();
        var min = System.Math.Min(item.minValue, item.maxValue);
        var max = System.Math.Min(item.minValue, item.maxValue);
        return (int)(random.Next(min, max) * Imperium.RoundManager.scrapValueMultiplier);
    }

    /// <summary>
    /// Tries to find value in a dictionary by key. If the key does not exist,
    /// a new value of type T is created, indexed in the dictionary with the given key and returned.
    ///
    /// Basically a helper function to emulate a default dictionary.
    /// </summary>
    internal static T DictionaryGetOrNew<T>(IDictionary<string, T> map, string key) where T : new()
    {
        if (map.TryGetValue(key, out var list)) return list;
        return map[key] = new T();
    }

    internal static void ToggleGameObjects(IEnumerable<GameObject> list, bool isOn)
    {
        foreach (var obj in list.Where(obj => obj != null))
        {
            obj.SetActive(isOn);
        }
    }

    internal static void ToggleGameObjects(IEnumerable<Component> list, bool isOn)
    {
        foreach (var obj in list.Where(obj => obj != null && obj.gameObject != null))
        {
            obj.gameObject.SetActive(isOn);
        }
    }


    internal static Random CloneRandom(Random random)
    {
        var cloned = new Random();

        // The int array needs to be deep-copied since its a referenced type
        var seedArray = Reflection.Get<Random, int[]>(random, "_seedArray");
        Reflection.Set(cloned, "_seedArray", seedArray.ToArray());

        Reflection.Copy(random, cloned, "_inextp");
        Reflection.Copy(random, cloned, "_inext");

        return cloned;
    }

    internal static float TimeToNormalized(float currentTime) => currentTime / Imperium.TimeOfDay.totalTime;

    /// <summary>
    /// Formats daytime like RoundManager.currentDayTime or TimeOfDay.globalTime
    /// </summary>
    /// <param name="dayTime"></param>
    /// <returns></returns>
    internal static string FormatDayTime(float dayTime)
    {
        return FormatTime(TimeToNormalized(dayTime));
    }

    internal static string FormatFraction(int num1, int num2, bool ignoreEmpty = true)
    {
        if (ignoreEmpty && num1 == 0 && num2 == 0) return "";
        return $"({num1}/{num2})";
    }

    internal static string FormatTime(float normalizedTime)
    {
        var time = (int)(normalizedTime * Imperium.TimeOfDay.lengthOfHours * Imperium.TimeOfDay.numberOfHours) + 360;
        var minutes = time % 60;
        var hours = time / 60;
        var suffix = hours < 12 ? "AM" : "PM";
        hours %= 12;
        if (hours == 0) hours = 12;

        return $"{hours:00}:{minutes:00} {suffix}";
    }

    internal static string FormatVector(Vector3 input, int roundDigits = -1)
    {
        var x = roundDigits > -1 ? MathF.Round(input.x, roundDigits) : input.x;
        var y = roundDigits > -1 ? MathF.Round(input.y, roundDigits) : input.y;
        var z = roundDigits > -1 ? MathF.Round(input.z, roundDigits) : input.z;
        return $"({x}/{y}/{z})";
    }

    internal static T InvokeDefaultOnNull<T>(Func<T> callback)
    {
        try
        {
            return callback.Invoke();
        }
        catch (NullReferenceException)
        {
            return default;
        }
    }

    internal static void PlayClip(AudioClip audioClip, bool randomize = false)
    {
        RoundManager.PlayRandomClip(Imperium.HUDManager.UIAudio, [audioClip], randomize);
    }

    internal static SpawnableItemWithRarity AddScrapToSpawnList(
        Item itemType,
        ICollection<SpawnableItemWithRarity> scrapList
    )
    {
        var newScrap = new SpawnableItemWithRarity { spawnableItem = itemType, rarity = 0 };
        scrapList.Add(newScrap);
        return newScrap;
    }

    internal static SpawnableEnemyWithRarity AddEntityToSpawnList(
        EnemyType entityType,
        ICollection<SpawnableEnemyWithRarity> entityList
    )
    {
        var newEntity = new SpawnableEnemyWithRarity { enemyType = entityType, rarity = 0 };
        entityList.Add(newEntity);
        return newEntity;
    }

    internal abstract class RichText
    {
        internal static string Strikethrough(string value) => $"<s>{value}</s>";
        internal static string Underlined(string value) => $"<u>{value}</u>";
        internal static string Bold(string value) => $"<b>{value}</b>";
        internal static string Italic(string value) => $"<i>{value}</i>";
    }

    internal static int ToggleLayerInMask(int layerMask, int layer)
    {
        if ((layerMask & (1 << layer)) != 0)
        {
            return layerMask & ~(1 << layer);
        }

        return layerMask | (1 << layer);
    }

    internal static void LogBlock(List<string> lines, string title = "Imperium Monitoring")
    {
        if (!ImpSettings.Preferences.GeneralLogging.Value) return;

        var output = "[MON] Imperium message block :)\n";
        title = "< " + title + " >";
        var width = Mathf.Max(lines.Max(line => line.Length) + 4, 20);
        var fullWidth = string.Concat(Enumerable.Repeat("\u2550", width - 2));
        var titlePaddingCount = (width - title.Length) / 2 - 1;
        if ((width - title.Length) / 2 % 2 == 0) titlePaddingCount++;

        var titlePadding = string.Concat(Enumerable.Repeat(" ", titlePaddingCount));


        output += "\u2552" + fullWidth + "\u2555\n";
        output += "\u2502" + titlePadding + title + titlePadding + "\u2502\n";
        output += "\u255e" + fullWidth + "\u2561\n";
        output = lines.Aggregate(output,
            (current, line) => current + $"\u2502 {line}".PadRight(width - 2) + " \u2502\n");
        output += "\u2558" + fullWidth + "\u255b";

        Imperium.Log.Log(LogLevel.Message, output);
    }

    internal abstract class Interface
    {
        internal static void ToggleImageActive(Image image, bool isOn)
        {
            image.color = ChangeAlpha(image.color,
                isOn ? ImpConstants.Opacity.Enabled : ImpConstants.Opacity.ImageDisabled);
        }

        internal static void ToggleTextActive(TMP_Text text, bool isOn)
        {
            text.color = ChangeAlpha(text.color,
                isOn ? ImpConstants.Opacity.Enabled : ImpConstants.Opacity.TextDisabled);
        }

        private static Color ChangeAlpha(Color oldColor, float newAlpha)
        {
            var color = oldColor;
            color.a = newAlpha;
            return color;
        }

        internal static void ToggleCursorState(bool uiOpen)
        {
            Imperium.Player.quickMenuManager.isMenuOpen = uiOpen;
            Cursor.lockState = uiOpen ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    internal abstract class Math
    {
        internal static float SampleQuadraticBezier(float start, float end, float control, float x)
        {
            return (1 - x) * (1 - x) * start + 2 * (1 - x) * x * control + x * x * end;
        }

        internal static string FormatChance(float chance) =>
            NormalizeFloat(MathF.Round(chance * 100, 2)).ToString(CultureInfo.InvariantCulture) + "%";

        /// <summary>
        /// Removes trailing zeros from float if decimals are equal to zero
        /// e.g. 100.00 => 100
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static float NormalizeFloat(float value)
        {
            var parsed = value.ToString(CultureInfo.InvariantCulture).Split('.');
            if (parsed.Length == 1) return value;

            if (int.Parse(parsed[1]) == 0)
            {
                return (int)value;
            }

            return MathF.Round(value);
        }

        /// <summary>
        /// Limits a float to 3 digits.
        /// e.g 100.01 => 100, 14.23 => 12.3, 1.22 => 1.22, 0.1 => 0.1
        ///
        /// Note: This only works for positive numbers smaller than 999
        /// </summary>
        internal static string FormatFloatToThreeDigits(float value) =>
            value switch
            {
                >= 100 => Mathf.RoundToInt(value).ToString(),
                >= 10 => MathF.Round(value, 1).ToString(CultureInfo.InvariantCulture),
                _ => MathF.Round(value, 2).ToString(CultureInfo.InvariantCulture)
            };
    }

    internal abstract class VectorMath
    {
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

    internal abstract class Geometry
    {
        internal static LineRenderer CreateLine(
            Transform parent,
            float thickness = 0.05f,
            bool useWorldSpace = false,
            Color? color = null,
            params Vector3[] positions
        )
        {
            var rayObject = new GameObject
            {
                transform = { parent = parent },
                name = "ImpObject"
            };

            var lineRenderer = rayObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = thickness;
            lineRenderer.endWidth = thickness;
            lineRenderer.useWorldSpace = useWorldSpace;
            lineRenderer.positionCount = 2;

            return lineRenderer;
        }

        internal static void SetLineColor(LineRenderer lineRenderer, Color color)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        internal static void SetLinePositions(LineRenderer lineRenderer, params Vector3[] positions)
        {
            lineRenderer.positionCount = positions.Length;
            for (var i = 0; i < positions.Length; i++)
            {
                lineRenderer.SetPosition(i, positions[i]);
            }
        }

        internal static GameObject CreatePrimitive(
            PrimitiveType type,
            Transform parent,
            Color color,
            float size = 1,
            string name = null
        )
        {
            var sphere = CreatePrimitive(type, parent, size, name);

            var renderer = sphere.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            var material = renderer.material;
            material.shader = Shader.Find("HDRP/Unlit");
            material.color = color;

            return sphere;
        }

        internal static GameObject CreatePrimitive(
            PrimitiveType type,
            Transform parent,
            [CanBeNull] Material material,
            float size = 1,
            string name = null
        )
        {
            var sphere = CreatePrimitive(type, parent, size, name);
            if (name != null) sphere.name = name;

            var renderer = sphere.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            if (material != null)
            {
                renderer.material = material;
            }
            else
            {
                renderer.material.shader = Shader.Find("HDRP/Unlit");
            }

            return sphere;
        }

        private static GameObject CreatePrimitive(
            PrimitiveType type,
            Transform parent,
            float size = 1,
            string name = "ImpObject"
        )
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;

            switch (type)
            {
                case PrimitiveType.Sphere:
                    Object.Destroy(primitive.GetComponent<SphereCollider>());
                    break;
                case PrimitiveType.Cylinder:
                case PrimitiveType.Cube:
                case PrimitiveType.Plane:
                case PrimitiveType.Quad:
                    Object.Destroy(primitive.GetComponent<BoxCollider>());
                    break;
                case PrimitiveType.Capsule:
                    Object.Destroy(primitive.GetComponent<CapsuleCollider>());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            primitive.transform.localScale = Vector3.one * size;
            primitive.transform.position = parent.position;
            primitive.transform.SetParent(parent);

            return primitive;
        }
    }
}
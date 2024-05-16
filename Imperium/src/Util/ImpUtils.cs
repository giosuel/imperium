#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Imperium.Core;
using JetBrains.Annotations;
using MonoMod.Cil;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = System.Random;

#endregion

namespace Imperium.Util;

public abstract class ImpUtils
{
    internal static int RandomItemValue(Item item)
    {
        var random = Imperium.RoundManager.AnomalyRandom != null
            ? CloneRandom(Imperium.RoundManager.AnomalyRandom)
            : new Random();
        var min = System.Math.Min(item.minValue, item.maxValue);
        var max = System.Math.Max(item.minValue, item.maxValue);
        return (int)(random.Next(min, max) * Imperium.RoundManager.scrapValueMultiplier);
    }

    /// <summary>
    ///     Tries to find value in a dictionary by key. If the key does not exist,
    ///     a new value of type T is created, indexed in the dictionary with the given key and returned.
    ///     Basically a helper function to emulate a default dictionary.
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

    internal static Random CloneRandom(Random random)
    {
        var cloned = new Random();

        // The seed array needs to be deep-copied since arrays are referenced types
        var seedArray = Reflection.Get<Random, int[]>(random, "_seedArray");
        Reflection.Set(cloned, "_seedArray", seedArray.ToArray());

        Reflection.CopyField(random, cloned, "_inextp");
        Reflection.CopyField(random, cloned, "_inext");

        return cloned;
    }

    internal static float TimeToNormalized(float currentTime) => currentTime / Imperium.TimeOfDay.totalTime;

    /// <summary>
    ///     Formats daytime like RoundManager.currentDayTime or TimeOfDay.globalTime
    /// </summary>
    /// <param name="dayTime"></param>
    /// <returns></returns>
    internal static string FormatDayTime(float dayTime) => FormatTime(TimeToNormalized(dayTime));

    /// <summary>
    ///     Generates a formatted string of a fraction; '(num1, num2)'
    /// </summary>
    /// <param name="num1"></param>
    /// <param name="num2"></param>
    /// <param name="ignoreEmpty">If the function should return an empty string if both parameters are zero</param>
    /// <returns></returns>
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

    internal static string FormatMinutesSeconds(float seconds)
    {
        var minutesLeft = Mathf.RoundToInt(seconds) / 60;
        var secondsLeft = Mathf.RoundToInt(seconds) % 60;
        return $"{minutesLeft}:{secondsLeft:00}";
    }

    internal static string FormatSeconds(float seconds)
    {
        ;
        return $"{seconds:0.0}s";
    }

    internal static string FormatVector(
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

    /// <summary>
    /// Formats the parents of a Unity transform into a string.
    ///
    /// e.g. "ImpInterface/imperium_ui/Container/Window/Content"
    /// </summary>
    /// <param name="root"></param>
    /// <returns></returns>
    internal static string GetTransformPath(Transform root)
    {
        if (!root) return "";

        List<string> path = [];
        while (root)
        {
            path.Add(root.name);
            root = root.parent;
        }

        return path.AsEnumerable().Reverse().Aggregate((a, b) => a + "/" + b);
    }

    internal static Item AddScrapToSpawnList(
        Item itemType,
        ICollection<SpawnableItemWithRarity> scrapList
    )
    {
        var newScrap = new SpawnableItemWithRarity { spawnableItem = itemType, rarity = 0 };
        scrapList.Add(newScrap);
        return itemType;
    }

    internal static EnemyType AddEntityToSpawnList(
        EnemyType entityType,
        ICollection<SpawnableEnemyWithRarity> entityList
    )
    {
        var newEntity = new SpawnableEnemyWithRarity { enemyType = entityType, rarity = 0 };
        entityList.Add(newEntity);
        return entityType;
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

    internal static int ToggleLayersInMask(int layerMask, params int[] layers)
    {
        return layers.Aggregate(layerMask, ToggleLayerInMask);
    }

    internal abstract class Interface
    {
        internal static void ToggleImageActive(Image image, bool isOn)
        {
            image.color = ChangeAlpha(
                image.color,
                isOn ? ImpConstants.Opacity.Enabled : ImpConstants.Opacity.ImageDisabled
            );
        }

        internal static void ToggleTextActive(TMP_Text text, bool isOn)
        {
            text.color = ChangeAlpha(
                text.color,
                isOn ? ImpConstants.Opacity.Enabled : ImpConstants.Opacity.TextDisabled
            );
        }

        internal static Color ChangeAlpha(Color oldColor, float newAlpha)
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
        ///     Removes trailing zeros from float if decimals are equal to zero
        ///     e.g. 100.00 => 100
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
        ///     Limits a float to 3 digits.
        ///     e.g 100.01 => 100, 14.23 => 12.3, 1.22 => 1.22, 0.1 => 0.1
        ///     Note: This only works for positive numbers smaller than 999
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

    public abstract class Geometry
    {
        public static LineRenderer CreateLine(
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

        public static void SetLineColor(LineRenderer lineRenderer, Color color)
        {
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        public static void SetLinePositions(LineRenderer lineRenderer, params Vector3[] positions)
        {
            lineRenderer.positionCount = positions.Length;
            for (var i = 0; i < positions.Length; i++)
            {
                lineRenderer.SetPosition(i, positions[i]);
            }
        }

        internal static GameObject CreatePrimitive(
            PrimitiveType type,
            [CanBeNull] Transform parent,
            Color color,
            float size = 1,
            int layer = 0,
            string name = null,
            bool removeCollider = true
        )
        {
            var sphere = CreatePrimitive(type, parent, size, name, layer, removeCollider);

            var renderer = sphere.GetComponent<MeshRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;

            var material = renderer.material;
            material.shader = Shader.Find("HDRP/Unlit");
            material.color = color;

            return sphere;
        }

        internal static GameObject CreatePrimitive(
            PrimitiveType type,
            [CanBeNull] Transform parent,
            [CanBeNull] Material material,
            float size = 1,
            int layer = 0,
            string name = null,
            bool removeCollider = true
        )
        {
            var sphere = CreatePrimitive(type, parent, size, name, layer, removeCollider);
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

        internal static GameObject CreatePrimitive(
            PrimitiveType type,
            [CanBeNull] Transform parent,
            float size = 1,
            string name = "ImpObject",
            int layer = 0,
            bool removeCollider = true,
            bool removeRenderer = false
        )
        {
            var primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;
            primitive.layer = layer;

            if (removeCollider)
            {
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
            }

            primitive.transform.localScale = Vector3.one * size;
            if (parent)
            {
                primitive.transform.position = parent.position;
                primitive.transform.SetParent(parent);
            }

            if (removeRenderer)
            {
                Object.Destroy(primitive.GetComponent<MeshRenderer>());
                Object.Destroy(primitive.GetComponent<MeshFilter>());
            }

            return primitive;
        }
    }
}
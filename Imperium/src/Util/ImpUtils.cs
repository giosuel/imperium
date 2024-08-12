#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using GameNetcodeStuff;
using HarmonyLib;
using Imperium.Core;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

#endregion

namespace Imperium.Util;

public abstract class ImpUtils
{
    public static int RandomItemValue(Item item)
    {
        var random = Imperium.RoundManager.AnomalyRandom != null
            ? CloneRandom(Imperium.RoundManager.AnomalyRandom)
            : new Random();
        var min = Math.Min(item.minValue, item.maxValue);
        var max = Math.Max(item.minValue, item.maxValue);
        return (int)(random.Next(min, max) * Imperium.RoundManager.scrapValueMultiplier);
    }

    /// <summary>
    ///     Tries to find value in a dictionary by key. If the key does not exist,
    ///     a new value of type T is created, indexed in the dictionary with the given key and returned.
    ///     Basically a helper function to emulate a default dictionary.
    /// </summary>
    public static T DictionaryGetOrNew<T>(IDictionary<string, T> map, string key) where T : new()
    {
        if (map.TryGetValue(key, out var list)) return list;
        return map[key] = new T();
    }

    public static void ToggleGameObjects(IEnumerable<GameObject> list, bool isOn)
    {
        foreach (var obj in list.Where(obj => obj)) obj.SetActive(isOn);
    }

    /// <summary>
    ///     Clones a random number generator. The new generator will produce the same sequence of numbers as the original.
    /// </summary>
    public static Random CloneRandom(Random random)
    {
        var cloned = new Random();

        // The seed array needs to be deep-copied since arrays are referenced types
        var seedArray = Reflection.Get<Random, int[]>(random, "_seedArray");
        Reflection.Set(cloned, "_seedArray", seedArray.ToArray());

        Reflection.CopyField(random, cloned, "_inextp");
        Reflection.CopyField(random, cloned, "_inext");

        return cloned;
    }

    /// <summary>
    ///     Attempts to invoke a callback.
    ///     If the callback throws a <see cref="NullReferenceException" /> returns default.
    /// </summary>
    /// <param name="callback"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T InvokeDefaultOnNull<T>(Func<T> callback)
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

    public static int ToggleLayerInMask(int layerMask, int layer)
    {
        if ((layerMask & (1 << layer)) != 0)
        {
            return layerMask & ~(1 << layer);
        }

        return layerMask | (1 << layer);
    }

    public static int ToggleLayersInMask(int layerMask, params int[] layers)
    {
        return layers.Aggregate(layerMask, ToggleLayerInMask);
    }

    public static string GetEntityLocationText(EnemyAI entity)
    {
        return entity.isInsidePlayerShip
            ? "In Ship"
            : entity.isOutside
                ? "Outdoors"
                : "Indoors";
    }

    public static string GetPlayerLocationText(PlayerControllerB player) => GetPlayerLocationText(player, false);

    public static string GetPlayerLocationText(PlayerControllerB player, bool locationOnly)
    {
        var isNearOtherPlayers =
            Reflection.Invoke<PlayerControllerB, bool>(player, "NearOtherPlayers", Imperium.Player, 17f);
        var isHearingOthers = Reflection.Invoke<PlayerControllerB, bool>(
            player, "PlayerIsHearingOthersThroughWalkieTalkie", Imperium.Player
        );

        var isAlone = !locationOnly && !isNearOtherPlayers && !isHearingOthers ? " (Alone)" : "";
        if (Imperium.Player.isInHangarShipRoom) return "Ship" + isAlone;
        if (Imperium.Player.isInElevator) return "Elevator" + isAlone;

        return (Imperium.Player.isInsideFactory ? "Indoors" : "Outdoors") + isAlone;
    }

    public static string GetItemLocationText(GrabbableObject item)
    {
        return item.isInShipRoom
            ? "In Ship"
            : item.isInElevator
                ? "Elevator"
                : item.isInFactory
                    ? "Indoors"
                    : "Outdoors";
    }

    public static bool RunSafe(Action action, string logTitle = null)
    {
        return RunSafe(action, out _, logTitle);
    }

    /// <summary>
    /// Runs a function, catches all exceptions and returns a boolean with the status.
    /// /// </summary>
    /// <param name="action"></param>
    /// <param name="exception"></param>
    /// <param name="logTitle"></param>
    /// <returns></returns>
    public static bool RunSafe(Action action, out Exception exception, string logTitle = null)
    {
        try
        {
            action.Invoke();
            exception = null;
            return true;
        }
        catch (Exception e)
        {
            exception = e;
            if (logTitle != null)
            {
                Imperium.IO.LogBlock(
                    exception.StackTrace.Split('\n').Select(line => line.Trim()).ToList(),
                    title: $"[ERR] {logTitle}: {exception.Message}"
                );
            }
            return false;
        }
    }

    /// <summary>
    ///     Attempts to deserialize a JSON string into a given object. If <see cref="JsonSerializationException" /> is thrown
    ///     or the resulting object is null, false is returned and default is assigned to the out argument.
    /// </summary>
    /// <param name="jsonString"></param>
    /// <param name="deserializedObj"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool DeserializeJsonSafe<T>(string jsonString, out T deserializedObj)
    {
        try
        {
            deserializedObj = JsonConvert.DeserializeObject<T>(jsonString);
            return deserializedObj != null;
        }
        catch (JsonSerializationException)
        {
            deserializedObj = default;
            return false;
        }
    }

    public static string GetItemHeldByText(GrabbableObject item)
    {
        return item.isHeldByEnemy
            ? "An Entity"
            : item.isHeld
                ? "A Player"
                : "Nobody";
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

        internal static void ToggleCursorState(bool isShown)
        {
            Imperium.Player.quickMenuManager.isMenuOpen = isShown;
            
            Cursor.lockState = isShown ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isShown;
        }
    }

    internal abstract class Transpiling
    {
        internal static IEnumerable<CodeInstruction> SkipWaitingForSeconds(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            for (var i = 0; i < codes.Count; i++)
            {
                if (i >= 2
                    && codes[i].opcode == OpCodes.Stfld
                    && codes[i - 1].opcode == OpCodes.Newobj
                    && codes[i - 2].opcode == OpCodes.Ldc_R4
                   )
                {
                    codes[i - 2].operand = 0f;
                }
            }

            return codes.AsEnumerable();
        }
    }
}
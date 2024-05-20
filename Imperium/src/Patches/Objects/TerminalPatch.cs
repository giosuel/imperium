#region

using HarmonyLib;
using Imperium.Core;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(Terminal))]
internal static class TerminalPatch
{
    [HarmonyPatch(typeof(Terminal))]
    internal static class PreloadPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        private static void StartPatch()
        {
            if (!ImpSettings.Preferences.CustomWelcome.Value) return;

            var ingamePlayerHud = GameObject.Find("IngamePlayerHUD");
            if (ingamePlayerHud)
            {
                var tipText = ingamePlayerHud.transform.Find("BottomMiddle/SystemsOnline/TipLeft1");
                tipText.GetComponent<TMP_Text>().text = "GREETINGS, PADAWAN";
                tipText.GetComponent<TMP_Text>().fontSize = 30;
            }
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetItemSales")]
    private static void SetItemSalesPatch(Terminal __instance)
    {
        for (var i = 0; i < __instance.itemSalesPercentages.Length; i++)
        {
            __instance.itemSalesPercentages[i] = 31;
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("BeginUsingTerminal")]
    private static void BeginUsingTerminalPatch()
    {
        Imperium.Interface.Close();
    }
}
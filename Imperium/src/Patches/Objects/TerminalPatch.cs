#region

using HarmonyLib;
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
            var ingamePlayerHud = GameObject.Find("IngamePlayerHUD");
            if (ingamePlayerHud)
            {
                var tipText = ingamePlayerHud.transform.Find("BottomMiddle/SystemsOnline/TipLeft1");

                tipText.GetComponent<TMP_Text>().text = "GREETINGS, PADAWAN";
                tipText.GetComponent<TMP_Text>().fontSize = 30;
            }
        }
    }

    [HarmonyPrefix]
    [HarmonyPatch("BeginUsingTerminal")]
    private static void BeginUsingTerminalPatch()
    {
        Imperium.Interface.Close();
    }
}
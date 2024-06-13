#region

using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using HarmonyLib;
using Imperium.Util;
using UnityExplorer;
using UniverseLib.UI;

#endregion

namespace Imperium.Integration;

public static class UnityExplorerIntegration
{
    private static bool IsEnabled => Chainloader.PluginInfos.ContainsKey("com.sinai.unityexplorer");

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void CloseUI()
    {
        if (!IsEnabled) return;

        CloseUIInternal();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void PatchFunctions(Harmony harmony)
    {
        if (!IsEnabled) return;

        PatchFunctionsInternal(harmony);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void SetUIActivePatch(string id, bool active)
    {
        if (!IsEnabled) return;

        OnOpenUIInternal(id, active);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void CloseUIInternal()
    {
        UniversalUI.SetUIActive(ExplorerCore.GUID, false);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void PatchFunctionsInternal(Harmony harmony)
    {
        harmony.Patch(
            typeof(UniversalUI).GetMethod("SetUIActive"),
            prefix: new HarmonyMethod(
                typeof(UnityExplorerIntegration)
                    .GetMethod(nameof(SetUIActivePatch), bindingAttr: BindingFlags.NonPublic | BindingFlags.Static)
            )
        );
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void OnOpenUIInternal(string id, bool active)
    {
        if (ExplorerCore.GUID == id)
        {
            if (Imperium.Settings.Preferences.UnityExplorerMouseFix.Value)
            {
                ImpUtils.Interface.ToggleCursorState(active);
            }

            if (active) Imperium.Interface.Close(false);
        }
    }
}
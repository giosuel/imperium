#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(StartMatchLever))]
internal static class StartMatchLeverPatch;
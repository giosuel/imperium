#region

using HarmonyLib;
using Imperium.Core;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(StartMatchLever))]
internal static class StartMatchLeverPatch;
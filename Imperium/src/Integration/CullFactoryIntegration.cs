#region

using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using HarmonyLib;
using Imperium.Util;
using UnityEngine;
using UnityExplorer;
using UniverseLib.UI;

#endregion

// namespace Imperium.Integration;
//
// public static class CullFactoryIntegration
// {
//     private static bool IsEnabled => Chainloader.PluginInfos.ContainsKey("com.fumiko.CullFactory");
//
//     [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
//     internal static void DisableCulling(Camera camera)
//     {
//         if (!IsEnabled) return;
//
//         DisableCullingInternal(camera);
//     }
//
//     [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
//     private static void DisableCullingInternal(Camera camera)
//     {
//         var cullingOptions = camera.gameObject.AddComponent<CullFactory.Behaviours.API.CameraCullingOptions>();
//         cullingOptions.DisableCulling = true;
//     }
// }
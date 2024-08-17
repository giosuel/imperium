#region

using HarmonyLib;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(SpringManAI))]
public static class SpringManPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DoAIInterval")]
    private static void DoAIIntervalPostfixPatch(DocileLocustBeesAI __instance)
    {
        if (__instance.currentBehaviourStateIndex == 0)
        {
            Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
                __instance,
                __instance.eye,
                30,
                material: ImpAssets.WireframeRed
            );
        }
    }
}
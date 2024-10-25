#region

using HarmonyLib;
using Imperium.API.Types;
using Imperium.Util;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(DocileLocustBeesAI))]
public static class DocileLocustBeesPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("DoAIInterval")]
    private static void DoAIIntervalPostfixPatch(DocileLocustBeesAI __instance)
    {
        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            __instance,
            null,
            8,
            material: ImpAssets.WireframeRed,
            gizmoType: GizmoType.Custom
        );

        Imperium.Visualization.EntityGizmos.SphereVisualizerUpdate(
            __instance,
            null,
            16,
            material: ImpAssets.WireframeGreen,
            gizmoType: GizmoType.Custom
        );
    }
}
#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(OutOfBoundsTrigger))]
internal static class OutOfBoundsTriggerPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("OnTriggerEnter")]
    private static bool OnTriggerEnterPatch()
    {
        Imperium.Log.LogInfo("ON TRIGGER ENTERRRRRR");
        return false;
    }
}
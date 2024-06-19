#region

using Imperium.API.Types.Networking;

#endregion

namespace Imperium.API;

public static class Spawning
{
    internal static void SpawnEntity(EntitySpawnRequest request)
    {
        APIHelpers.AssertImperiumReady();
        Imperium.ObjectManager.SpawnEntity(request);
    }

    internal static void SpawnItem(ItemSpawnRequest request)
    {
        APIHelpers.AssertImperiumReady();
        Imperium.ObjectManager.SpawnItem(request);
    }

    internal static void SpawnMapHazard(MapHazardSpawnRequest request)
    {
        APIHelpers.AssertImperiumReady();
        Imperium.ObjectManager.SpawnMapHazard(request);
    }
}
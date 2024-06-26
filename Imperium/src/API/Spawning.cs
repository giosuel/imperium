#region

using Imperium.API.Types.Networking;

#endregion

namespace Imperium.API;

public static class Spawning
{
    public static void SpawnEntity(EntitySpawnRequest request)
    {
        APIHelpers.AssertImperiumReady();
        Imperium.ObjectManager.SpawnEntity(request);
    }

    public static void SpawnItem(ItemSpawnRequest request)
    {
        APIHelpers.AssertImperiumReady();
        Imperium.ObjectManager.SpawnItem(request);
    }

    public static void SpawnMapHazard(MapHazardSpawnRequest request)
    {
        APIHelpers.AssertImperiumReady();
        Imperium.ObjectManager.SpawnMapHazard(request);
    }
}
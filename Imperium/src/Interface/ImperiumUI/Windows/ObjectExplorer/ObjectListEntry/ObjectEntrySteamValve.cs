#region

using Imperium.API.Types.Networking;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal class ObjectEntrySteamValve : ObjectEntry
{
    private SteamValveHazard steamValve;

    protected override bool CanRespawn() => true;
    protected override bool CanDrop() => false;
    protected override bool CanTeleportHere() => true;

    protected override void Respawn()
    {
        Destroy();
        Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
        {
            Name = "SteamValve",
            SpawnPosition = containerObject.transform.position
        });
    }

    protected override void ToggleObject(bool isActive)
    {
        if (!steamValve) return;

        if (!isActive)
        {
            // Reflection.Invoke(steamValve, "BurstValve");
            Imperium.ObjectManager.BurstSteamValve(objectNetId!.Value);
        }
        else
        {
            steamValve.FixValve();
        }
    }

    public override void Destroy()
    {
        base.Destroy();
        Imperium.ObjectManager.DespawnEntity(objectNetId!.Value);
    }

    protected override void TeleportHere()
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;
        Imperium.ImpPositionIndicator.Activate(position =>
        {
            Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
            {
                Destination = position,
                NetworkId = objectNetId!.Value
            });
        }, origin);
    }

    protected override string GetObjectName() => $"Steam Valve <i>{component.GetInstanceID()}</i>";

    protected override void InitEntry()
    {
        steamValve = (SteamValveHazard)component;
    }

    private void Update()
    {
        if (!steamValve.valveHasBeenRepaired && steamValve.valveHasBurst && IsObjectActive.Value)
        {
            IsObjectActive.Set(false);
        }
        else if (steamValve.valveHasBeenRepaired && !IsObjectActive.Value)
        {
            IsObjectActive.Set(true);
        }
    }
}
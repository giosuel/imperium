#region

using System;
using System.Linq;
using GameNetcodeStuff;
using Imperium.API.Types.Networking;
using Imperium.Core.Lifecycle;
using Imperium.Core.Scripts.Tags;
using Imperium.Interface.Common;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal static class ObjectEntryActions
{
    internal static bool CanDestroy(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player => false,
        ObjectType.Vehicle when entry.component is VehicleController
        {
            currentPassenger: not null,
            currentDriver: not null
        } => false,
        _ => true
    };

    internal static bool CanRespawn(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.BreakerBox => false,
        ObjectType.Item => false,
        ObjectType.Vent => false,
        ObjectType.Player => false,
        ObjectType.SteamValve => false,
        ObjectType.SpiderWeb => false,
        ObjectType.SecurityDoor => false,
        ObjectType.VainShroud => false,
        ObjectType.StoryLog => false,
        ObjectType.OutsideObject => false,
        _ => true
    };

    internal static bool CanDrop(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Item => true,
        _ => false
    };

    internal static bool CanUnlock(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.StoryLog => true,
        _ => false
    };

    internal static bool CanKill(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player when entry.component is PlayerControllerB { isPlayerDead: false } => true,
        _ => false
    };

    internal static bool CanRevive(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player when entry.component is PlayerControllerB { isPlayerDead: true } => true,
        _ => false
    };

    internal static bool CanToggle(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player => false,
        ObjectType.Vehicle => false,
        ObjectType.Item => false,
        ObjectType.SpiderWeb => false,
        ObjectType.Vent => false,
        ObjectType.StoryLog => false,
        ObjectType.VainShroud => false,
        ObjectType.OutsideObject => false,
        _ when entry.netObj.HasValue => true,
        _ => false,
    };

    internal static bool DespawnObject(ObjectEntry entry, bool isRespawn = false)
    {
        switch (entry.Type)
        {
            case ObjectType.Entity:
                Imperium.ObjectManager.DespawnEntity(new EntityDespawnRequest
                {
                    EntityNetObj = entry.netObj!.Value,
                    IsRespawn = isRespawn
                });
                return true;
            case ObjectType.Item:
                Imperium.ObjectManager.DespawnItem(entry.netObj!.Value);
                break;
            case ObjectType.OutsideObject:
                Imperium.ObjectManager.DespawnOutsideObject(new OutsideObjectDespawnRequest
                {
                    Position = entry.containerObject.transform.position
                });
                return true;
            case ObjectType.VainShroud:
                Imperium.ObjectManager.DespawnVainShroud(new VainShroudDespawnRequest
                {
                    Position = entry.containerObject.transform.position
                });
                return true;
            case ObjectType.Vehicle:
                var cruiser = (VehicleController)entry.component;
                if (cruiser.currentDriver || cruiser.currentPassenger) return false;
                Imperium.ObjectManager.DespawnVehicle(new VehicleDespawnRequest
                {
                    VehicleNetObj = entry.netObj!.Value,
                    IsRespawn = isRespawn
                });
                return true;
            case ObjectType.SpiderWeb:
                var spiderWeb = (SandSpiderWebTrap)entry.component;
                Imperium.ObjectManager.DespawnSpiderWeb(new SpiderWebDespawnRequest
                {
                    SpiderNetObj = spiderWeb.mainScript.GetComponent<NetworkObject>(),
                    TrapId = spiderWeb.trapID
                });
                break;
            case ObjectType.BreakerBox:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
            case ObjectType.StoryLog:
            case ObjectType.SecurityDoor:
                Imperium.ObjectManager.DespawnObstacle(entry.netObj!.Value);
                return true;
            case ObjectType.Player:
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    internal static void RespawnObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Vehicle:
                var vehicle = (VehicleController)entry.component;
                var buyableVehicle = Imperium.ObjectManager.LoadedVehicles.Value.Values.FirstOrDefault(vhc => vhc
                    .vehiclePrefab
                    .GetComponent<VehicleController>()?
                    .vehicleID.Equals(vehicle.vehicleID) ?? false
                );

                if (buyableVehicle == null)
                {
                    Imperium.IO.LogError($"[SPAWN] Failed to find vehicle to respawn by ID. ID: {vehicle.vehicleID}");
                    return;
                }

                if (DespawnObject(entry, isRespawn: true))
                {
                    Imperium.ObjectManager.SpawnVehicle(new VehicleSpawnRequest
                    {
                        Name = buyableVehicle.vehicleDisplayName,
                        SpawnPosition = entry.containerObject.transform.position
                    });
                }

                break;
            case ObjectType.Landmine:
                if (DespawnObject(entry, isRespawn: true))
                {
                    Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                    {
                        Name = "Landmine",
                        SpawnPosition = entry.containerObject.transform.position
                    });
                }
                break;
            case ObjectType.Turret:
                if (DespawnObject(entry, isRespawn: true))
                {
                    Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                    {
                        Name = "TurretContainer",
                        SpawnPosition = entry.containerObject.transform.position
                    });
                }
                break;
            case ObjectType.SpikeTrap:
                if (DespawnObject(entry, isRespawn: true))
                {
                    Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                    {
                        Name = "Spike Trap",
                        SpawnPosition = entry.containerObject.transform.position
                    });
                }
                break;
            case ObjectType.Entity:
                if (DespawnObject(entry, isRespawn: true))
                {
                    var entityType = ((EnemyAI)entry.component).enemyType;
                    Imperium.ObjectManager.SpawnEntity(new EntitySpawnRequest
                    {
                        Name = entityType.enemyName,
                        SpawnPosition = entry.containerObject.transform.position
                    });
                }
                break;
            case ObjectType.Vent:
            case ObjectType.SteamValve:
            case ObjectType.SpiderWeb:
            case ObjectType.Player:
            case ObjectType.BreakerBox:
            case ObjectType.Item:
            case ObjectType.SecurityDoor:
            case ObjectType.OutsideObject:
            case ObjectType.VainShroud:
            case ObjectType.StoryLog:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void DropObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Item when entry.component is GrabbableObject item:
                if (!item.isHeld || item.playerHeldBy is null) return;

                Imperium.PlayerManager.DropItem(new DropItemRequest
                {
                    PlayerId = item.playerHeldBy.playerClientId,
                    ItemIndex = PlayerManager.GetItemHolderSlot(item)
                });
                break;
            case ObjectType.Vehicle:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.Player:
            case ObjectType.BreakerBox:
            case ObjectType.SecurityDoor:
            case ObjectType.OutsideObject:
            case ObjectType.VainShroud:
            case ObjectType.StoryLog:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void UnlockObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.StoryLog:
                var storyLog = (StoryLog)entry.component;
                storyLog.CollectLog();
                break;
            case ObjectType.BreakerBox:
            case ObjectType.Vehicle:
            case ObjectType.Entity:
            case ObjectType.Landmine:
            case ObjectType.Player:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.SecurityDoor:
            case ObjectType.OutsideObject:
            case ObjectType.VainShroud:
            case ObjectType.Turret:
            case ObjectType.Vent:
            case ObjectType.Item:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void KillObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Player when entry.component is PlayerControllerB { isPlayerDead: false } player:
                Imperium.PlayerManager.KillPlayer(player.playerClientId);
                break;
            case ObjectType.Vehicle:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.Item:
            case ObjectType.BreakerBox:
            case ObjectType.SecurityDoor:
            case ObjectType.OutsideObject:
            case ObjectType.VainShroud:
            case ObjectType.StoryLog:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void ReviveObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Player when entry.component is PlayerControllerB { isPlayerDead: true } player:
                Imperium.PlayerManager.RevivePlayer(player.playerClientId);
                break;
            case ObjectType.Vehicle:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.Item:
            case ObjectType.BreakerBox:
            case ObjectType.SecurityDoor:
            case ObjectType.OutsideObject:
            case ObjectType.VainShroud:
            case ObjectType.StoryLog:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void ToggleObject(ObjectEntry entry, bool isActive)
    {
        switch (entry.Type)
        {
            case ObjectType.Landmine:
                ((Landmine)entry.component).ToggleMine(isActive);
                break;
            case ObjectType.Turret:
            case ObjectType.SteamValve:
                if (!isActive)
                {
                    Imperium.ObjectManager.BurstSteamValve(entry.netObj!.Value);
                }
                else
                {
                    ((SteamValveHazard)entry.component).FixValve();
                }

                break;
            case ObjectType.Entity:
                var entity = (EnemyAI)entry.component;
                entity.enabled = isActive;
                entity.agent.isStopped = !isActive;
                if (entity.creatureAnimator) entity.creatureAnimator.enabled = isActive;
                break;
            case ObjectType.BreakerBox:
                MoonManager.ToggleBreaker((BreakerBox)entry.component, isActive);
                break;
            case ObjectType.SecurityDoor:
                ((TerminalAccessibleObject)entry.component).SetDoorToggleLocalClient();
                break;
            case ObjectType.SpikeTrap:
                ((SpikeRoofTrap)entry.component).slamOnIntervals = isActive;
                break;
            case ObjectType.StoryLog:
            case ObjectType.SpiderWeb:
            case ObjectType.Player:
            case ObjectType.Vehicle:
            case ObjectType.Item:
            case ObjectType.Vent:
            case ObjectType.OutsideObject:
            case ObjectType.VainShroud:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void TeleportObjectHere(ObjectEntry entry)
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;

        switch (entry.Type)
        {
            case ObjectType.Vehicle:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
                    {
                        Destination = position + Vector3.up * 5f,
                        NetworkObj = entry.netObj!.Value
                    });
                }, origin, castGround: true);
                break;
            case ObjectType.Player:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.PlayerManager.TeleportPlayer(new TeleportPlayerRequest
                    {
                        PlayerId = ((PlayerControllerB)entry.component).playerClientId,
                        Destination = position
                    });
                }, Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null, castGround: true);
                Imperium.Interface.Close();
                break;
            case ObjectType.VainShroud:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportVainShroud(new VainShroudTeleportRequest
                    {
                        Position = entry.containerObject.transform.position,
                        Destination = position
                    });
                }, origin, castGround: true);
                break;
            case ObjectType.OutsideObject:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportOutsideObject(new OutsideObjectTeleportRequest
                    {
                        Position = entry.containerObject.transform.position,
                        Destination = position
                    });
                }, origin, castGround: true);
                break;
            case ObjectType.SpiderWeb:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    var spiderWeb = (SandSpiderWebTrap)entry.component;
                    Imperium.ObjectManager.TeleportSpiderWeb(new SpiderWebTeleportRequest
                    {
                        SpiderNetObj = spiderWeb.mainScript.GetComponent<NetworkObject>(),
                        TrapId = spiderWeb.trapID,
                        Position = position
                    });
                }, origin, castGround: true);
                break;
            case ObjectType.Entity:
            case ObjectType.BreakerBox:
            case ObjectType.Item:
            case ObjectType.Landmine:
            case ObjectType.SpikeTrap:
            case ObjectType.Turret:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
            case ObjectType.SecurityDoor:
            case ObjectType.StoryLog:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
                    {
                        Destination = position,
                        NetworkObj = entry.netObj!.Value
                    });
                }, origin, castGround: false);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void IntervalUpdate(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.SteamValve:
                var steamValve = (SteamValveHazard)entry.component;
                switch (steamValve.valveHasBeenRepaired)
                {
                    case false when steamValve.valveHasBurst && entry.IsObjectActive.Value:
                        entry.IsObjectActive.Set(false);
                        break;
                    case true when !entry.IsObjectActive.Value:
                        entry.IsObjectActive.Set(true);
                        break;
                }

                break;
            case ObjectType.Item:
                var item = (GrabbableObject)entry.component;
                var isHeld = item.isHeld || item.heldByPlayerOnServer;

                if (entry.dropButton.interactable != isHeld)
                {
                    ImpButton.ToggleButton(entry.dropButton, isHeld);
                    ImpButton.ToggleButton(entry.teleportHereButton, !isHeld);
                }
                break;
            case ObjectType.StoryLog:
                var storyLog = (StoryLog)entry.component;
                entry.unlockButton.interactable = !storyLog.collected;
                break;
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.BreakerBox:
            case ObjectType.SpikeTrap:
            case ObjectType.SpiderWeb:
            case ObjectType.Player:
            case ObjectType.Vehicle:
            case ObjectType.VainShroud:
            case ObjectType.SecurityDoor:
            case ObjectType.OutsideObject:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void InitObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.SpikeTrap:
                entry.activeToggle.Set(((SpikeRoofTrap)entry.component).slamOnIntervals);
                entry.activeToggle.gameObject.AddComponent<ImpTooltipTrigger>().Init(new TooltipDefinition
                {
                    Title = "Spike Trap Toggle",
                    Description = "On - Slam on Intervals\nOff - Slam on Motion",
                    Tooltip = entry.tooltip
                });
                break;
            case ObjectType.StoryLog:
            case ObjectType.SteamValve:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.BreakerBox:
            case ObjectType.SpiderWeb:
            case ObjectType.Player:
            case ObjectType.Vehicle:
            case ObjectType.Item:
            case ObjectType.VainShroud:
            case ObjectType.SecurityDoor:
            case ObjectType.OutsideObject:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static string GetObjectName(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.BreakerBox => GetObjectGenericName("Breaker Box", entry.component),
        ObjectType.Vehicle => GetVehicleName(entry.component),
        ObjectType.Entity => GetEntityName((EnemyAI)entry.component),
        ObjectType.Item => ((GrabbableObject)entry.component).itemProperties.itemName,
        ObjectType.Landmine => GetTerminalAccessibleName("Landmine", entry.component),
        ObjectType.Player => GetPlayerName((PlayerControllerB)entry.component),
        ObjectType.SpiderWeb => GetObjectGenericName("Spider Web", entry.component),
        ObjectType.SpikeTrap => GetObjectGenericName("Spike Trap", entry.component),
        ObjectType.SteamValve => GetObjectGenericName("Steam Valve", entry.component),
        ObjectType.Turret => GetTerminalAccessibleName("Turret", entry.component),
        ObjectType.SecurityDoor => GetTerminalAccessibleName("Security Door", entry.component),
        ObjectType.OutsideObject => GetOutsideObjectName(entry.component.gameObject),
        ObjectType.Vent => GetVentName((EnemyVent)entry.component),
        ObjectType.StoryLog => GetStoryLogName((StoryLog)entry.component),
        ObjectType.VainShroud => $"Mold Spore (<i>ID: {entry.component.GetInstanceID()}</i>)",
        _ => entry.component.gameObject.name
    };

    internal static Vector3 GetTeleportPosition(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Vent => ((EnemyVent)entry.component).floorNode.position,
        _ => entry.containerObject.transform.position
    };

    internal static GameObject GetContainerObject(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Landmine or ObjectType.Turret => entry.component.transform.parent.gameObject,
        ObjectType.SpikeTrap => entry.component.transform.parent.parent.parent.gameObject,
        _ => entry.component.gameObject
    };

    private static string GetVehicleName(Component obj)
    {
        var vehicle = (VehicleController)obj;
        var buyableVehicle = Imperium.ObjectManager.LoadedVehicles.Value.Values.FirstOrDefault(vhc => vhc
            .vehiclePrefab
            .GetComponent<VehicleController>()?
            .vehicleID.Equals(vehicle.vehicleID) ?? false
        );

        return buyableVehicle == null
            ? $"Unknown Vehicle (ID: {RichText.Size(obj.GetInstanceID().ToString(), 10)})"
            : buyableVehicle.vehicleDisplayName;
    }

    private static string GetObjectGenericName(string name, Component obj)
    {
        return $"{name} (ID: {RichText.Size(obj.GetInstanceID().ToString(), 10)})";
    }

    private static string GetTerminalAccessibleName(string name, Component obj)
    {
        string code = "??";
        if (obj.TryGetComponent<TerminalAccessibleObject>(out var terminalComponent))
        {
            code = terminalComponent.objectCode;
        }
        return $"{name} [{code}] (ID: {RichText.Size(obj.GetInstanceID().ToString(), 10)})";
    }

    private static string GetOutsideObjectName(GameObject obj)
    {
        var displayName = Imperium.ObjectManager.GetOverrideDisplayName(obj.name) ?? obj.name.Replace("(Clone)", "");
        return $"{displayName} (ID: {RichText.Size(obj.GetInstanceID().ToString(), 10)})";
    }

    private static string GetVentName(EnemyVent vent)
    {
        if (vent.occupied && vent.enemyType)
        {
            return $"Vent <i>{vent.GetInstanceID()}</i> ({vent.enemyType.enemyName})";
        }

        return $"Vent <i>{vent.GetInstanceID()}</i> (Empty)";
    }

    private static string GetEntityName(EnemyAI entity)
    {
        var personalName = $"({Imperium.ObjectManager.GetEntityName(entity)})";
        var entityName = $"{entity.enemyType.enemyName} {RichText.Size(personalName, 10)}";
        return entity.isEnemyDead ? RichText.Strikethrough(entityName) : entityName;
    }

    private static string GetStoryLogName(StoryLog storyLog)
    {
        return Imperium.Terminal.logEntryFiles[storyLog.storyLogID].creatureName;
    }

    private static string GetPlayerName(PlayerControllerB player)
    {
        var playerName = player.playerUsername;
        if (string.IsNullOrEmpty(playerName)) playerName = $"Player {player.GetInstanceID()}";

        // Check if player is also using Imperium
        if (Imperium.Networking.ImperiumUsers.Value.Contains(player.playerClientId))
        {
            playerName = $"[I] {playerName}";
        }

        if (player.isPlayerControlled)
        {
            if (player.isInHangarShipRoom)
            {
                playerName += " (In Ship)";
            }
            else if (player.isInsideFactory)
            {
                playerName += " (Indoors)";
            }
            else
            {
                playerName += " (Outdoors)";
            }
        }

        if (player.playerClientId == NetworkManager.ServerClientId)
        {
            playerName = RichText.Bold(playerName);
        }

        return player.isPlayerDead ? RichText.Strikethrough(playerName) : playerName;
    }
}
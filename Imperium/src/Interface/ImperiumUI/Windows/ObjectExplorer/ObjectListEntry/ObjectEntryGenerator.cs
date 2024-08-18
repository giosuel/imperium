#region

using System;
using GameNetcodeStuff;
using Imperium.API.Types.Networking;
using Imperium.Core.Lifecycle;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal static class ObjectEntryGenerator
{
    internal static bool CanDestroy(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player => false,
        ObjectType.Cruiser when entry.component is VehicleController
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
        ObjectType.VainShroud => false,
        ObjectType.OutsideObject => false,
        _ => true
    };

    internal static bool CanDrop(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Item => true,
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
        ObjectType.Cruiser => false,
        ObjectType.Player => false,
        ObjectType.Item => false,
        ObjectType.SpiderWeb => false,
        ObjectType.SpikeTrap => false,
        ObjectType.VainShroud => false,
        ObjectType.OutsideObject => false,
        _ => true
    };

    internal static void DespawnObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Entity:
                Imperium.ObjectManager.DespawnEntity(entry.objectNetId!.Value);
                break;
            case ObjectType.Item:
                Imperium.ObjectManager.DespawnItem(entry.objectNetId!.Value);
                break;
            case ObjectType.VainShroud:
                Imperium.ObjectManager.DespawnLocalObject(new LocalObjectDespawnRequest
                {
                    Type = LocalObjectType.VainShroud,
                    Position = entry.containerObject.transform.position
                });
                break;
            case ObjectType.OutsideObject:
                Imperium.ObjectManager.DespawnLocalObject(new LocalObjectDespawnRequest
                {
                    Type = LocalObjectType.OutsideObject,
                    Position = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Cruiser:
                var cruiser = (VehicleController)entry.component;
                if (cruiser.currentDriver || cruiser.currentPassenger) return;
                Imperium.ObjectManager.DespawnObstacle(entry.objectNetId!.Value);
                break;
            case ObjectType.Player:
                break;
            case ObjectType.BreakerBox:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
                Imperium.ObjectManager.DespawnObstacle(entry.objectNetId!.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        entry.forceDelayedUpdate.Invoke();
    }

    internal static void RespawnObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Cruiser:
                DespawnObject(entry);
                Imperium.ObjectManager.SpawnCompanyCruiser(new CompanyCruiserSpawnRequest
                {
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Landmine:
                DespawnObject(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Landmine",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Turret:
                DespawnObject(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Turret",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.SpiderWeb:
                DespawnObject(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "SpiderWeb",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.SpikeTrap:
                DespawnObject(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Spike Trap",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Entity:
                DespawnObject(entry);
                var entity = (EnemyAI)entry.component;
                Imperium.ObjectManager.SpawnEntity(new EntitySpawnRequest
                {
                    Name = entity.enemyType.enemyName,
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Vent:
            case ObjectType.SteamValve:
            case ObjectType.Player:
            case ObjectType.BreakerBox:
            case ObjectType.Item:
            case ObjectType.VainShroud:
            case ObjectType.OutsideObject:
                break;
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
            case ObjectType.Cruiser:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.VainShroud:
            case ObjectType.Player:
            case ObjectType.BreakerBox:
            case ObjectType.OutsideObject:
                break;
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
            case ObjectType.Cruiser:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.VainShroud:
            case ObjectType.Item:
            case ObjectType.BreakerBox:
            case ObjectType.OutsideObject:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        entry.forceDelayedUpdate.Invoke();
    }

    internal static void ReviveObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Player when entry.component is PlayerControllerB { isPlayerDead: true } player:
                Imperium.PlayerManager.RevivePlayer(player.playerClientId);
                break;
            case ObjectType.Cruiser:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.VainShroud:
            case ObjectType.Item:
            case ObjectType.BreakerBox:
            case ObjectType.OutsideObject:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        entry.forceDelayedUpdate.Invoke();
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
                    Imperium.ObjectManager.BurstSteamValve(entry.objectNetId!.Value);
                }
                else
                {
                    ((SteamValveHazard)entry.component).FixValve();
                }

                break;
            case ObjectType.Vent:
            case ObjectType.Entity:
                var entity = (EnemyAI)entry.component;
                entity.enabled = isActive;
                entity.agent.isStopped = !isActive;
                if (entity.creatureAnimator) entity.creatureAnimator.enabled = isActive;
                break;
            case ObjectType.BreakerBox:
                MoonManager.ToggleBreaker((BreakerBox)entry.component, isActive);
                break;
            case ObjectType.SpikeTrap:
            case ObjectType.SpiderWeb:
            case ObjectType.VainShroud:
            case ObjectType.Player:
            case ObjectType.Cruiser:
            case ObjectType.Item:
            case ObjectType.OutsideObject:
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
            case ObjectType.Cruiser:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
                    {
                        Destination = position + Vector3.up * 5f,
                        NetworkId = entry.objectNetId!.Value
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
                    Imperium.ObjectManager.TeleportLocalObject(new LocalObjectTeleportRequest
                    {
                        Type = LocalObjectType.VainShroud,
                        Position = entry.containerObject.transform.position,
                        Destination = position
                    });
                }, origin, castGround: true);
                break;
            case ObjectType.OutsideObject:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportLocalObject(new LocalObjectTeleportRequest
                    {
                        Type = LocalObjectType.OutsideObject,
                        Position = entry.containerObject.transform.position,
                        Destination = position
                    });
                }, origin, castGround: true);
                break;
            case ObjectType.Entity:
            case ObjectType.BreakerBox:
            case ObjectType.Item:
            case ObjectType.Landmine:
            case ObjectType.SpikeTrap:
            case ObjectType.SpiderWeb:
            case ObjectType.Turret:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
                    {
                        Destination = position,
                        NetworkId = entry.objectNetId!.Value
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
                entry.dropButton.interactable = isHeld;
                entry.teleportHereButton.interactable = !isHeld;
                break;
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.BreakerBox:
            case ObjectType.SpikeTrap:
            case ObjectType.SpiderWeb:
            case ObjectType.VainShroud:
            case ObjectType.Player:
            case ObjectType.Cruiser:
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
            case ObjectType.VainShroud:
            case ObjectType.SteamValve:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.Vent:
            case ObjectType.Entity:
            case ObjectType.BreakerBox:
            case ObjectType.SpikeTrap:
            case ObjectType.SpiderWeb:
            case ObjectType.Player:
            case ObjectType.Cruiser:
            case ObjectType.Item:
            case ObjectType.OutsideObject:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static string GetObjectName(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.BreakerBox => $"Breaker Box (<i>ID: {entry.component.GetInstanceID()})</i>",
        ObjectType.Cruiser => $"Cruiser (<i>ID: {entry.component.GetInstanceID()})</i>",
        ObjectType.Entity => GetEntityName((EnemyAI)entry.component),
        ObjectType.Item => ((GrabbableObject)entry.component).itemProperties.itemName,
        ObjectType.Landmine => $"Landmine (<i>ID: {entry.component.GetInstanceID()})</i>",
        ObjectType.VainShroud => $"Mold Spore (<i>ID: {entry.component.GetInstanceID()})</i>",
        ObjectType.Player => GetPlayerName((PlayerControllerB)entry.component),
        ObjectType.SpiderWeb => $"Spider Web (<i>ID: {entry.component.GetInstanceID()})</i>",
        ObjectType.SpikeTrap => $"Spike Trap (<i>ID: {entry.component.GetInstanceID()})</i>",
        ObjectType.SteamValve => $"Steam Valve (<i>ID: {entry.component.GetInstanceID()})</i>",
        ObjectType.Turret => $"Turret (<i>ID: {entry.component.GetInstanceID()})</i>",
        ObjectType.OutsideObject => GetOutsideObjectName(entry.component.gameObject),
        ObjectType.Vent => GetVentName((EnemyVent)entry.component),
        _ => throw new ArgumentOutOfRangeException()
    };

    internal static Vector3 GetTeleportPosition(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Vent => ((EnemyVent)entry.component).floorNode.position,
        _ => entry.containerObject.transform.position
    };

    internal static GameObject GetContainerObject(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Landmine => entry.component.transform.parent.gameObject,
        ObjectType.Turret => entry.component.transform.parent.gameObject,
        _ => entry.component.gameObject
    };

    private static string GetOutsideObjectName(GameObject obj)
    {
        return Imperium.ObjectManager.GetOverrideDisplayName(obj.name) ?? obj.name;
    }

    private static string GetVentName(EnemyVent vent)
    {
        if (vent.occupied && vent.enemyType)
        {
            return $"Vent <i>{vent.GetInstanceID()}</i> ({vent.enemyType.enemyName})";
        }

        return $"Vent <i>{vent.GetInstanceID()}</i>";
    }

    private static string GetEntityName(EnemyAI entity)
    {
        var personalName = $"({Imperium.ObjectManager.GetEntityName(entity)})";
        var entityName = $"{entity.enemyType.enemyName} {RichText.Size(personalName, 10)}";
        return entity.isEnemyDead ? RichText.Strikethrough(entityName) : entityName;
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
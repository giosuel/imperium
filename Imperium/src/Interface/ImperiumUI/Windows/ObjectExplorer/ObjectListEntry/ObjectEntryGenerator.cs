using System;
using GameNetcodeStuff;
using Imperium.API.Types.Networking;
using Imperium.Core.Lifecycle;
using Imperium.Interface.Common;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal static class ObjectEntryGenerator
{
    internal static bool CanDestroy(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player => false,
        _ => true
    };

    internal static bool CanRespawn(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.BreakerBox => false,
        ObjectType.Item => false,
        ObjectType.Vent => false,
        ObjectType.VainShroud => false,
        ObjectType.Player => false,
        ObjectType.SteamValve => false,
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
        ObjectType.VainShroud => false,
        ObjectType.SpiderWeb => false,
        ObjectType.SpikeTrap => false,
        _ => true
    };

    internal static void DestroyObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.BreakerBox:
            case ObjectType.Cruiser:
            case ObjectType.Landmine:
            case ObjectType.Turret:
            case ObjectType.SpiderWeb:
            case ObjectType.SpikeTrap:
            case ObjectType.SteamValve:
            case ObjectType.Vent:
                Imperium.ObjectManager.DespawnObstacle(entry.objectNetId!.Value);
                break;
            case ObjectType.Entity:
                Imperium.ObjectManager.DespawnEntity(entry.objectNetId!.Value);
                break;
            case ObjectType.Item:
                Imperium.ObjectManager.DespawnItem(entry.objectNetId!.Value);
                break;
            case ObjectType.VainShroud:
                if (Imperium.ObjectManager.StaticPrefabLookupMap.TryGetValue(entry.containerObject, out var moldObject))
                {
                    Imperium.ObjectManager.DespawnObstacle(moldObject);
                }

                break;
            case ObjectType.Player:
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
                DestroyObject(entry);
                Imperium.ObjectManager.SpawmCompanyCruiser(new CompanyCruiserSpawnRequest
                {
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Landmine:
                DestroyObject(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Landmine",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Turret:
                DestroyObject(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Turret",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.SpiderWeb:
                DestroyObject(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "SpiderWeb",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.SpikeTrap:
                DestroyObject(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Spike Trap",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Entity:
                DestroyObject(entry);
                var entity = (EnemyAI)entry.component;
                Imperium.ObjectManager.SpawnEntity(new EntitySpawnRequest
                {
                    Name = entity.enemyType.enemyName,
                    PrefabName = entity.enemyType.enemyPrefab.name,
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectType.Vent:
            case ObjectType.SteamValve:
            case ObjectType.Player:
            case ObjectType.VainShroud:
            case ObjectType.BreakerBox:
            case ObjectType.Item:
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
                }, origin);
                break;
            case ObjectType.Player:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.PlayerManager.TeleportPlayer(new TeleportPlayerRequest
                    {
                        PlayerId = ((PlayerControllerB)entry.component).playerClientId,
                        Destination = position
                    });
                }, Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null);
                Imperium.Interface.Close();
                break;
            case ObjectType.Entity:
            case ObjectType.BreakerBox:
            case ObjectType.Item:
            case ObjectType.Landmine:
            case ObjectType.VainShroud:
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
                InitVainShroud(entry);
                break;
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
                entry.destroyButton.interactable = true;
                entry.teleportHereButton.interactable = true;
                if (entry.destroyButton.TryGetComponent<ImpInteractable>(out var destroyInteractable))
                {
                    Object.Destroy(destroyInteractable);
                }

                if (entry.teleportHereButton.TryGetComponent<ImpInteractable>(out var teleportInteractable))
                {
                    Object.Destroy(teleportInteractable);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static string GetObjectName(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.BreakerBox => $"Breaker Box <i>{entry.component.GetInstanceID()}</i>",
        ObjectType.Cruiser => $"Cruiser <i>{entry.component.GetInstanceID()}</i>",
        ObjectType.Entity => GetEntityName((EnemyAI)entry.component),
        ObjectType.Item => ((GrabbableObject)entry.component).itemProperties.itemName,
        ObjectType.Landmine => $"Landmine <i>{entry.component.GetInstanceID()}</i>",
        ObjectType.VainShroud => $"Mold Spore <i>{entry.component.GetInstanceID()}</i>",
        ObjectType.Player => GetPlayerName((PlayerControllerB)entry.component),
        ObjectType.SpiderWeb => $"Spider Web <i>{entry.component.GetInstanceID()}</i>",
        ObjectType.SpikeTrap => $"Spike Trap <i>{entry.component.GetInstanceID()}</i>",
        ObjectType.SteamValve => $"Steam Valve <i>{entry.component.GetInstanceID()}</i>",
        ObjectType.Turret => $"Turret <i>{entry.component.GetInstanceID()}</i>",
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

    private static void InitVainShroud(ObjectEntry entry)
    {
        var canModify = Imperium.ObjectManager.StaticPrefabLookupMap.ContainsKey(entry.containerObject);

        if (!canModify && entry.tooltip)
        {
            entry.destroyButton.interactable = false;
            entry.teleportHereButton.interactable = false;

            if (!entry.destroyButton.TryGetComponent<ImpInteractable>(out _))
            {
                var interactable = entry.destroyButton.gameObject.AddComponent<ImpInteractable>();
                interactable.onEnter += () => entry.tooltip.Activate(
                    "Local Object",
                    "Unable to destroy local objects instantiated by the game."
                );
                interactable.onExit += () => entry.tooltip.Deactivate();
                interactable.onOver += position => entry.tooltip.UpdatePosition(position);
            }

            if (!entry.teleportHereButton.TryGetComponent<ImpInteractable>(out _))
            {
                var interactable = entry.teleportHereButton.gameObject.AddComponent<ImpInteractable>();
                interactable.onEnter += () => entry.tooltip.Activate(
                    "Local Object",
                    "Unable to teleport local objects instantiated by the game."
                );
                interactable.onExit += () => entry.tooltip.Deactivate();
                interactable.onOver += position => entry.tooltip.UpdatePosition(position);
            }
        }
    }
}
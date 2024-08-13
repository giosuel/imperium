using System;
using GameNetcodeStuff;
using Imperium.API.Types.Networking;
using Imperium.Core.Lifecycle;
using Imperium.Interface.Common;
using Imperium.Util;
using Unity.Netcode;
using UnityEngine;

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal static class DynamicObjectEntryTypeHelper
{
    internal static bool CanDestroy(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.Player => false,
        _ => true
    };

    internal static bool CanRespawn(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.BreakerBox => false,
        ObjectEntryType.Item => false,
        ObjectEntryType.Vent => false,
        ObjectEntryType.MoldSpore => false,
        ObjectEntryType.Player => false,
        ObjectEntryType.SteamValve => false,
        _ => true
    };

    internal static bool CanDrop(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.Item => true,
        _ => false
    };

    internal static bool CanKill(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.Player when entry.component is PlayerControllerB { isPlayerDead: false } => true,
        _ => false
    };

    internal static bool CanRevive(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.Player when entry.component is PlayerControllerB { isPlayerDead: true } => true,
        _ => false
    };

    internal static bool CanToggle(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.CompanyCruiser => false,
        ObjectEntryType.Player => false,
        ObjectEntryType.Item => false,
        ObjectEntryType.MoldSpore => false,
        ObjectEntryType.SpiderWeb => false,
        ObjectEntryType.SpikeTrap => false,
        _ => true
    };

    internal static void Destroy(DynamicObjectEntry entry)
    {
        switch (entry.entryType)
        {
            case ObjectEntryType.BreakerBox:
            case ObjectEntryType.CompanyCruiser:
            case ObjectEntryType.Landmine:
            case ObjectEntryType.Turret:
            case ObjectEntryType.SpiderWeb:
            case ObjectEntryType.SpikeTrap:
            case ObjectEntryType.SteamValve:
            case ObjectEntryType.Vent:
                Imperium.ObjectManager.DespawnObstacle(entry.objectNetId!.Value);
                break;
            case ObjectEntryType.Entity:
                Imperium.ObjectManager.DespawnEntity(entry.objectNetId!.Value);
                break;
            case ObjectEntryType.Item:
                Imperium.ObjectManager.DespawnItem(entry.objectNetId!.Value);
                break;
            case ObjectEntryType.MoldSpore:
                if (Imperium.ObjectManager.StaticPrefabLookupMap.TryGetValue(entry.containerObject, out var moldObject))
                {
                    Imperium.ObjectManager.DespawnObstacle(moldObject);
                }

                break;
            case ObjectEntryType.Player:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void Respawn(DynamicObjectEntry entry)
    {
        switch (entry.entryType)
        {
            case ObjectEntryType.CompanyCruiser:
                Destroy(entry);
                Imperium.ObjectManager.SpawmCompanyCruiser(new CompanyCruiserSpawnRequest
                {
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectEntryType.Landmine:
                Destroy(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Landmine",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectEntryType.Turret:
                Destroy(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Turret",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectEntryType.SpiderWeb:
                Destroy(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "SpiderWeb",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectEntryType.SpikeTrap:
                Destroy(entry);
                Imperium.ObjectManager.SpawnMapHazard(new MapHazardSpawnRequest
                {
                    Name = "Spike Trap",
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectEntryType.Entity:
                Destroy(entry);
                Imperium.ObjectManager.SpawnEntity(new EntitySpawnRequest
                {
                    Name = entry.objectName,
                    PrefabName = ((EnemyAI)entry.component).enemyType.enemyPrefab.name,
                    SpawnPosition = entry.containerObject.transform.position
                });
                break;
            case ObjectEntryType.Vent:
            case ObjectEntryType.SteamValve:
            case ObjectEntryType.Player:
            case ObjectEntryType.MoldSpore:
            case ObjectEntryType.BreakerBox:
            case ObjectEntryType.Item:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void Drop(DynamicObjectEntry entry)
    {
        switch (entry.entryType)
        {
            case ObjectEntryType.Item when entry.component is GrabbableObject item:
                if (!item.isHeld || item.playerHeldBy is null) return;

                Imperium.PlayerManager.DropItem(new DropItemRequest
                {
                    PlayerId = item.playerHeldBy.playerClientId,
                    ItemIndex = PlayerManager.GetItemHolderSlot(item)
                });
                break;
            case ObjectEntryType.CompanyCruiser:
            case ObjectEntryType.Landmine:
            case ObjectEntryType.Turret:
            case ObjectEntryType.SpiderWeb:
            case ObjectEntryType.SpikeTrap:
            case ObjectEntryType.SteamValve:
            case ObjectEntryType.Vent:
            case ObjectEntryType.Entity:
            case ObjectEntryType.MoldSpore:
            case ObjectEntryType.Player:
            case ObjectEntryType.BreakerBox:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void Kill(DynamicObjectEntry entry)
    {
        switch (entry.entryType)
        {
            case ObjectEntryType.Player when entry.component is PlayerControllerB { isPlayerDead: false } player:
                Imperium.PlayerManager.KillPlayer(player.playerClientId);
                break;
            case ObjectEntryType.CompanyCruiser:
            case ObjectEntryType.Landmine:
            case ObjectEntryType.Turret:
            case ObjectEntryType.SpiderWeb:
            case ObjectEntryType.SpikeTrap:
            case ObjectEntryType.SteamValve:
            case ObjectEntryType.Vent:
            case ObjectEntryType.Entity:
            case ObjectEntryType.MoldSpore:
            case ObjectEntryType.Item:
            case ObjectEntryType.BreakerBox:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void Revive(DynamicObjectEntry entry)
    {
        switch (entry.entryType)
        {
            case ObjectEntryType.Player when entry.component is PlayerControllerB { isPlayerDead: true } player:
                Imperium.PlayerManager.RevivePlayer(player.playerClientId);
                break;
            case ObjectEntryType.CompanyCruiser:
            case ObjectEntryType.Landmine:
            case ObjectEntryType.Turret:
            case ObjectEntryType.SpiderWeb:
            case ObjectEntryType.SpikeTrap:
            case ObjectEntryType.SteamValve:
            case ObjectEntryType.Vent:
            case ObjectEntryType.Entity:
            case ObjectEntryType.MoldSpore:
            case ObjectEntryType.Item:
            case ObjectEntryType.BreakerBox:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void ToggleObject(DynamicObjectEntry entry, bool isActive)
    {
        switch (entry.entryType)
        {
            case ObjectEntryType.Landmine:
                ((Landmine)entry.component).ToggleMine(isActive);
                break;
            case ObjectEntryType.Turret:
            case ObjectEntryType.SteamValve:
                if (!isActive)
                {
                    // Reflection.Invoke(steamValve, "BurstValve");
                    Imperium.ObjectManager.BurstSteamValve(entry.objectNetId!.Value);
                }
                else
                {
                    ((SteamValveHazard)entry.component).FixValve();
                }

                break;
            case ObjectEntryType.Vent:
            case ObjectEntryType.Entity:
                var entity = (EnemyAI)entry.component;
                entity.enabled = isActive;
                entity.agent.isStopped = !isActive;
                if (entity.creatureAnimator) entity.creatureAnimator.enabled = isActive;
                break;
            case ObjectEntryType.BreakerBox:
                MoonManager.ToggleBreaker((BreakerBox)entry.component, isActive);
                break;
            case ObjectEntryType.SpikeTrap:
            case ObjectEntryType.SpiderWeb:
            case ObjectEntryType.MoldSpore:
            case ObjectEntryType.Player:
            case ObjectEntryType.CompanyCruiser:
            case ObjectEntryType.Item:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void TeleportHere(DynamicObjectEntry entry)
    {
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;

        switch (entry.entryType)
        {
            case ObjectEntryType.Entity:
            case ObjectEntryType.BreakerBox:
            case ObjectEntryType.Item:
            case ObjectEntryType.Landmine:
            case ObjectEntryType.MoldSpore:
            case ObjectEntryType.SpikeTrap:
            case ObjectEntryType.SpiderWeb:
            case ObjectEntryType.Turret:
            case ObjectEntryType.SteamValve:
            case ObjectEntryType.Vent:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
                    {
                        Destination = position,
                        NetworkId = entry.objectNetId!.Value
                    });
                }, origin, castGround: false);
                break;
            case ObjectEntryType.CompanyCruiser:
                Imperium.ImpPositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportObject(new ObjectTeleportRequest
                    {
                        Destination = position + Vector3.up * 5f,
                        NetworkId = entry.objectNetId!.Value
                    });
                }, origin);
                break;
            case ObjectEntryType.Player:
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
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void Update(DynamicObjectEntry entry)
    {
        switch (entry.entryType)
        {
            case ObjectEntryType.SteamValve:
                var steamValve = ((SteamValveHazard)entry.component);
                if (!steamValve.valveHasBeenRepaired && steamValve.valveHasBurst && entry.IsObjectActive.Value)
                {
                    entry.IsObjectActive.Set(false);
                }
                else if (steamValve.valveHasBeenRepaired && !entry.IsObjectActive.Value)
                {
                    entry.IsObjectActive.Set(true);
                }

                break;
            case ObjectEntryType.Landmine:
            case ObjectEntryType.Turret:
            case ObjectEntryType.Vent:
            case ObjectEntryType.Entity:
            case ObjectEntryType.BreakerBox:
            case ObjectEntryType.SpikeTrap:
            case ObjectEntryType.SpiderWeb:
            case ObjectEntryType.MoldSpore:
            case ObjectEntryType.Player:
            case ObjectEntryType.CompanyCruiser:
            case ObjectEntryType.Item:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void InitObject(DynamicObjectEntry entry)
    {
        switch (entry.entryType)
        {
            case ObjectEntryType.MoldSpore:
                InitMoldSpore(entry);
                break;
            case ObjectEntryType.SteamValve:
            case ObjectEntryType.Landmine:
            case ObjectEntryType.Turret:
            case ObjectEntryType.Vent:
            case ObjectEntryType.Entity:
            case ObjectEntryType.BreakerBox:
            case ObjectEntryType.SpikeTrap:
            case ObjectEntryType.SpiderWeb:
            case ObjectEntryType.Player:
            case ObjectEntryType.CompanyCruiser:
            case ObjectEntryType.Item:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static string GetObjectName(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.BreakerBox => $"Breaker Box <i>{entry.component.GetInstanceID()}</i>",
        ObjectEntryType.CompanyCruiser => $"Cruiser <i>{entry.component.GetInstanceID()}</i>",
        ObjectEntryType.Entity => GetEntityName((EnemyAI)entry.component),
        ObjectEntryType.Item => ((GrabbableObject)entry.component).itemProperties.itemName,
        ObjectEntryType.Landmine => $"Landmine <i>{entry.component.GetInstanceID()}</i>",
        ObjectEntryType.MoldSpore => $"Mold Spore <i>{entry.component.GetInstanceID()}</i>",
        ObjectEntryType.Player => GetPlayerName((PlayerControllerB)entry.component),
        ObjectEntryType.SpiderWeb => $"Spider Web <i>{entry.component.GetInstanceID()}</i>",
        ObjectEntryType.SpikeTrap => $"Spike Trap <i>{entry.component.GetInstanceID()}</i>",
        ObjectEntryType.SteamValve => $"Steam Valve <i>{entry.component.GetInstanceID()}</i>",
        ObjectEntryType.Turret => $"Turret <i>{entry.component.GetInstanceID()}</i>",
        ObjectEntryType.Vent => GetVentName((EnemyVent)entry.component),
        _ => throw new ArgumentOutOfRangeException()
    };

    internal static Vector3 GetTeleportPosition(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.Vent => ((EnemyVent)entry.component).floorNode.position,
        _ => entry.containerObject.transform.position
    };

    internal static GameObject GetContainerObject(DynamicObjectEntry entry) => entry.entryType switch
    {
        ObjectEntryType.Landmine => entry.component.transform.parent.gameObject,
        ObjectEntryType.Turret => entry.component.transform.parent.gameObject,
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

    private static void InitMoldSpore(DynamicObjectEntry entry)
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

internal enum ObjectEntryType
{
    BreakerBox,
    CompanyCruiser,
    Entity,
    Item,
    Landmine,
    MoldSpore,
    Player,
    SpiderWeb,
    SpikeTrap,
    SteamValve,
    Turret,
    Vent
}
#region

using System;
using System.Collections.Generic;
using GameNetcodeStuff;
using Imperium.API.Types.Networking;
using Imperium.Netcode;
using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Core.Lifecycle;

internal class PlayerManager : ImpLifecycleObject
{
    internal readonly ImpBinaryBinding IsFlying = new(false);

    private readonly ImpNetMessage<ulong> killPlayerMessage = new("KillPlayer");
    private readonly ImpNetMessage<ulong> revivePlayerMessage = new("RevivePlayer");
    private readonly ImpNetMessage<TeleportPlayerRequest> teleportPlayerMessage = new("TeleportPlayer");
    private readonly ImpNetMessage<DropItemRequest> dropItemMessage = new("Dropitem");

    private static readonly Dictionary<int, Vector2> CameraOriginalResolutions = [];

    internal readonly ImpExternalBinding<Vector3?, bool> ShipTPAnchor = new(
        () => GameObject.Find("CatwalkShip")?.transform.position
    );

    internal readonly ImpExternalBinding<Vector3?, bool> MainEntranceTPAnchor = new(
        () => GameObject.Find("EntranceTeleportA")?.transform.position
    );

    internal readonly ImpExternalBinding<Vector3?, bool> ApparatusTPAnchor = new(
        () => GameObject.Find("LungApparatus(Clone)")?.transform.position
    );

    internal bool AllowPlayerDeathOverride;
    internal bool FlyIsAscending;
    internal bool FlyIsDescending;

    public PlayerManager(ImpBinaryBinding sceneLoaded, IBinding<int> playersConnected)
        : base(sceneLoaded, playersConnected)
    {
        sceneLoaded.onTrigger += ShipTPAnchor.Refresh;
        sceneLoaded.onTrigger += MainEntranceTPAnchor.Refresh;
        sceneLoaded.onTrigger += ApparatusTPAnchor.Refresh;

        dropItemMessage.OnClientRecive += OnDropitem;
        teleportPlayerMessage.OnClientRecive += OnTeleportPlayer;
        killPlayerMessage.OnClientRecive += OnKillPlayer;
        revivePlayerMessage.OnClientRecive += OnRevivePlayer;
    }

    [ImpAttributes.RemoteMethod]
    internal void KillPlayer(ulong playerId) => killPlayerMessage.DispatchToServer(playerId);

    [ImpAttributes.RemoteMethod]
    internal void RevivePlayer(ulong playerId) => revivePlayerMessage.DispatchToServer(playerId);

    [ImpAttributes.RemoteMethod]
    internal void TeleportLocalPlayer(Vector3 position) => TeleportPlayer(new TeleportPlayerRequest
    {
        PlayerId = NetworkManager.Singleton.LocalClientId,
        Destination = position
    });

    [ImpAttributes.RemoteMethod]
    internal void TeleportPlayer(TeleportPlayerRequest request) => teleportPlayerMessage.DispatchToClients(request);

    [ImpAttributes.RemoteMethod]
    internal void DropItem(ulong playerId, int itemIndex)
    {
        dropItemMessage.DispatchToClients(new DropItemRequest
        {
            PlayerId = playerId,
            ItemIndex = itemIndex
        });
    }

    [ImpAttributes.LocalMethod]
    internal static void GrabObject(GrabbableObject grabbableItem, PlayerControllerB player)
    {
        NetworkObjectReference networkObject = grabbableItem.NetworkObject;

        player.carryWeight += Mathf.Clamp(grabbableItem.itemProperties.weight - 1f, 0f, 10f);
        Reflection.Invoke(player, "GrabObjectServerRpc", networkObject);

        grabbableItem.parentObject = player.localItemHolder;
        grabbableItem.GrabItemOnClient();
    }

    internal static int GetItemHolderSlot(GrabbableObject grabbableObject)
    {
        if (!grabbableObject.playerHeldBy || !grabbableObject.playerHeldBy.currentlyHeldObjectServer) return -1;

        for (var i = 0; i < grabbableObject.playerHeldBy.ItemSlots.Length; i++)
        {
            if (grabbableObject.playerHeldBy.ItemSlots[i] == grabbableObject)
            {
                return i;
            }
        }

        throw new ArgumentOutOfRangeException();
    }

    [ImpAttributes.LocalMethod]
    internal static void RestoreLocalPlayerHealth(PlayerControllerB player)
    {
        player.health = 100;
        HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);
    }

    [ImpAttributes.RemoteMethod]
    internal static void DiscardHotbarItem(int playerIndex, int itemSlot)
    {
        var player = StartOfRound.Instance.allPlayerScripts[playerIndex];
        var previousSlot = player.currentItemSlot;

        // Switch to item slot, discard item and switch back
        Reflection.Invoke(player, "SwitchToItemSlot", itemSlot, null);
        player.DiscardHeldObject();
        Reflection.Invoke(player, "SwitchToItemSlot", previousSlot, null);
    }

    internal static void UpdateCameras(bool _) => UpdateCameras();

    internal static void UpdateCameras()
    {
        foreach (var camera in Object.FindObjectsOfType<Camera>())
        {
            if (camera.gameObject.name == "MapCamera" || !camera.targetTexture) continue;

            var targetTexture = camera.targetTexture;

            if (!CameraOriginalResolutions.TryGetValue(targetTexture.GetInstanceID(), out var originalResolution))
            {
                originalResolution = new Vector2(targetTexture.width, targetTexture.height);
                CameraOriginalResolutions[targetTexture.GetInstanceID()] = originalResolution;
            }

            targetTexture.Release();
            targetTexture.width = Mathf.RoundToInt(
                originalResolution.x * Imperium.Settings.Rendering.ResolutionMultiplier.Value
            );
            targetTexture.height = Mathf.RoundToInt(
                originalResolution.y * Imperium.Settings.Rendering.ResolutionMultiplier.Value
            );
            targetTexture.Create();
        }

        Resources.UnloadUnusedAssets();

        foreach (var camera in Resources.FindObjectsOfTypeAll<HDAdditionalCameraData>())
        {
            if (camera.gameObject.name == "MapCamera") continue;

            camera.customRenderingSettings = true;

            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.DecalLayers] = true;
            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.DecalLayers, Imperium.Settings.Rendering.DecalLayers.Value
            );

            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.SSGI] = true;
            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.SSGI, Imperium.Settings.Rendering.SSGI.Value
            );

            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.RayTracing] = true;
            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.RayTracing, Imperium.Settings.Rendering.RayTracing.Value
            );

            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.VolumetricClouds] = true;
            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.VolumetricClouds, Imperium.Settings.Rendering.VolumetricClouds.Value
            );

            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.SubsurfaceScattering] = true;
            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.SubsurfaceScattering, Imperium.Settings.Rendering.SSS.Value
            );

            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.ReprojectionForVolumetrics] = true;
            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.ReprojectionForVolumetrics, Imperium.Settings.Rendering.VolumeReprojection.Value
            );

            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.TransparentPrepass] = true;
            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.TransparentPrepass, Imperium.Settings.Rendering.TransparentPrepass.Value
            );

            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.TransparentPostpass] = true;
            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.TransparentPostpass, Imperium.Settings.Rendering.TransparentPostpass.Value
            );
        }
    }

    #region RPC Handlers

    [ImpAttributes.LocalMethod]
    private static void OnDropitem(DropItemRequest request)
    {
        var player = StartOfRound.Instance.allPlayerScripts[request.PlayerId];
        var previousSlot = player.currentItemSlot;

        // Switch to item slot, discard item and switch back
        Reflection.Invoke(player, "SwitchToItemSlot", request.ItemIndex, null);
        player.DiscardHeldObject();
        Reflection.Invoke(player, "SwitchToItemSlot", previousSlot, null);
    }

    [ImpAttributes.LocalMethod]
    private static void OnTeleportPlayer(TeleportPlayerRequest request)
    {
        var player = Imperium.StartOfRound.allPlayerScripts[request.PlayerId];

        player.TeleportPlayer(request.Destination);
        var isInFactory = request.Destination.y < -100;
        player.isInsideFactory = isInFactory;

        // There is no easy way to check this, so it will just be off by default for now
        var isInElevator = Imperium.StartOfRound.shipBounds.bounds.Contains(request.Destination);
        player.isInElevator = isInElevator;

        var isInShip = Imperium.StartOfRound.shipInnerRoomBounds.bounds.Contains(request.Destination);
        player.isInHangarShipRoom = isInShip;

        foreach (var heldItem in Imperium.Player.ItemSlots)
        {
            if (!heldItem) continue;
            heldItem.isInFactory = isInFactory;
            heldItem.isInShipRoom = isInShip;
            heldItem.isInFactory = isInFactory;
        }

        if (request.PlayerId == NetworkManager.Singleton.LocalClientId) TimeOfDay.Instance.DisableAllWeather();
    }

    [ImpAttributes.LocalMethod]
    private void OnKillPlayer(ulong playerId)
    {
        Imperium.StartOfRound.livingPlayers++;

        if (playerId == NetworkManager.Singleton.LocalClientId)
        {
            AllowPlayerDeathOverride = true;
            Imperium.Player.KillPlayer(Vector3.zero, deathAnimation: 1);
            AllowPlayerDeathOverride = false;
        }
    }

    [ImpAttributes.LocalMethod]
    private static void OnRevivePlayer(ulong playerId)
    {
        var player = Imperium.StartOfRound.allPlayerScripts[playerId];

        // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
        if (player.playerBodyAnimator) player.playerBodyAnimator.SetBool("Limp", value: false);
        // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
        HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
        // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
        HUDManager.Instance.gameOverAnimator.SetTrigger("revive");

        player.isClimbingLadder = false;
        player.thisController.enabled = true;
        player.health = 100;
        player.carryWeight = 1;
        player.disableLookInput = false;
        player.isPlayerDead = false;
        player.isPlayerControlled = true;
        player.isInElevator = true;
        player.isInHangarShipRoom = true;
        player.isInsideFactory = false;
        player.wasInElevatorLastFrame = false;
        player.setPositionOfDeadPlayer = false;
        player.criticallyInjured = false;
        player.bleedingHeavily = false;
        player.activatingItem = false;
        player.twoHanded = false;
        player.inSpecialInteractAnimation = false;
        player.disableSyncInAnimation = false;
        player.inAnimationWithEnemy = null;
        player.holdingWalkieTalkie = false;
        player.speakingToWalkieTalkie = false;
        player.isSinking = false;
        player.isUnderwater = false;
        player.sinkingValue = 0f;
        player.hasBegunSpectating = false;
        player.hinderedMultiplier = 1f;
        player.isMovementHindered = 0;
        player.sourcesCausingSinking = 0;
        player.spectatedPlayerScript = null;
        player.helmetLight.enabled = false;

        player.ResetPlayerBloodObjects(player.isPlayerDead);
        player.ResetZAndXRotation();
        player.TeleportPlayer(Imperium.StartOfRound.shipDoorNode.position);
        player.DisablePlayerModel(player.gameObject, enable: true, disableLocalArms: true);
        player.Crouch(crouch: false);
        player.statusEffectAudio.Stop();
        player.DisableJetpackControlsLocally();
        Imperium.StartOfRound.SetPlayerObjectExtrapolate(enable: false);

        HUDManager.Instance.RemoveSpectateUI();
        HUDManager.Instance.UpdateHealthUI(100, hurtPlayer: false);

        Imperium.StartOfRound.SetSpectateCameraToGameOverMode(enableGameOver: false, player);

        // Close interface if player has revived themselves
        if (playerId == NetworkManager.Singleton.LocalClientId) Imperium.Interface.Close();

        Imperium.StartOfRound.allPlayersDead = false;
        Imperium.StartOfRound.UpdatePlayerVoiceEffects();
        Reflection.Invoke(Imperium.StartOfRound, "ResetMiscValues");

        // Respawn UI because for some reason this is not happening already
        Imperium.Settings.Rendering.PlayerHUD.Set(false);
        Imperium.Settings.Rendering.PlayerHUD.Set(true);
    }

    #endregion
}
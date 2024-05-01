#region

using System;
using GameNetcodeStuff;
using Imperium.Netcode;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Core;

internal class PlayerManager(ImpBinaryBinding sceneLoaded, ImpBinding<int> playersConnected, Component freecam)
    : ImpLifecycleObject(sceneLoaded, playersConnected)
{
    internal readonly ImpExternalBinding<Vector3?, bool> ShipTPAnchor = new(
        () => GameObject.Find("CatwalkShip")?.transform.position,
        sceneLoaded
    );

    internal readonly ImpExternalBinding<Vector3?, bool> MainEntranceTPAnchor = new(
        () => GameObject.Find("EntranceTeleportA")?.transform.position,
        sceneLoaded
    );

    internal readonly ImpExternalBinding<Vector3?, bool> ApparatusTPAnchor = new(
        () => GameObject.Find("LungApparatus(Clone)")?.transform.position,
        sceneLoaded
    );

    internal bool AllowPlayerDeathOverride;

    [ImpAttributes.LocalMethod]
    internal static void TeleportTo(Vector3 position)
    {
        ImpNetPlayer.Instance.TeleportPlayerServerRpc(
            GetPlayerID(Imperium.Player),
            new ImpVector(position)
        );
    }

    [ImpAttributes.LocalMethod]
    internal static void TeleportPlayer(Vector3 position, int playerIndex)
    {
        var player = Imperium.StartOfRound.allPlayerScripts[playerIndex];

        player.TeleportPlayer(position);
        var isInFactory = position.y < -100;
        // player.isInElevator = isInFactory;
        player.isInsideFactory = isInFactory;
        // player.isInHangarShipRoom = isInFactory;
        foreach (var heldItem in Imperium.Player.ItemSlots)
        {
            if (!heldItem) continue;
            heldItem.isInFactory = isInFactory;
        }
    }

    [ImpAttributes.RemoteMethod]
    internal static void GrabObject(GrabbableObject grabbableItem, PlayerControllerB player)
    {
        NetworkObjectReference networkObject = grabbableItem.NetworkObject;

        player.carryWeight += Mathf.Clamp(grabbableItem.itemProperties.weight - 1f, 0f, 10f);
        Reflection.Invoke(player, "GrabObjectServerRpc", networkObject);

        grabbableItem.parentObject = player.localItemHolder;
        grabbableItem.GrabItemOnClient();
    }

    [ImpAttributes.LocalMethod]
    internal static void RevivePlayer(int playerId)
    {
        var player = Imperium.StartOfRound.allPlayerScripts[playerId];

        // ReSharper disable once Unity.PreferAddressByIdToGraphicsParams
        // This is taken directly from the game
        if (player.playerBodyAnimator != null) player.playerBodyAnimator.SetBool("Limp", value: false);
        HUDManager.Instance.gasHelmetAnimator.SetBool("gasEmitting", value: false);
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
        if (IsLocalPlayer(playerId)) Imperium.Interface.Close();

        Imperium.StartOfRound.allPlayersDead = false;
        Imperium.StartOfRound.UpdatePlayerVoiceEffects();
        Reflection.Invoke(Imperium.StartOfRound, "ResetMiscValues");

        // Respawn UI because for some reason this is not happening already
        ImpSettings.Rendering.PlayerHUD.Set(false);
        ImpSettings.Rendering.PlayerHUD.Set(true);
    }

    internal static int GetItemHolderSlot(GrabbableObject grabbableObject)
    {
        if (!grabbableObject.playerHeldBy) return -1;

        for (var i = 0; i < grabbableObject.playerHeldBy.ItemSlots.Length; i++)
        {
            if (grabbableObject.playerHeldBy.ItemSlots[i] == grabbableObject)
            {
                return i;
            }
        }

        throw new ArgumentOutOfRangeException();
    }

    internal static int LocalPlayerId => GetPlayerID(Imperium.Player);

    internal static bool IsLocalPlayer(int playerIndex)
    {
        return Imperium.StartOfRound.allPlayerScripts[playerIndex] == Imperium.Player;
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

    internal static int GetPlayerID(PlayerControllerB player)
    {
        for (var i = 0; i < Imperium.StartOfRound.allPlayerScripts.Length; i++)
        {
            if (Imperium.StartOfRound.allPlayerScripts[i] == player)
            {
                return i;
            }
        }

        throw new ArgumentOutOfRangeException();
    }

    internal static PlayerControllerB GetPlayerFromID(int playerId)
    {
        if (playerId < 0 || playerId >= Imperium.StartOfRound.allPlayerScripts.Length)
        {
            throw new ArgumentOutOfRangeException();
        }
        return Imperium.StartOfRound.allPlayerScripts[playerId];
    }

    // Override for ImperiumSettings to use this as a method group
    internal static void UpdateCameras(bool _) => UpdateCameras();

    [ImpAttributes.LocalMethod]
    internal static void UpdateCameras()
    {
        foreach (var camera in Object.FindObjectsOfType<Camera>())
        {
            if (camera.gameObject.name == "MapCamera") continue;

            if (!camera.targetTexture) continue;
            var targetTexture = camera.targetTexture;
            targetTexture.Release();
            targetTexture.width = Mathf.RoundToInt(860 * ImpSettings.Rendering.ResolutionMultiplier.Value);
            targetTexture.height = Mathf.RoundToInt(520 * ImpSettings.Rendering.ResolutionMultiplier.Value);
            targetTexture.Create();

            Resources.UnloadUnusedAssets();
        }

        foreach (var camera in Resources.FindObjectsOfTypeAll<HDAdditionalCameraData>())
        {
            if (camera.gameObject.name == "MapCamera") continue;

            camera.customRenderingSettings = true;

            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.DecalLayers, ImpSettings.Rendering.DecalLayers.Value);
            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.DecalLayers] = true;

            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.SSGI, ImpSettings.Rendering.SSGI.Value);
            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.SSGI] = true;

            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.RayTracing, ImpSettings.Rendering.RayTracing.Value);
            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.RayTracing] = true;

            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.VolumetricClouds, ImpSettings.Rendering.VolumetricClouds.Value);
            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.VolumetricClouds] = true;

            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.SubsurfaceScattering, ImpSettings.Rendering.SubsurfaceScattering.Value);
            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.SubsurfaceScattering] = true;

            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.ReprojectionForVolumetrics, ImpSettings.Rendering.VolumeReprojection.Value);
            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.ReprojectionForVolumetrics] = true;

            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.TransparentPrepass, ImpSettings.Rendering.TransparentPrepass.Value);
            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.TransparentPrepass] = true;

            camera.renderingPathCustomFrameSettings.SetEnabled(
                FrameSettingsField.TransparentPostpass, ImpSettings.Rendering.TransparentPostpass.Value);
            camera.renderingPathCustomFrameSettingsOverrideMask.mask
                [(int)FrameSettingsField.TransparentPostpass] = true;
        }
    }
}
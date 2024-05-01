using System;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;

namespace Imperium.MonoBehaviours.ImpUI.MapUI;

internal class MapUI : LayerSelector.LayerSelector
{
    private Camera mapCamera;
    private GameObject compass;
    private Transform compassNorth;
    private Transform compassEast;
    private Transform compassSouth;
    private Transform compassWest;

    private Transform target;
    private Vector3 cameraViewOrigin = Vector3.zero;

    private Vector3 cameraTargetRotation;
    private bool mouseDragBlocked;
    private float snapBackAnimationTimer;

    private float mouseDragX;
    private float mouseDragY;

    private static readonly Quaternion cameraRotation = Quaternion.Euler(90f, 0f, 0f);

    public override void Awake() => InitializeUI();

    private readonly ImpBinding<PlayerControllerB> selectedPlayer = new(null);
    private readonly ImpBinding<EnemyAI> selectedEntity = new(null);
    private readonly ImpBinding<KeyValuePair<GameObject, string>> selectedMapHazard = new();

    protected override void InitUI()
    {
        var originalMapCam = GameObject.Find("MapCamera").GetComponent<Camera>();

        var cameraMapObject = new GameObject("ImpMap");
        cameraMapObject.transform.SetParent(originalMapCam.transform);
        cameraMapObject.transform.position = originalMapCam.transform.position + Vector3.up * 20f;
        cameraMapObject.transform.rotation = cameraRotation;

        var light = new GameObject("ImpMapLight").AddComponent<Light>();
        light.transform.SetParent(transform);
        light.transform.position = originalMapCam.transform.position + Vector3.up * 30f;
        light.range = 200f;
        light.intensity = 1000f;
        light.gameObject.layer = LayerMask.NameToLayer("HelmetVisor");

        var mapBorder = container.Find("MapBorder");
        mapBorder.gameObject.AddComponent<ImpInteractable>().onClick += () => Imperium.Log.LogInfo("Map Clicked!");

        var canvasScale = GetComponent<Canvas>().scaleFactor;
        var mapBorderRect = mapBorder.gameObject.GetComponent<RectTransform>().rect;
        var layerSelectorWidth = container.Find("Window").GetComponent<RectTransform>().rect.width * canvasScale - 5;
        var mapContainerWidth = mapBorderRect.width * canvasScale - 15;
        var mapContainerHeight = mapBorderRect.height * canvasScale - 15;

        mapCamera = cameraMapObject.AddComponent<Camera>();
        mapCamera.enabled = false;
        mapCamera.orthographic = true;
        // mapCamera.farClipPlane = 100f;
        // mapCamera.nearClipPlane = -30f;
        mapCamera.rect = new Rect(
            (1 - mapContainerWidth / Screen.width) / 2,
            (1 - mapContainerHeight / Screen.height) / 2,
            mapContainerWidth / Screen.width - layerSelectorWidth / Screen.width,
            mapContainerHeight / Screen.height
        );

        mapCamera.cullingMask = ImpSettings.Map.CameraLayerMask.Value;
        ImpSettings.Map.CameraLayerMask.onUpdate += value => mapCamera.cullingMask = value;

        mapCamera.orthographicSize = ImpSettings.Map.CameraZoom.Value;
        ImpSettings.Map.CameraZoom.onUpdate += value => mapCamera.orthographicSize = value;

        mapCamera.farClipPlane = ImpSettings.Map.CameraFarClip.Value;
        ImpSettings.Map.CameraFarClip.onUpdate += value => mapCamera.farClipPlane = value;

        mapCamera.nearClipPlane = ImpSettings.Map.CameraNearClip.Value;
        ImpSettings.Map.CameraNearClip.onUpdate += value => mapCamera.nearClipPlane = value;

        var hdCameraData = cameraMapObject.AddComponent<HDAdditionalCameraData>();
        hdCameraData.customRenderingSettings = true;
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, false);
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;

        base.InitUI();
        Bind(new ImpBinding<bool>(true), ImpSettings.Map.CameraLayerMask);

        // Subscribe to the switch item action for zooming
        Imperium.IngamePlayerSettings.playerInput.actions.FindAction("SwitchItem").performed += OnMouseScroll;
        Imperium.InputBindings.BaseMap["Reset"].performed += OnMapReset;

        // "Apply" target rotation when enabling / disabling rotation lock
        ImpSettings.Map.RotationLock.onUpdate += isOn =>
        {
            if (isOn)
            {
                mouseDragX -= target.rotation.eulerAngles.y;
            }
            else
            {
                mouseDragX += target.rotation.eulerAngles.y;
            }
        };

        InitCompass();
        InitZoomSlider();
        InitMapSettings();
        InitTargetSelection();
        InitMouseBlockers();
    }

    private void OnMapReset(InputAction.CallbackContext callbackContext) => OnMapReset();
    private void OnMapReset() => MoveCameraToTarget(Imperium.Player.gameplayCamera.transform);

    private void InitMouseBlockers()
    {
        var mapSettings = container.Find("MapSettings").gameObject.AddComponent<ImpInteractable>();
        mapSettings.onEnter += () => mouseDragBlocked = true;
        mapSettings.onExit += () => mouseDragBlocked = false;

        var targetSelection = container.Find("TargetSelection").gameObject.AddComponent<ImpInteractable>();
        targetSelection.onEnter += () => mouseDragBlocked = true;
        targetSelection.onExit += () => mouseDragBlocked = false;

        var zoomSlider = container.Find("ZoomSlider").gameObject.AddComponent<ImpInteractable>();
        zoomSlider.onEnter += () => mouseDragBlocked = true;
        zoomSlider.onExit += () => mouseDragBlocked = false;

        var nearPlaneSlider = container.Find("NearClip").gameObject.AddComponent<ImpInteractable>();
        nearPlaneSlider.onEnter += () => mouseDragBlocked = true;
        nearPlaneSlider.onExit += () => mouseDragBlocked = false;

        var farPlaneSlider = container.Find("FarClip").gameObject.AddComponent<ImpInteractable>();
        farPlaneSlider.onEnter += () => mouseDragBlocked = true;
        farPlaneSlider.onExit += () => mouseDragBlocked = false;

        var layerSelector = container.Find("Window").gameObject.AddComponent<ImpInteractable>();
        layerSelector.onEnter += () => mouseDragBlocked = true;
        layerSelector.onExit += () => mouseDragBlocked = false;
    }

    private void InitCompass()
    {
        compass = container.Find("Compass").gameObject;
        compass.SetActive(ImpSettings.Map.CompassEnabled.Value);
        compass.gameObject.AddComponent<ImpInteractable>().onClick += OnMapReset;
        ImpSettings.Map.CompassEnabled.onUpdate += compass.SetActive;

        compassNorth = compass.transform.Find("North");
        compassEast = compass.transform.Find("East");
        compassSouth = compass.transform.Find("West");
        compassWest = compass.transform.Find("South");
    }

    private void InitZoomSlider()
    {
        ImpSlider.Bind(
            path: "ZoomSlider",
            container: container,
            valueBinding: ImpSettings.Map.CameraZoom,
            indicatorUnit: "x",
            indicatorFormatter: ImpUtils.Math.FormatFloatToThreeDigits
        );

        ImpSlider.Bind(
            path: "NearClip",
            container: container,
            valueBinding: ImpSettings.Map.CameraNearClip,
            indicatorFormatter: ImpUtils.Math.FormatFloatToThreeDigits
        );

        ImpSlider.Bind(
            path: "FarClip",
            container: container,
            valueBinding: ImpSettings.Map.CameraFarClip,
            indicatorFormatter: ImpUtils.Math.FormatFloatToThreeDigits
        );
    }

    private void InitMapSettings()
    {
        ImpToggle.Bind("MapSettings/MinimapEnabled", container, ImpSettings.Map.MinimapEnabled);
        ImpToggle.Bind("MapSettings/CompassEnabled", container, ImpSettings.Map.CompassEnabled);
        ImpToggle.Bind("MapSettings/RotationLock", container, ImpSettings.Map.RotationLock);
        ImpToggle.Bind("MapSettings/UnlockView", container, ImpSettings.Map.UnlockView);
    }

    private void InitTargetSelection()
    {
        selectedPlayer.onUpdate += player =>
        {
            if (!player) return;

            selectedEntity.Set(null);
            selectedMapHazard.Set(default);

            MoveCameraToTarget(player.gameplayCamera.transform);
        };

        selectedEntity.onUpdate += entity =>
        {
            if (!entity) return;

            selectedPlayer.Set(null);
            selectedMapHazard.Set(default);

            target = entity.transform;
            MoveCameraToTarget(entity.transform);
        };

        selectedMapHazard.onUpdate += mapHazardEntry =>
        {
            if (!mapHazardEntry.Key) return;

            selectedPlayer.Set(null);
            selectedEntity.Set(null);

            target = mapHazardEntry.Key.transform;
            MoveCameraToTarget(mapHazardEntry.Key.transform);
        };

        // Target local player by default
        selectedPlayer.Set(Imperium.Player);

        ImpMultiSelect.Bind(
            "TargetSelection/Players",
            container,
            selectedPlayer,
            Imperium.ObjectManager.CurrentPlayers,
            player => player.playerUsername
        );

        ImpMultiSelect.Bind(
            "TargetSelection/Entities",
            container,
            selectedEntity,
            Imperium.ObjectManager.CurrentLevelEntities,
            entity => entity.enemyType.enemyName
        );

        var mapHazardBinding =
            new ImpExternalBinding<HashSet<KeyValuePair<GameObject, string>>, HashSet<Turret>>(
                () => Imperium.ObjectManager.CurrentLevelTurrets.Value
                    .Where(obj => obj != null)
                    .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Turret"))
                    .Concat(Imperium.ObjectManager.CurrentLevelBreakerBoxes.Value
                        .Where(obj => obj != null)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Breaker Box")))
                    .Concat(Imperium.ObjectManager.CurrentLevelVents.Value
                        .Where(obj => obj != null)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Vent")))
                    .Concat(Imperium.ObjectManager.CurrentLevelLandmines.Value
                        .Where(obj => obj != null)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Landmine")))
                    .Concat(Imperium.ObjectManager.CurrentLevelSpikeTraps.Value
                        .Where(obj => obj != null)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Spike Trap")))
                    .Concat(Imperium.ObjectManager.CurrentLevelSpiderWebs.Value
                        .Where(obj => obj != null)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Spider Web")))
                    .ToHashSet(),
                Imperium.ObjectManager.CurrentLevelTurrets
            );
        Imperium.ObjectManager.CurrentLevelBreakerBoxes.onTrigger += mapHazardBinding.Refresh;
        Imperium.ObjectManager.CurrentLevelVents.onTrigger += mapHazardBinding.Refresh;
        Imperium.ObjectManager.CurrentLevelLandmines.onTrigger += mapHazardBinding.Refresh;
        Imperium.ObjectManager.CurrentLevelSpikeTraps.onTrigger += mapHazardBinding.Refresh;
        Imperium.ObjectManager.CurrentLevelSpiderWebs.onTrigger += mapHazardBinding.Refresh;

        ImpMultiSelect.Bind(
            "TargetSelection/MapHazards",
            container,
            selectedMapHazard,
            mapHazardBinding,
            entry => entry.Value
        );
    }

    private void MoveCameraToTarget(Transform newTarget)
    {
        target = newTarget;

        // Set target to default top-down rotation and start animation
        cameraTargetRotation = ImpSettings.Map.UnlockView.Value
            ? new Vector3(UnityEngine.Random.Range(0, 366), 40, 0)
            : new Vector3(0, 90, 0);
        snapBackAnimationTimer = 1;
    }

    private void OnMouseScroll(InputAction.CallbackContext context)
    {
        if (!enabled || mouseDragBlocked) return;

        var multiplier = ImpSettings.Map.CameraZoom.Value / 100 * 8;
        ImpSettings.Map.CameraZoom.Set(
            Mathf.Clamp(
                ImpSettings.Map.CameraZoom.Value - context.ReadValue<float>() * multiplier,
                0.1f,
                100
            )
        );
    }

    protected override void OnOpen() => mapCamera.enabled = true;
    protected override void OnClose() => mapCamera.enabled = false;

    // We override this function to skip the layer manager logic with showing / hiding the UI
    private void Update()
    {
        if (ImpSettings.Map.CompassEnabled.Value)
        {
            var rotationY = mapCamera.transform.rotation.eulerAngles.y;
            compass.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationY));

            // Counter-rotate to keep the labels upright
            compassNorth.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassEast.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassSouth.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassWest.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
        }
    }

    private void LateUpdate()
    {
        if (!IsOpen) return;

        // Camera sliding animation
        if (snapBackAnimationTimer > 0 && target)
        {
            // Camera rotation
            mouseDragX = Mathf.Lerp(
                mouseDragX,
                // Compensate for target rotation if rotation lock is enabled
                ImpSettings.Map.RotationLock.Value
                    ? cameraTargetRotation.x - target.rotation.eulerAngles.y
                    : cameraTargetRotation.x,
                1 - snapBackAnimationTimer
            );
            mouseDragY = Mathf.Lerp(
                mouseDragY,
                cameraTargetRotation.y,
                1 - snapBackAnimationTimer
            );

            // View origin translation
            cameraViewOrigin = Vector3.Lerp(
                cameraViewOrigin,
                target.position,
                1 - snapBackAnimationTimer
            );

            snapBackAnimationTimer -= Time.deltaTime;
        }
        else if (target)
        {
            cameraViewOrigin = target.position;
        }

        if (!mouseDragBlocked)
        {
            var input = Imperium.InputBindings.BaseMap["Look"].ReadValue<Vector2>();
            if (Imperium.InputBindings.BaseMap["LeftClick"].IsPressed())
            {
                mouseDragX += input.x * 0.25f;
                mouseDragY -= input.y * 0.25f;
                mouseDragY = Mathf.Clamp(mouseDragY, -89.9f, 89.9f);

                // Set the animation timer to 0 to interrupt the slide animation
                snapBackAnimationTimer = 0;
                cameraTargetRotation = new Vector3(mouseDragX, mouseDragY, 0);
            }
            else if (Imperium.InputBindings.BaseMap["RightClick"].IsPressed())
            {
                var inputVector = new Vector3(
                    -input.x * 0.002f * ImpSettings.Map.CameraZoom.Value,
                    -input.y * 0.002f * ImpSettings.Map.CameraZoom.Value,
                    0
                );
                inputVector = mapCamera.transform.TransformDirection(inputVector);

                cameraViewOrigin += inputVector;

                // De-select current entity / player when switching to pan mode
                selectedEntity.Set(null);
                selectedPlayer.Set(null);
                target = null;

                // Set the animation timer to 0 to interrupt the slide animation
                snapBackAnimationTimer = 0;
                cameraTargetRotation = new Vector3(mouseDragX, mouseDragY, 0);
            }
        }

        var direction = new Vector3(0, 0, -10.0f);
        // Add target rotation if rotation lock is activated
        var dragX = target && ImpSettings.Map.RotationLock.Value
            ? mouseDragX + target.rotation.eulerAngles.y
            : mouseDragX;
        var rotation = Quaternion.Euler(mouseDragY, dragX, 0);
        mapCamera.transform.position = cameraViewOrigin + rotation * direction;
        mapCamera.transform.LookAt(cameraViewOrigin);
    }
}
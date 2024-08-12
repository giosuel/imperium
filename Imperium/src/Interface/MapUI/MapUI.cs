#region

using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Interface.MapUI;

internal class MapUI : BaseUI
{
    private GameObject compass;
    private Transform compassNorth;
    private Transform compassEast;
    private Transform compassSouth;
    private Transform compassWest;

    private ImpSlider farClipSlider;
    private ImpSlider nearClipSlider;

    private Transform target;
    private Vector3 cameraViewOrigin = Vector3.zero;

    private Vector3 cameraTargetRotation;
    private bool mouseDragBlocked;
    private float snapBackAnimationTimer;

    private float mouseOffsetX;
    private float mouseOffsetY;

    private Rect mapUICameraRect;

    private readonly ImpBinding<PlayerControllerB> selectedPlayer = new(null);
    private readonly ImpBinding<EnemyAI> selectedEntity = new(null);
    private readonly ImpBinding<KeyValuePair<GameObject, string>> selectedMapHazard = new();

    protected override void InitUI()
    {
        mapUICameraRect = GetCameraRect();

        Imperium.InputBindings.BaseMap.Reset.performed += OnMapReset;
        Imperium.InputBindings.BaseMap.Minimap.performed += OnMinimapToggle;

        InitCompass();
        InitSliders();
        InitMapSettings();
        InitTargetSelection();
        InitMouseBlockers();

        // Init layer selector and bind the layer mask
        var layerSelector = container.Find("LayerSelector").gameObject.AddComponent<LayerSelector.LayerSelector>();
        layerSelector.InitUI(theme);
        layerSelector.Bind(new ImpBinding<bool>(true), Imperium.Settings.Map.CameraLayerMask);

        Imperium.Settings.Map.RotationLock.onTrigger += OnRotationLockChange;

        selectedPlayer.Set(Imperium.Player);
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("LayerSelector", Variant.BACKGROUND),
            new StyleOverride("LayerSelector/Border", Variant.DARKER),
            new StyleOverride("LayerSelector/ScrollView/Scrollbar", Variant.DARKEST),
            new StyleOverride("LayerSelector/ScrollView/Scrollbar/SlidingArea/Handle", Variant.FOREGROUND),
            new StyleOverride("MapBorder", Variant.DARKER),
            new StyleOverride("MapSettings", Variant.BACKGROUND),
            new StyleOverride("MapSettings/Border", Variant.DARKER),
            new StyleOverride("TargetSelection", Variant.BACKGROUND),
            new StyleOverride("TargetSelection/Border", Variant.DARKER),
            // Compass
            new StyleOverride("Compass", Variant.FOREGROUND),
            new StyleOverride("Compass/Icon", Variant.FOREGROUND),
            // Far Clip Slider
            new StyleOverride("FarClip", Variant.BACKGROUND),
            new StyleOverride("FarClip/Border", Variant.DARKER),
            // Near Clip Slider
            new StyleOverride("NearClip", Variant.BACKGROUND),
            new StyleOverride("NearClip/Border", Variant.DARKER),
            // Zoom Slider
            new StyleOverride("ZoomSlider", Variant.BACKGROUND),
            new StyleOverride("ZoomSlider/Border", Variant.DARKER)
        );
        ImpThemeManager.StyleText(
            themeUpdate,
            container,
            new StyleOverride("Compass/North", Variant.FOREGROUND),
            new StyleOverride("Compass/East", Variant.FOREGROUND),
            new StyleOverride("Compass/South", Variant.FOREGROUND),
            new StyleOverride("Compass/West", Variant.FOREGROUND)
        );
    }

    private void OnRotationLockChange() => MoveCameraToTarget(target);

    private Rect GetCameraRect()
    {
        var mapBorder = container.Find("MapBorder");
        var canvasScale = GetComponent<Canvas>().scaleFactor;
        var borderTransform = mapBorder.gameObject.GetComponent<RectTransform>();

        return ImpGeometry.NormalizeRectTransform(borderTransform, canvasScale);
    }

    private static void OnMinimapToggle(InputAction.CallbackContext _)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat ||
            Imperium.ShipBuildModeManager.InBuildMode) return;

        Imperium.Settings.Map.MinimapEnabled.Set(!Imperium.Settings.Map.MinimapEnabled.Value);
    }

    private void OnMapReset(InputAction.CallbackContext _) => OnMapReset();
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

        var layerSelector = container.Find("LayerSelector").gameObject.AddComponent<ImpInteractable>();
        layerSelector.onEnter += () => mouseDragBlocked = true;
        layerSelector.onExit += () => mouseDragBlocked = false;

        var floorSlider = container.Find("FloorSlider").gameObject.AddComponent<ImpInteractable>();
        floorSlider.onEnter += () => mouseDragBlocked = true;
        floorSlider.onExit += () => mouseDragBlocked = false;
    }

    private void InitCompass()
    {
        compass = container.Find("Compass").gameObject;
        compass.SetActive(Imperium.Settings.Map.CompassEnabled.Value);
        compass.gameObject.AddComponent<ImpInteractable>().onClick += OnMapReset;
        Imperium.Settings.Map.CompassEnabled.onUpdate += compass.SetActive;

        compassNorth = compass.transform.Find("North");
        compassEast = compass.transform.Find("East");
        compassSouth = compass.transform.Find("West");
        compassWest = compass.transform.Find("South");
    }

    private void InitSliders()
    {
        ImpSlider.Bind(
            path: "ZoomSlider",
            container: container,
            valueBinding: Imperium.Settings.Map.CameraZoom,
            indicatorUnit: "x",
            indicatorFormatter: value => Mathf.RoundToInt(value).ToString(),
            theme: theme
        );
        container.Find("ZoomSlider/MinIcon").gameObject.AddComponent<ImpInteractable>()
            .onClick += () => Imperium.Settings.Map.CameraZoom.Set(1);
        container.Find("ZoomSlider/MaxIcon").gameObject.AddComponent<ImpInteractable>()
            .onClick += () => Imperium.Settings.Map.CameraZoom.Set(100);

        farClipSlider = ImpSlider.Bind(
            path: "NearClip",
            container: container,
            valueBinding: Imperium.Map.CameraNearClip,
            indicatorFormatter: value => Mathf.RoundToInt(value).ToString(),
            playClickSound: false,
            theme: theme
        );
        farClipSlider.gameObject.SetActive(!Imperium.Settings.Map.AutoClipping.Value);
        Imperium.Settings.Map.AutoClipping.onUpdate += value => farClipSlider.gameObject.SetActive(!value);

        nearClipSlider = ImpSlider.Bind(
            path: "FarClip",
            container: container,
            valueBinding: Imperium.Map.CameraFarClip,
            indicatorFormatter: value => Mathf.RoundToInt(value).ToString(),
            playClickSound: false,
            theme: theme
        );
        nearClipSlider.gameObject.SetActive(!Imperium.Settings.Map.AutoClipping.Value);
        Imperium.Settings.Map.AutoClipping.onUpdate += value => nearClipSlider.gameObject.SetActive(!value);
    }

    private void InitMapSettings()
    {
        ImpToggle.Bind("MapSettings/MinimapEnabled", container, Imperium.Settings.Map.MinimapEnabled, theme);
        ImpToggle.Bind("MapSettings/CompassEnabled", container, Imperium.Settings.Map.CompassEnabled, theme);
        ImpToggle.Bind(
            "MapSettings/RotationLock",
            container,
            Imperium.Settings.Map.RotationLock,
            theme,
            tooltipDefinition: new TooltipDefinition
            {
                Tooltip = tooltip,
                Description = "Whether the camera is clamped\nto the target's rotation"
            }
        );
        ImpToggle.Bind(
            "MapSettings/UnlockView",
            container,
            Imperium.Settings.Map.UnlockView,
            theme,
            tooltipDefinition: new TooltipDefinition
            {
                Tooltip = tooltip,
                Description = "When off, the camera resets to a 45 angle.\nWhen on, the camers resets to top-down view."
            }
        );
        ImpToggle.Bind("MapSettings/AutoClipping", container, Imperium.Settings.Map.AutoClipping, theme);
        ImpButton.Bind(
            "MapSettings/MinimapSettings",
            container,
            () => Imperium.Interface.Open<MinimapSettings>(),
            theme
        );
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

        ImpMultiSelect.Bind(
            "TargetSelection/Players",
            container,
            selectedPlayer,
            Imperium.ObjectManager.CurrentPlayers,
            player => player.playerUsername,
            theme: theme
        );

        ImpMultiSelect.Bind(
            "TargetSelection/Entities",
            container,
            selectedEntity,
            Imperium.ObjectManager.CurrentLevelEntities,
            entity => entity.enemyType.enemyName,
            theme: theme
        );

        ImpMultiSelect.Bind(
            "TargetSelection/MapHazards",
            container,
            selectedMapHazard,
            GenerateMapHazardBinding(),
            entry => entry.Value,
            theme: theme
        );

        // "Apply" target rotation when enabling / disabling rotation lock
        Imperium.Settings.Map.RotationLock.onUpdate += isOn =>
        {
            if (!target) return;

            if (isOn)
            {
                mouseOffsetX -= target.rotation.eulerAngles.y;
            }
            else
            {
                mouseOffsetX += target.rotation.eulerAngles.y;
            }
        };
    }

    /// <summary>
    ///     We need to do this because the map hazards are all stored in different lists as they are all different types.
    ///     Since we want to put them all in the same list here, we need to concat them into a list of key-value pairs.
    ///     The list has to subscribe to all the source bindings in order to be updated when something changes.
    /// </summary>
    /// <returns></returns>
    private static ImpBinding<IReadOnlyCollection<KeyValuePair<GameObject, string>>> GenerateMapHazardBinding()
    {
        var mapHazardBinding =
            new ImpExternalBinding<IReadOnlyCollection<KeyValuePair<GameObject, string>>, IReadOnlyCollection<Turret>>(
                () => Imperium.ObjectManager.CurrentLevelTurrets.Value
                    .Where(obj => obj)
                    .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Turret"))
                    .Concat(Imperium.ObjectManager.CurrentLevelBreakerBoxes.Value
                        .Where(obj => obj)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Breaker Box")))
                    .Concat(Imperium.ObjectManager.CurrentLevelVents.Value
                        .Where(obj => obj)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Vent")))
                    .Concat(Imperium.ObjectManager.CurrentLevelLandmines.Value
                        .Where(obj => obj)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Landmine")))
                    .Concat(Imperium.ObjectManager.CurrentLevelSpikeTraps.Value
                        .Where(obj => obj)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Spike Trap")))
                    .Concat(Imperium.ObjectManager.CurrentLevelSpiderWebs.Value
                        .Where(obj => obj)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry.gameObject, "Spider Web")))
                    .Concat(Imperium.ObjectManager.CurrentLevelMoldSpores.Value
                        .Where(obj => obj)
                        .Select(entry => new KeyValuePair<GameObject, string>(entry, "Mold Spore")))
                    .ToHashSet(),
                Imperium.ObjectManager.CurrentLevelTurrets
            );
        mapHazardBinding.Refresh();
        Imperium.ObjectManager.CurrentLevelVents.onTrigger += mapHazardBinding.Refresh;
        Imperium.ObjectManager.CurrentLevelLandmines.onTrigger += mapHazardBinding.Refresh;
        Imperium.ObjectManager.CurrentLevelSpikeTraps.onTrigger += mapHazardBinding.Refresh;
        Imperium.ObjectManager.CurrentLevelSpiderWebs.onTrigger += mapHazardBinding.Refresh;
        Imperium.ObjectManager.CurrentLevelBreakerBoxes.onTrigger += mapHazardBinding.Refresh;

        return mapHazardBinding;
    }

    private void MoveCameraToTarget(Transform newTarget)
    {
        target = newTarget;

        // Set target to default top-down rotation and start animation
        var originX = Imperium.Settings.Map.RotationLock.Value ? target.rotation.eulerAngles.y : 0;
        cameraTargetRotation = Imperium.Settings.Map.UnlockView.Value
            ? new Vector3(Random.Range(0, 366), 40, 0)
            : new Vector3(originX, 89.9f, 0);
        snapBackAnimationTimer = 1;


        // Use free-look clipping when camera is unlocked
        if (Imperium.Settings.Map.UnlockView.Value) Imperium.Map.SetCameraClipped(false);
    }

    protected override void OnOpen()
    {
        Imperium.Map.Camera.enabled = true;
        Imperium.Map.Camera.rect = mapUICameraRect;
    }

    protected override void OnClose()
    {
        if (!Imperium.Map.Minimap.IsOpen) Imperium.Map.Camera.enabled = false;

        // Reset camera rotation to match target when closing the UI and rotation lock is enabled
        if (Imperium.Settings.Map.RotationLock.Value && target) mouseOffsetX = 0;
    }

    /// <summary>
    ///     We don't call the base update function here, as the underlaying layer selector doesn't need to be
    ///     opened or closed.
    /// </summary>
    private void Update()
    {
        if (Imperium.Settings.Map.CompassEnabled.Value)
        {
            var rotationY = Imperium.Map.Camera.transform.rotation.eulerAngles.y;
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
        // Camera sliding animation
        if (snapBackAnimationTimer > 0 && target)
        {
            // Camera rotation
            mouseOffsetX = Mathf.Lerp(
                mouseOffsetX,
                // Compensate for target rotation if rotation lock is enabled
                Imperium.Settings.Map.RotationLock.Value
                    ? cameraTargetRotation.x - target.rotation.eulerAngles.y
                    : cameraTargetRotation.x,
                1 - snapBackAnimationTimer
            );
            mouseOffsetY = Mathf.Lerp(
                mouseOffsetY,
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

            // Reset clipping at the end of the animation if auto clipping is on
            if (Imperium.Settings.Map.AutoClipping.Value
                && !Imperium.Settings.Map.UnlockView.Value
                && snapBackAnimationTimer < 0.5f
                && (Imperium.Player.isInsideFactory
                    || Imperium.Player.isInElevator
                    || Imperium.Player.isInHangarShipRoom))
            {
                Imperium.Map.SetCameraClipped(true);
            }
        }
        else if (target)
        {
            cameraViewOrigin = target.position;

            // Make sure the camera rotation is always fixed when the minimap is open and rotation lock is enabled
            if (Imperium.Settings.Map.RotationLock.Value && Imperium.Map.Minimap.IsOpen) mouseOffsetX = 0;
        }

        // Mouse input processing
        if (IsOpen && !mouseDragBlocked)
        {
            var input = Imperium.Player.playerActions.Movement.Look.ReadValue<Vector2>();
            if (Imperium.InputBindings.BaseMap.MapRotate.IsPressed())
            {
                mouseOffsetX += input.x * 0.25f;
                mouseOffsetY -= input.y * 0.25f;
                mouseOffsetY = Mathf.Clamp(mouseOffsetY, -89.9f, 89.9f);

                // Change clipping to global when the perspective is changed
                if (input.y > 1)
                {
                    Imperium.Map.CameraNearClip.Set(ImpConstants.DefaultMapCameraNearClipFreeLook);
                    Imperium.Map.CameraFarClip.Set(ImpConstants.DefaultMapCameraFarClipFreeLook);
                }

                // Set the animation timer to 0 to interrupt the slide animation
                snapBackAnimationTimer = 0;
                cameraTargetRotation = new Vector3(mouseOffsetX, mouseOffsetY, 0);
            }
            else if (Imperium.InputBindings.BaseMap.MapPan.IsPressed())
            {
                var inputVector = new Vector3(
                    -input.x * 0.0016f * Imperium.Settings.Map.CameraZoom.Value,
                    -input.y * 0.0016f * Imperium.Settings.Map.CameraZoom.Value,
                    0
                );
                inputVector = Imperium.Map.Camera.transform.TransformDirection(inputVector);

                cameraViewOrigin += inputVector;

                // De-select current entity / player when switching to pan mode
                selectedEntity.Set(null);
                selectedPlayer.Set(null);

                if (target)
                {
                    // "Apply" target rotation when enabling / disabling rotation lock
                    if (Imperium.Settings.Map.RotationLock.Value)
                    {
                        mouseOffsetX += target.rotation.eulerAngles.y;
                    }

                    target = null;
                }

                // Set the animation timer to 0 to interrupt the slide animation
                snapBackAnimationTimer = 0;
                cameraTargetRotation = new Vector3(mouseOffsetX, mouseOffsetY, 0);
            }
        }

        // Camera position update
        var direction = new Vector3(0, 0, -10.0f);
        // Add target rotation if rotation lock is activated
        var dragX = target && Imperium.Settings.Map.RotationLock.Value
            ? mouseOffsetX + target.rotation.eulerAngles.y
            : mouseOffsetX;
        var rotation = Quaternion.Euler(mouseOffsetY, dragX, 0);
        Imperium.Map.Camera.transform.position = cameraViewOrigin + rotation * direction;
        Imperium.Map.Camera.transform.LookAt(cameraViewOrigin);
    }
}
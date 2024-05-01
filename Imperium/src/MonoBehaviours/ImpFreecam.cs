#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.LayerSelector;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.MonoBehaviours;

public class ImpFreecam : MonoBehaviour
{
    private Camera gameplayCamera;
    private Vector2 lookInput;
    private LayerSelector layerSelector;

    internal Camera FreecamCamera { get; private set; }

    private static Rect minicamRect => new(100f / Screen.width, 1 - 100f / Screen.height - 0.4f, 0.4f, 0.4f);

    internal readonly ImpBinaryBinding IsFreecamEnabled = new(false);
    internal readonly ImpBinaryBinding IsMinicamEnabled = new(false);

    internal static ImpFreecam Create() => new GameObject("ImpFreecam").AddComponent<ImpFreecam>();

    private bool firstTimeOpen = true;

    private void Awake()
    {
        gameplayCamera = Imperium.Player.gameplayCamera;

        FreecamCamera = gameObject.AddComponent<Camera>();
        FreecamCamera.CopyFrom(gameplayCamera);
        FreecamCamera.cullingMask = ImpSettings.Freecam.FreecamLayerMask.Value;
        FreecamCamera.farClipPlane = 2000f;
        FreecamCamera.enabled = false;

        var hdCameraData = FreecamCamera.gameObject.AddComponent<HDAdditionalCameraData>();
        hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;
        hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, false);

        var layerSelectorObject = Instantiate(ImpAssets.LayerSelectorFreecam, transform);
        layerSelector = layerSelectorObject.AddComponent<LayerSelector>();
        layerSelector.Bind(ImpSettings.Freecam.LayerSelector, ImpSettings.Freecam.FreecamLayerMask);

        IsFreecamEnabled.onTrue += OnFreecamEnable;
        IsFreecamEnabled.onFalse += OnFreecamDisable;

        IsMinicamEnabled.onTrue += OnMinicamEnable;
        IsMinicamEnabled.onFalse += OnMinicamDisable;

        var lightObject = Instantiate(Imperium.Player.nightVision.gameObject, transform, false);
        lightObject.transform.position = Vector3.up;

        Imperium.InputBindings.BaseMap["Freecam"].performed += OnFreecamToggle;
        Imperium.InputBindings.BaseMap["Minicam"].performed += OnMinicamToggle;
        Imperium.InputBindings.BaseMap["Reset"].performed += OnFreecamReset;
        Imperium.InputBindings.FreecamMap["LayerSelector"].performed += OnToggleLayerSelector;
        ImpSettings.Freecam.FreecamLayerMask.onUpdate += value => FreecamCamera.cullingMask = value;
    }

    private void OnMinicamToggle(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat ||
            Imperium.ShipBuildModeManager.InBuildMode) return;

        IsMinicamEnabled.Toggle();
    }

    private void OnFreecamToggle(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat) return;

        IsFreecamEnabled.Toggle();
    }

    private void OnMinicamEnable()
    {
        if (IsFreecamEnabled.Value) IsFreecamEnabled.SetFalse();

        HUDManager.Instance.HideHUD(true);
        FreecamCamera.enabled = true;
        FreecamCamera.rect = minicamRect;
    }

    private void OnMinicamDisable()
    {
        // Hide UI if view is not switching from minicam to freecam
        if (!IsFreecamEnabled.Value) HUDManager.Instance.HideHUD(false);

        FreecamCamera.enabled = false;

        FreecamCamera.rect = new Rect(0, 0, 1, 1);
    }

    private void OnFreecamEnable()
    {
        Imperium.Interface.Close();

        if (IsMinicamEnabled.Value) IsMinicamEnabled.SetFalse();

        HUDManager.Instance.HideHUD(true);
        Imperium.InputBindings.FreecamMap.Enable();
        FreecamCamera.enabled = true;
        Imperium.StartOfRound.SwitchCamera(FreecamCamera);
        Imperium.Player.isFreeCamera = true;
        enabled = true;

        if (firstTimeOpen)
        {
            firstTimeOpen = false;
            FreecamCamera.transform.position = Imperium.Player.gameplayCamera.transform.position + Vector3.up * 2;
        }
    }

    private void OnFreecamDisable()
    {
        layerSelector.OnUIClose();

        // Hide UI if view is not switching to minimap state
        if (!IsMinicamEnabled.Value) HUDManager.Instance.HideHUD(false);
        
        Imperium.InputBindings.FreecamMap.Disable();
        FreecamCamera.enabled = false;
        Imperium.StartOfRound.SwitchCamera(gameplayCamera);
        Imperium.Player.isFreeCamera = false;
        enabled = false;
    }

    private void OnFreecamReset(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat) return;

        FreecamCamera.transform.position = Imperium.Player.gameplayCamera.transform.position + Vector3.up * 2;

        ImpSettings.Freecam.FreecamFieldOfView.Set(ImpConstants.DefaultFOV);
    }

    private void OnToggleLayerSelector(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen ||
            Imperium.Player.inTerminalMenu ||
            Imperium.Player.isTypingChat) return;

        ImpSettings.Freecam.LayerSelector.Set(!layerSelector.IsOpen);
        if (layerSelector.IsOpen)
        {
            layerSelector.OnUIClose();
        }
        else
        {
            layerSelector.OnUIOpen();
        }
    }

    private void Update()
    {
        // The component is only enabled when the freecam is active
        // Stop update of a quick menu an ImpUI is open with freecam 
        if (Imperium.Player.quickMenuManager.isMenuOpen) return;

        var scrollValue = Imperium.IngamePlayerSettings.playerInput.actions
            .FindAction("SwitchItem")
            .ReadValue<float>();

        ImpSettings.Freecam.FreecamMovementSpeed.Set(scrollValue switch
        {
            > 0 => Mathf.Min(ImpSettings.Freecam.FreecamMovementSpeed.Value + 1f, 1000),
            < 0 => Mathf.Max(ImpSettings.Freecam.FreecamMovementSpeed.Value - 1f, 1f),
            _ => ImpSettings.Freecam.FreecamMovementSpeed.Value
        });

        if (Imperium.InputBindings.FreecamMap["ArrowLeft"].IsPressed())
        {
            ImpSettings.Freecam.FreecamFieldOfView.Set(Mathf.Max(-360, ImpSettings.Freecam.FreecamFieldOfView.Value - 1));
        }

        if (Imperium.InputBindings.FreecamMap["ArrowRight"].IsPressed())
        {
            ImpSettings.Freecam.FreecamFieldOfView.Set(Mathf.Min(360, ImpSettings.Freecam.FreecamFieldOfView.Value + 1));
        }

        FreecamCamera.fieldOfView = ImpSettings.Freecam.FreecamFieldOfView.Value;

        var cameraTransform = transform;

        var rotation = Imperium.InputBindings.BaseMap["Look"].ReadValue<Vector2>();
        lookInput.x += rotation.x * 0.008f * Imperium.IngamePlayerSettings.settings.lookSensitivity;
        lookInput.y += rotation.y * 0.008f * Imperium.IngamePlayerSettings.settings.lookSensitivity;
        cameraTransform.rotation = Quaternion.Euler(-lookInput.y, lookInput.x, 0);

        var movement = Imperium.InputBindings.FreecamMap["Move"].ReadValue<Vector2>();
        var movementY = Imperium.InputBindings.FreecamMap["Ascend"].IsPressed() ? -1 :
            Imperium.InputBindings.FreecamMap["Descend"].IsPressed() ? 1 : 0;
        var deltaMove = new Vector3(movement.x, movementY, movement.y)
                        * (ImpSettings.Freecam.FreecamMovementSpeed.Value * Time.deltaTime);
        cameraTransform.Translate(deltaMove);
    }
}
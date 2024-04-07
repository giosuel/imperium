#region

using Imperium.Core;
using Imperium.Integration;
using Imperium.MonoBehaviours.ImpUI.LayerSelector;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.MonoBehaviours;

public class ImpFreecam : MonoBehaviour
{
    private Camera freecamCamera;
    private Camera gameplayCamera;
    private Vector2 lookInput;
    private LayerSelector layerSelector;

    private Vector3 lookOrigin;

    internal readonly ImpBinaryBinding IsFreecamEnabled = new(false);
    internal readonly ImpBinaryBinding IsMinicamEnabled = new(false);

    internal static ImpFreecam Create() => new GameObject("ImpFreecam").AddComponent<ImpFreecam>();

    private void Awake()
    {
        gameplayCamera = Imperium.Player.gameplayCamera;

        freecamCamera = gameObject.AddComponent<Camera>();
        freecamCamera.CopyFrom(gameplayCamera);
        freecamCamera.cullingMask = ImpSettings.Hidden.FreecamLayerMask.Value;
        freecamCamera.enabled = false;

        var layerSelectorObject = Instantiate(ImpAssets.LayerSelector, transform);
        layerSelector = layerSelectorObject.AddComponent<LayerSelector>();

        IsFreecamEnabled.onTrue += OnFreecamEnable;
        IsFreecamEnabled.onFalse += OnFreecamDisable;
        
        IsMinicamEnabled.onTrue += OnMinicamEnable;
        IsMinicamEnabled.onFalse += OnMinicamDisable;

        // Close Unity Explorer if open
        UnityExplorerIntegration.CloseUI();

        Imperium.InputBindings.BaseMap["Freecam"].performed += OnFreecamToggle;
        Imperium.InputBindings.BaseMap["Minicam"].performed += OnMinicamToggle;
        Imperium.InputBindings.FreecamMap["Reset"].performed += OnFreecamReset;
        Imperium.InputBindings.FreecamMap["LayerSelector"].performed += OnToggleLayerSelector;
        ImpSettings.Hidden.FreecamLayerMask.onUpdate += value => freecamCamera.cullingMask = value;
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
        
        freecamCamera.enabled = true;
        const float margin = 100f;
        freecamCamera.rect = new Rect(margin / Screen.width, 1 - margin / Screen.height - 0.4f, 0.4f, 0.4f);
    }

    private void OnMinicamDisable()
    {
        freecamCamera.enabled = false;
        freecamCamera.rect = new Rect(0, 0, 1, 1);
    }

    private void OnFreecamEnable()
    {
        Imperium.Interface.Close();
        
        if (IsMinicamEnabled.Value) IsMinicamEnabled.SetFalse();

        HUDManager.Instance.HideHUD(true);
        Imperium.InputBindings.FreecamMap.Enable();
        freecamCamera.enabled = true;
        Imperium.StartOfRound.SwitchCamera(freecamCamera);
        Imperium.Player.isFreeCamera = true;
        enabled = true;
    }

    private void OnFreecamDisable()
    {
        layerSelector.OnUIClose();
        
        HUDManager.Instance.HideHUD(false);
        Imperium.InputBindings.FreecamMap.Disable();
        freecamCamera.enabled = false;
        Imperium.StartOfRound.SwitchCamera(gameplayCamera);
        Imperium.Player.isFreeCamera = false;
        enabled = false;
    }

    private void OnFreecamReset(InputAction.CallbackContext callbackContext)
    {
        var playerTransform = Imperium.Player.gameplayCamera.transform;
        var freecamTransform = freecamCamera.transform;
        freecamTransform.position = playerTransform.position + Vector3.up * 2;
        lookOrigin = playerTransform.localRotation.eulerAngles;
        // freecamTransform.rotation = playerTransform.localRotation;

        ImpSettings.Hidden.FreecamFieldOfView.Set(ImpConstants.DefaultFOV);
    }

    private void OnToggleLayerSelector(InputAction.CallbackContext callbackContext)
    {
        ImpSettings.Hidden.FreecamLayerSelector.Set(!layerSelector.IsOpen);
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
        // Stop update of a the quick menu an ImpUI is open with freecam 
        if (Imperium.Player.quickMenuManager.isMenuOpen) return;

        var scrollValue = Imperium.IngamePlayerSettings.playerInput.actions
            .FindAction("SwitchItem")
            .ReadValue<float>();

        ImpSettings.Hidden.FreecamMovementSpeed.Set(scrollValue switch
        {
            > 0 => Mathf.Min(ImpSettings.Hidden.FreecamMovementSpeed.Value + 0.8f, 1000),
            < 0 => Mathf.Max(ImpSettings.Hidden.FreecamMovementSpeed.Value - 0.8f, 0),
            _ => ImpSettings.Hidden.FreecamMovementSpeed.Value
        });

        if (Imperium.InputBindings.FreecamMap["ArrowLeft"].IsPressed())
        {
            ImpSettings.Hidden.FreecamFieldOfView.Set(Mathf.Max(-360, ImpSettings.Hidden.FreecamFieldOfView.Value - 1));
        }

        if (Imperium.InputBindings.FreecamMap["ArrowRight"].IsPressed())
        {
            ImpSettings.Hidden.FreecamFieldOfView.Set(Mathf.Min(360, ImpSettings.Hidden.FreecamFieldOfView.Value + 1));
        }

        freecamCamera.fieldOfView = ImpSettings.Hidden.FreecamFieldOfView.Value;

        var cameraTransform = transform;

        var rotation = Imperium.InputBindings.FreecamMap["Look"].ReadValue<Vector2>();
        lookInput.x += rotation.x * 0.008f * Imperium.IngamePlayerSettings.settings.lookSensitivity;
        lookInput.y += rotation.y * 0.008f * Imperium.IngamePlayerSettings.settings.lookSensitivity;
        cameraTransform.rotation = Quaternion.Euler(-lookInput.y, lookInput.x, 0);

        var movement = Imperium.InputBindings.FreecamMap["Move"].ReadValue<Vector2>();
        var movementY = Imperium.InputBindings.FreecamMap["Ascend"].IsPressed() ? -1 :
            Imperium.InputBindings.FreecamMap["Descend"].IsPressed() ? 1 : 0;
        var deltaMove = new Vector3(movement.x, movementY, movement.y)
                        * (ImpSettings.Hidden.FreecamMovementSpeed.Value * Time.deltaTime);
        cameraTransform.Translate(deltaMove);
    }
}
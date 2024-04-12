#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.RenderingUI;
using Imperium.Util;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.LayerSelector;

/// <summary>
///     This UI is a bit special as it is controlled by the bindings <see cref="ImpSettings.Hidden.FreecamLayerSelector" />
///     and <see cref="ImpFreecam.IsFreecamEnabled" /> and the in-game variable <see cref="QuickMenuManager.isMenuOpen" />
///     that indicates if the quick menu or an Imperium UI is open.
/// </summary>
internal class LayerSelector : StandaloneUI
{
    private int selectedLayer;
    private GameObject layerTemplate;
    private readonly LayerToggle[] layerToggles = new LayerToggle[31];

    public override void Awake() => InitializeUI(isCollapsible: false, closeOnMovement: false);

    protected override void InitUI()
    {
        layerTemplate = content.Find("LayerItem").gameObject;
        layerTemplate.SetActive(false);

        for (var i = 0; i < layerToggles.Length; i++)
        {
            var toggleObj = Instantiate(layerTemplate, content);
            toggleObj.SetActive(true);
            layerToggles[i] = toggleObj.AddComponent<LayerToggle>();
            layerToggles[i].Init(LayerMask.LayerToName(i), i);
        }

        layerToggles[0].SetSelected(true);

        Imperium.InputBindings.FreecamMap["ArrowDown"].performed += OnLayerDown;
        Imperium.InputBindings.FreecamMap["ArrowUp"].performed += OnLayerUp;
        Imperium.InputBindings.FreecamMap["Select"].performed += OnLayerSelect;

        foreach (var toggle in layerToggles) toggle.UpdateIsOn(ImpSettings.Hidden.FreecamLayerMask.Value);
    }

    private void OnLayerDown(InputAction.CallbackContext callbackContext)
    {
        if (!IsOpen) return;

        layerToggles[selectedLayer].SetSelected(false);
        if (selectedLayer == layerToggles.Length - 1)
        {
            selectedLayer = 0;
        }
        else
        {
            selectedLayer++;
        }

        layerToggles[selectedLayer].SetSelected(true);
    }

    private void OnLayerUp(InputAction.CallbackContext callbackContext)
    {
        if (!IsOpen) return;

        layerToggles[selectedLayer].SetSelected(false);
        if (selectedLayer == 0)
        {
            selectedLayer = layerToggles.Length - 1;
        }
        else
        {
            selectedLayer--;
        }

        layerToggles[selectedLayer].SetSelected(true);
    }

    private void OnLayerSelect(InputAction.CallbackContext callbackContext)
    {
        if (!IsOpen) return;

        GameManager.PlayClip(ImpAssets.GrassClick);
        var newMask = ImpUtils.ToggleLayerInMask(ImpSettings.Hidden.FreecamLayerMask.Value, selectedLayer);
        ImpSettings.Hidden.FreecamLayerMask.Set(newMask);
        layerToggles[selectedLayer].UpdateIsOn(newMask);
    }

    private void Update()
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen)
        {
            if (IsOpen) OnUIClose();
        }
        else if (ImpSettings.Hidden.FreecamLayerSelector.Value && Imperium.Player.isFreeCamera)
        {
            if (!IsOpen) OnUIOpen();
        }
    }
}
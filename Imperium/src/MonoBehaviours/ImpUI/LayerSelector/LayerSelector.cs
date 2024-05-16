#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.MonoBehaviours.ImpUI.MapUI;
using Imperium.MonoBehaviours.ImpUI.RenderingUI;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.LayerSelector;

/// <summary>
/// This UI is a bit special as it is neither a child of another UI nor does it have a keybinding to open it at any time.
/// Instead, this UI can only be opened by the <see cref="ImpFreecam"/> and the <see cref="MapUI"/>.
/// </summary>
internal class LayerSelector : SingleplexUI
{
    private int selectedLayer;
    private GameObject layerTemplate;
    private readonly LayerToggle[] layerToggles = new LayerToggle[31];

    private ImpBinding<bool> IsEnabledBinding = new(false);
    private ImpBinding<int> LayerMaskBinding = new(0);
    
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
            var currentIndex = i;
            layerToggles[i].gameObject.AddComponent<ImpInteractable>().onEnter += () =>
            {
                layerToggles[selectedLayer].SetSelected(false);
                selectedLayer = currentIndex;
                layerToggles[selectedLayer].SetSelected(true);
            };
            layerToggles[i].GetComponent<Button>().onClick.AddListener(OnLayerSelect);
        }

        layerToggles[0].SetSelected(true);

        Imperium.InputBindings.FreecamMap["ArrowDown"].performed += OnLayerDown;
        Imperium.InputBindings.FreecamMap["ArrowUp"].performed += OnLayerUp;
        Imperium.InputBindings.FreecamMap["Select"].performed += OnLayerSelect;
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            layerTemplate.transform,
            new StyleOverride("Hover", Variant.FADED)
        );

        foreach (var toggle in layerToggles)
        {
            ImpThemeManager.Style(
                themeUpdate,
                toggle.transform,
                new StyleOverride("Hover", Variant.FADED)
            );
        }
    }

    internal void Bind(ImpBinding<bool> enabledBinding, ImpBinding<int> layerMaskBinding)
    {
        IsEnabledBinding = enabledBinding;
        LayerMaskBinding = layerMaskBinding;
        foreach (var toggle in layerToggles) toggle.UpdateIsOn(layerMaskBinding.Value);
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

        OnLayerSelect();
    }

    private void OnLayerSelect()
    {
        GameManager.PlayClip(ImpAssets.GrassClick);
        var newMask = ImpUtils.ToggleLayerInMask(LayerMaskBinding.Value, selectedLayer);
        LayerMaskBinding.Set(newMask);
        layerToggles[selectedLayer].UpdateIsOn(newMask);
    }

    private void Update()
    {
        if (Imperium.Player.quickMenuManager.isMenuOpen)
        {
            if (IsOpen) CloseUI();
        }
        else if (IsEnabledBinding is { Value: true } && Imperium.Player.isFreeCamera)
        {
            if (!IsOpen) OpenUI();
        }
    }
}
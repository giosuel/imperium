#region

using System;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ComponentManager;

internal class ComponentButton : MonoBehaviour
{
    private Button button;
    private TMP_Text text;

    private GameObject selectPanel;
    private Image icon;

    private float SpaceDoubleClickTimer;

    internal void Init(
        string componentName,
        Action onClick,
        Action onDoubleClick,
        IBinding<GameObject> selectedComponent,
        ImpBinding<ImpTheme> themeBinding
    )
    {
        text = transform.Find("Text").GetComponent<TMP_Text>();
        icon = transform.Find("Icon").GetComponent<Image>();
        button = transform.GetComponent<Button>();
        selectPanel = transform.Find("Selected").gameObject;
        selectPanel.SetActive(false);

        text.text = componentName;

        var iconSprite = ImpAssets.LoadSpriteFromFiles(componentName);
        if (iconSprite) icon.sprite = iconSprite;

        button.onClick.AddListener(() =>
        {
            selectedComponent.Set(gameObject);
            onClick();

            if (Time.realtimeSinceStartup - SpaceDoubleClickTimer < 0.63)
            {
                onDoubleClick();
            }

            SpaceDoubleClickTimer = Time.realtimeSinceStartup;
        });

        selectedComponent.onUpdate += comp => selectPanel.SetActive(comp == gameObject);

        themeBinding.onUpdate += OnThemeUpdate;
        OnThemeUpdate(themeBinding.Value);
    }

    private void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Selected", Variant.DARKER)
        );
    }

    internal void SetInteractable(bool isOn)
    {
        ImpUtils.Interface.ToggleImageActive(icon, isOn);
        ImpUtils.Interface.ToggleTextActive(text, isOn);
        button.interactable = isOn;
    }
}
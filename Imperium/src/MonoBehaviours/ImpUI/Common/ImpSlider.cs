#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

public class ImpSlider : MonoBehaviour
{
    private Slider slider;
    private TMP_Text indicatorText;

    private Func<float, string> indicatorFormatter;
    private string indicatorUnit;

    private float debounceTime;

    /// <summary>
    ///     Adds and binds an ImpSlider to a valid slider object
    /// </summary>
    /// >
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding">The binding that the value of the slider will be bound to</param>
    /// <param name="theme">The theme the slider will use</param>
    /// <param name="useLogarithmicScale">If the slider uses a logarithmic scale</param>
    /// <param name="indicatorUnit">Slider value unit (e.g. % or degrees)</param>
    /// <param name="indicatorDefaultValue">Overwrites the default value on the slider</param>
    /// <param name="indicatorFormatter">Formatter for custom indicator text</param>
    /// <param name="debounceTime">Debounce time for slider updates</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="options">Override options with provided labels</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the slider is interactable</param>
    internal static ImpSlider Bind(
        string path,
        Transform container,
        ImpBinding<float> valueBinding,
        ImpBinding<ImpTheme> theme = null,
        bool useLogarithmicScale = false,
        string indicatorUnit = "",
        float? indicatorDefaultValue = null,
        Func<float, string> indicatorFormatter = null,
        float debounceTime = 0f,
        bool interactableInvert = false,
        ImpBinding<List<string>> options = null,
        params ImpBinding<bool>[] interactableBindings
    )
    {
        var sliderObject = container.Find(path);
        if (!sliderObject) return null;

        var impSlider = sliderObject.gameObject.AddComponent<ImpSlider>();
        impSlider.debounceTime = debounceTime;
        impSlider.indicatorFormatter = indicatorFormatter;
        impSlider.indicatorUnit = indicatorUnit;

        indicatorFormatter ??= value => $"{Mathf.RoundToInt(value)}";

        var currentValue = useLogarithmicScale ? (float)Math.Log10(valueBinding.Value) : valueBinding.Value;

        if (options is { Value: not null, Value.Count: > 0})
        {
            impSlider.slider.minValue = 0;
            impSlider.slider.maxValue = options.Value.Count - 1;

            // Override indicator formatter to use provided labels instead
            indicatorFormatter = value => options.Value[(int)value];

            options.onUpdate += list => indicatorFormatter = value => list[(int)value];
        }

        impSlider.slider.onValueChanged.AddListener(value =>
        {
            // Fixes weird null pointer error after respawning UI
            if (!impSlider) return;

            var newValue = useLogarithmicScale ? (float)Math.Pow(10, value) : value;

            impSlider.indicatorText.text = $"{indicatorFormatter(newValue)}{indicatorUnit}";

            GameManager.PlayClip(ImpAssets.GrassClick);

            if (debounceTime > 0)
            {
                if (impSlider.debounceCoroutine != null) impSlider.StopCoroutine(impSlider.debounceCoroutine);
                impSlider.debounceCoroutine =
                    impSlider.StartCoroutine(impSlider.DebounceSlider(valueBinding, newValue));
            }
            else
            {
                valueBinding.Set(newValue);
            }
        });

        impSlider.slider.value = currentValue;
        impSlider.indicatorText.text = $"{indicatorFormatter(valueBinding.Value)}{indicatorUnit}";

        valueBinding.onUpdate += value =>
        {
            impSlider.slider.value = useLogarithmicScale ? (float)Math.Log10(value) : value;
            impSlider.indicatorText.text = $"{indicatorFormatter(value)}{indicatorUnit}";
        };

        if (sliderObject.Find("Reset"))
        {
            ImpButton.Bind(
                "Reset", sliderObject, () =>
                {
                    valueBinding.Reset();

                    var defaultValue = indicatorDefaultValue ?? valueBinding.DefaultValue;

                    impSlider.slider.value = useLogarithmicScale ? (float)Math.Log10(defaultValue) : defaultValue;
                    impSlider.indicatorText.text = $"{indicatorFormatter(defaultValue)}{indicatorUnit}";
                },
                theme: theme,
                interactableInvert: interactableInvert,
                interactableBindings: interactableBindings
            );
        }

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(impSlider.slider, interactableBindings.All(entry => entry.Value), interactableInvert);
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onUpdate += value =>
                {
                    ToggleInteractable(impSlider.slider, value, interactableInvert);
                };
            }
        }

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, sliderObject);
            OnThemeUpdate(theme.Value, sliderObject);
        }

        return impSlider;
    }

    private static void OnThemeUpdate(ImpTheme theme, Transform container)
    {
        ImpThemeManager.Style(
            theme,
            container,
            new StyleOverride("Slider/SliderArea", Variant.DARKER),
            new StyleOverride("Slider/SlideArea/Handle", Variant.FOREGROUND)
        );
    }

    public void Awake()
    {
        slider = transform.Find("Slider").GetComponent<Slider>();
        indicatorText = transform.Find("Slider/SlideArea/Handle/Text").GetComponent<TMP_Text>();
    }

    private Coroutine debounceCoroutine;

    // ReSharper disable Unity.PerformanceAnalysis
    // This is only called to debounce when the slider value has been changed
    private IEnumerator DebounceSlider(ImpBinding<float> binding, float value)
    {
        yield return new WaitForSeconds(debounceTime);
        binding.Set(value);
    }

    private void SetIndicatorText(float value)
    {
        indicatorText.text = indicatorFormatter != null
            ? indicatorFormatter(value)
            : $"{Mathf.RoundToInt(value)}{indicatorUnit}";
    }

    public void SetValue(float value)
    {
        slider.value = value;
        SetIndicatorText(value);
    }

    private static void ToggleInteractable(Selectable input, bool isOn, bool inverted)
    {
        input.interactable = inverted ? !isOn : isOn;
    }
}
#region

#endregion

#region

using System.Globalization;
using System.Linq;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.Common;

public abstract class ImpInput
{
    /// <summary>
    ///     Binds a Unity input field to an ImpBinding without adding a component (content type int)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding"></param>
    /// <param name="theme">The theme the input will use</param>
    /// <param name="min">Minimum input value</param>
    /// <param name="max">Maximum input value</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<int> valueBinding = null,
        IBinding<ImpTheme> theme = null,
        int min = int.MinValue,
        int max = int.MaxValue,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var inputObject = container.Find(path);
        if (!inputObject)
        {
            Imperium.IO.LogInfo($"[UI] Failed to input '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var input = inputObject.gameObject.GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.IntegerNumber;
        input.onValueChanged.AddListener(value => OnIntFieldInput(input, value, min, max));

        if (valueBinding != null)
        {
            input.text = valueBinding.Value.ToString();

            // Set binding to default value if input value is empty
            input.onSubmit.AddListener(value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    valueBinding.Set(valueBinding.DefaultValue);
                    input.text = valueBinding.DefaultValue.ToString();
                }
                else
                {
                    valueBinding.Set(int.Parse(value));
                }
            });

            valueBinding.onUpdate += value => input.text = value.ToString();
        }

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(input, interactableBindings.All(entry => entry.Value), interactableInvert);
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onUpdate += value => ToggleInteractable(input, value, interactableInvert);
            }
        }

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, inputObject);
        }

        return input;
    }

    /// <summary>
    ///     Binds a Unity input field to an ImpBinding without adding a component (content type float)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding"></param>
    /// <param name="theme">The theme the input will use</param>
    /// <param name="min">Minimum input value</param>
    /// <param name="max">Maximum input value</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<float> valueBinding = null,
        IBinding<ImpTheme> theme = null,
        float min = float.MinValue,
        float max = float.MaxValue,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var inputObject = container.Find(path);
        if (!inputObject) return null;

        var input = inputObject.gameObject.GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.DecimalNumber;
        input.onValueChanged.AddListener(value => OnFloatFieldInput(input, value, min, max));

        if (valueBinding != null)
        {
            input.text = valueBinding.Value.ToString(CultureInfo.InvariantCulture);
            input.onSubmit.AddListener(value =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    valueBinding.Set(valueBinding.DefaultValue);
                    input.text = valueBinding.DefaultValue.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    valueBinding.Set(float.Parse(value));
                }
            });
            valueBinding.onUpdate += value => input.text = value.ToString(CultureInfo.InvariantCulture);
        }

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(input, interactableBindings.All(entry => entry.Value), interactableInvert);
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onUpdate += value => ToggleInteractable(input, value, interactableInvert);
            }
        }

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, inputObject);
            OnThemeUpdate(theme.Value, inputObject);
        }

        return input;
    }

    private static void OnThemeUpdate(ImpTheme theme, Transform container)
    {
        ImpThemeManager.Style(
            theme,
            container,
            new StyleOverride("", Variant.FOREGROUND)
        );
    }

    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<string> valueBinding = null,
        IBinding<ImpTheme> theme = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var inputObject = container.Find(path);
        if (!inputObject) return null;

        var input = inputObject.gameObject.GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.Standard;

        if (valueBinding != null)
        {
            input.text = valueBinding.Value;

            // Set binding to default value if input value is empty
            input.onSubmit.AddListener(value => valueBinding.Set(value));

            valueBinding.onUpdate += value => input.text = value.ToString();
        }

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(input, interactableBindings.All(entry => entry.Value), interactableInvert);
            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.onUpdate += value => ToggleInteractable(input, value, interactableInvert);
            }
        }

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, inputObject);
        }

        return input;
    }

    /// <summary>
    ///     Creates a dummy field for decorative purposes (e.g. Outdoor deviation is a hard-coded value)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="text"></param>
    /// <param name="theme">The theme the input will use</param>
    /// <returns></returns>
    internal static void CreateStatic(
        string path,
        Transform container,
        string text,
        IBinding<ImpTheme> theme = null
    )
    {
        var inputObject = container.Find(path);
        var input = inputObject.gameObject.GetComponent<TMP_InputField>();
        input.text = text;
        input.interactable = false;
        input.contentType = TMP_InputField.ContentType.IntegerNumber;

        if (theme != null)
        {
            theme.onUpdate += value => OnThemeUpdate(value, inputObject);
            OnThemeUpdate(theme.Value, inputObject);
        }
    }

    private static void OnIntFieldInput(
        TMP_InputField field,
        string text,
        int min = int.MinValue,
        int max = int.MaxValue
    )
    {
        if (string.IsNullOrEmpty(text)) return;

        if (!int.TryParse(text, out var value))
        {
            field.text = min.ToString();
            return;
        }

        if (value > max)
        {
            field.text = max.ToString();
            return;
        }

        if (value < min)
        {
            field.text = min.ToString();
            return;
        }

        field.text = value.ToString();
    }

    private static void OnFloatFieldInput(
        TMP_InputField field,
        string text,
        float min = float.MinValue,
        float max = float.MaxValue
    )
    {
        if (string.IsNullOrEmpty(text)) return;

        var value = float.Parse(text);
        if (value > max)
        {
            field.text = max.ToString(CultureInfo.InvariantCulture);
            return;
        }

        if (value < min)
        {
            field.text = min.ToString(CultureInfo.InvariantCulture);
            return;
        }

        field.text = value.ToString(CultureInfo.InvariantCulture);
    }

    private static void ToggleInteractable(Selectable input, bool isOn, bool inverted)
    {
        input.interactable = inverted ? !isOn : isOn;
    }
}
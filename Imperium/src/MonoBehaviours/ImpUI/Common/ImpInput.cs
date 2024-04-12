#region

using System.Globalization;
using System.Linq;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

public abstract class ImpInput
{
    /// <summary>
    ///     Binds a Unity input field to an ImpBinding without adding a component (content type int)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding"></param>
    /// <param name="min">Minimum input value</param>
    /// <param name="max">Maximum input value</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        ImpBinding<int> valueBinding = null,
        int min = int.MinValue,
        int max = int.MaxValue,
        bool interactableInvert = false,
        params ImpBinding<bool>[] interactableBindings
    )
    {
        var inputObject = container.Find(path);
        var input = inputObject.gameObject.GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.IntegerNumber;
        input.onValueChanged.AddListener(value => OnIntFieldInput(input, value, min, max));

        if (valueBinding != null)
        {
            input.text = valueBinding.Value.ToString();

            // Set binding to default value if input value is empty
            input.onSubmit.AddListener(value =>
            {
                valueBinding.Set(string.IsNullOrEmpty(value) ? valueBinding.DefaultValue : int.Parse(value));
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

        return input;
    }

    /// <summary>
    ///     Binds a Unity input field to an ImpBinding without adding a component (content type float)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding"></param>
    /// <param name="min">Minimum input value</param>
    /// <param name="max">Maximum input value</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        ImpBinding<float> valueBinding = null,
        float min = float.MinValue,
        float max = float.MaxValue,
        bool interactableInvert = false,
        params ImpBinding<bool>[] interactableBindings
    )
    {
        var inputObject = container.Find(path);
        var input = inputObject.gameObject.GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.DecimalNumber;
        input.onValueChanged.AddListener(value => OnFloatFieldInput(input, value, min, max));

        if (valueBinding != null)
        {
            input.text = valueBinding.Value.ToString(CultureInfo.InvariantCulture);
            input.onSubmit.AddListener(value => valueBinding.Set(float.Parse(value)));
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

        return input;
    }

    /// <summary>
    ///     Creates a dummy field for decorative purposes (e.g. Outdoor deviation is a hard-coded value)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    internal static void CreateStatic(
        string path,
        Transform container,
        string text
    )
    {
        var inputObject = container.Find(path);
        var input = inputObject.gameObject.GetComponent<TMP_InputField>();
        input.text = text;
        input.interactable = false;
        input.contentType = TMP_InputField.ContentType.IntegerNumber;
    }

    private static void OnIntFieldInput(
        TMP_InputField field,
        string text,
        int min = int.MinValue,
        int max = int.MaxValue
    )
    {
        if (string.IsNullOrEmpty(text))
        {
            field.text = min.ToString();
            return;
        }

        var value = long.Parse(text);
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
        if (string.IsNullOrEmpty(text))
        {
            field.text = min.ToString(CultureInfo.InvariantCulture);
            return;
        }

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
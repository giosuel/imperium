#region

using System;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

/// <summary>
///     Clickable TMP text component that also supports has a custom hover effect
///     Note: Default hover effect is underlined text.
/// </summary>
public class ImpClickableText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Action callback;
    private TMP_Text textComponent;
    private string text;

    private Func<string, string> onHoverEffect;

    public void Init(string value, Action action, Func<string, string> hoverEffect = null)
    {
        textComponent = GetComponent<TMP_Text>();
        callback = action;
        text = value;
        onHoverEffect = hoverEffect ?? ImpUtils.RichText.Underlined;
        textComponent.text = text;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textComponent.text = onHoverEffect(text);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textComponent.text = text;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        callback.Invoke();
        textComponent.text = text;
    }
}
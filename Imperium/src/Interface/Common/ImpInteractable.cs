#region

using System;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

internal class ImpInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IDragHandler
{
    /// <summary>
    ///     OnPointerEnter
    /// </summary>
    internal event Action onEnter;

    /// <summary>
    ///     OnPointerExit
    /// </summary>
    internal event Action onExit;

    /// <summary>
    ///     OnPointerClick
    /// </summary>
    internal event Action onClick;

    /// <summary>
    ///     OnPointerDrag
    /// </summary>
    internal event Action<Vector3, Vector3> onDrag;

    public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke();
    public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
    public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke();
    public void OnDrag(PointerEventData eventData) => onDrag?.Invoke(eventData.position, eventData.pressPosition);
}
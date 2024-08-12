#region

using System;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace Imperium.Interface.Common;

internal class ImpInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler,
    IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IDragHandler
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
    ///     OnPointerDown
    /// </summary>
    internal event Action onDown;

    /// <summary>
    ///     OnPointerDown
    /// </summary>
    internal event Action onUp;

    /// <summary>
    ///     OnPointerMove
    /// </summary>
    internal event Action<Vector2> onOver;

    /// <summary>
    ///     OnPointerUp
    /// </summary>
    internal event Action<Vector2, Vector2> onDrag;

    public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke();
    public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
    public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke();
    public void OnPointerDown(PointerEventData eventData) => onDown?.Invoke();
    public void OnPointerUp(PointerEventData eventData) => onUp?.Invoke();
    public void OnPointerMove(PointerEventData eventData) => onOver?.Invoke(eventData.position);
    public void OnDrag(PointerEventData eventData) => onDrag?.Invoke(eventData.position, eventData.pressPosition);
}
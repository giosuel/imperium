#region

using System;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

internal class ImpInteractable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IDragHandler
{
    internal event Action onEnter;
    internal event Action onExit;
    internal event Action onClick;
    internal event Action<Vector3, Vector3> onDrag;

    public void OnPointerEnter(PointerEventData eventData) => onEnter?.Invoke();
    public void OnPointerExit(PointerEventData eventData) => onExit?.Invoke();
    public void OnPointerClick(PointerEventData eventData) => onClick?.Invoke();
    public void OnDrag(PointerEventData eventData) => onDrag?.Invoke(eventData.position, eventData.pressPosition);
}
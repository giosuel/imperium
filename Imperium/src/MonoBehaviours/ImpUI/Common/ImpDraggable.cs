using UnityEngine;
using UnityEngine.EventSystems;

namespace Imperium.MonoBehaviours.ImpUI.Common;

internal class ImpDraggable : MonoBehaviour, IDragHandler
{
    private Transform dragTarget;

    public void OnDrag(PointerEventData eventData)
    {
        dragTarget.position = (Vector2)dragTarget.position + eventData.delta;
    }

    internal void Init(Transform dragTargetTransform)
    {
        dragTarget = dragTargetTransform;
    }
}
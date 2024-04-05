#region

using UnityEngine.EventSystems;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.Common;

public class ImpDraggable : IDragHandler, IPointerClickHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        Imperium.Log.LogInfo("DRagging");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Imperium.Log.LogInfo("Clicking");
    }
}
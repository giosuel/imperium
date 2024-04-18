#region

using System;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SpawningUI;

public class SpawnEntryButton : MonoBehaviour, IPointerEnterHandler
{
    internal event Action onHover;

    public void OnPointerEnter(PointerEventData eventData)
    {
        onHover?.Invoke();
    }
}
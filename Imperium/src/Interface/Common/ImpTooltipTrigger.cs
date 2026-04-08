#region

using UnityEngine;

#endregion

namespace Imperium.Interface.Common;

internal class ImpTooltipTrigger : MonoBehaviour
{
    private ImpInteractable interactable;

    private void Awake()
    {
        interactable = gameObject.AddComponent<ImpInteractable>();
    }

    public void Init(TooltipDefinition definition)
    {
        interactable.onOver += position => definition.Tooltip.SetPosition(
            definition.Title,
            definition.Description,
            position,
            definition.HasAccess
        );
        interactable.onExit += () => definition.Tooltip.Deactivate();
    }
}
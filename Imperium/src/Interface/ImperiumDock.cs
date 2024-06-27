#region

using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface;

public class ImperiumDock : BaseUI
{
    internal void RegisterDockButton<T>(
        string buttonPath,
        ImpInterfaceManager dockInterfaceManager,
        string interfaceName,
        string interfaceDescription,
        params IBinding<bool>[] canOpenBindings
    ) where T : BaseUI
    {
        var button = ImpButton.Bind(
            buttonPath,
            container,
            () => dockInterfaceManager.Open<T>(),
            theme,
            isIconButton: true,
            playClickSound: false,
            tooltipDefinition: new TooltipDefinition
            {
                Tooltip = tooltip,
                Title = interfaceName,
                Description = interfaceDescription,
                HasAccess = true
            },
            interactableBindings: canOpenBindings
        );

        var buttonImage = button.GetComponent<Image>();
        buttonImage.enabled = false;
        dockInterfaceManager.OpenInterface.onUpdate += selectedInterface =>
        {
            if (!buttonImage) return;

            if (!selectedInterface)
            {
                buttonImage.enabled = false;
                return;
            }

            buttonImage.enabled = selectedInterface.GetType() == typeof(T);
        };
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("", Variant.BACKGROUND),
            new StyleOverride("Border", Variant.DARKER)
        );
    }
}
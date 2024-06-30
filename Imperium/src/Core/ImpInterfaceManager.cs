#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.Integration;
using Imperium.Interface;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core;

internal class ImpInterfaceManager : MonoBehaviour
{
    private readonly Dictionary<Type, BaseUI> interfaceControllers = [];

    private ImpTooltip tooltip;

    internal readonly ImpBinding<BaseUI> OpenInterface = new();

    private ImperiumDock imperiumDock;
    internal ImpBinding<ImpTheme> Theme { get; private set; }

    // We implement the exiting from UIs with bepinex controls as we have to differentiate
    // between the Escape and Tab character due to some UIs overriding the tab button callback.
    // Unfortunately the native OpenMenu action can be either of the keys.
    private KeyboardShortcut escShortcut = new(KeyCode.Escape);
    private KeyboardShortcut tabShortcut = new(KeyCode.Tab);

    internal static ImpInterfaceManager Create(ImpConfig<string> themeConfig)
    {
        var interfaceManager = new GameObject("ImpInterface").AddComponent<ImpInterfaceManager>();
        interfaceManager.Theme = new ImpBinding<ImpTheme>(ImpThemeManager.DefaultTheme);

        ImpThemeManager.BindTheme(themeConfig, interfaceManager.Theme);

        // Instantiate Imperium Tooltip
        interfaceManager.tooltip = Instantiate(
            ImpAssets.ImperiumTooltipObject, interfaceManager.transform
        ).AddComponent<ImpTooltip>();
        interfaceManager.tooltip.Init(interfaceManager.Theme, interfaceManager.tooltip);
        interfaceManager.tooltip.gameObject.SetActive(false);

        // Instantiate Imperium Dock
        interfaceManager.imperiumDock = Instantiate(
            ImpAssets.ImperiumDockObject,
            interfaceManager.transform
        ).AddComponent<ImperiumDock>();
        interfaceManager.imperiumDock.InitUI(interfaceManager.Theme, interfaceManager.tooltip);

        Imperium.IsSceneLoaded.onTrigger += interfaceManager.InvokeOnOpen;

        return interfaceManager;
    }

    private void Update()
    {
        if (!OpenInterface.Value) return;

        if (escShortcut.IsDown() || !OpenInterface.Value.IgnoreTab && tabShortcut.IsDown()) Close();
    }

    internal void RegisterInterface<T>(GameObject obj) where T : BaseUI
    {
        if (interfaceControllers.ContainsKey(typeof(T))) return;

        var interfaceObj = Instantiate(obj, transform).AddComponent<T>();
        interfaceObj.InitUI(Theme, tooltip);

        interfaceObj.interfaceManager = this;
        interfaceControllers[typeof(T)] = interfaceObj;
    }

    internal void RegisterInterface<T>(
        GameObject obj,
        string dockButtonPath,
        string interfaceName,
        string interfaceDescription,
        InputAction keybind,
        params IBinding<bool>[] canOpenBindings
    ) where T : BaseUI
    {
        RegisterInterface<T>(obj);

        if (imperiumDock)
        {
            imperiumDock.RegisterDockButton<T>(
                dockButtonPath,
                this,
                interfaceName,
                interfaceDescription,
                canOpenBindings
            );
        }

        keybind.performed += Toggle<T>;
        keybinds.Add((keybind, Toggle<T>));
    }

    private readonly List<(InputAction, Action<InputAction.CallbackContext>)> keybinds = [];

    private void OnDestroy()
    {
        foreach (var (action, function) in keybinds) action.performed -= function;
    }

    public void RefreshTheme() => Theme.Refresh();

    public void Unregister<T>()
    {
        Destroy(interfaceControllers[typeof(T)]);
        interfaceControllers.Remove(typeof(T));
    }

    public T Get<T>() where T : BaseUI
    {
        return (T)interfaceControllers.FirstOrDefault(controller => controller.Value is T).Value;
    }

    public void Open<T>(bool toggleCursorState = true, bool closeOthers = true)
    {
        Open(typeof(T), toggleCursorState, closeOthers);
    }

    public void Close() => Close(true);

    public void Close(bool toggleCursorState)
    {
        if (!OpenInterface.Value) return;

        imperiumDock.OnUIClose();
        OpenInterface.Value.OnUIClose();
        OpenInterface.Set(null);

        if (toggleCursorState)
        {
            tooltip.Deactivate();
            ImpUtils.Interface.ToggleCursorState(false);
        }
    }

    public void Toggle<T>(InputAction.CallbackContext _) => Toggle<T>();

    public void Toggle<T>(bool toggleCursorState = true, bool closeOthers = true)
    {
        if (!interfaceControllers.TryGetValue(typeof(T), out var controller)) return;

        if (controller.IsOpen)
        {
            Close(toggleCursorState);
        }
        else
        {
            Open<T>(toggleCursorState, closeOthers);
        }
    }

    private void InvokeOnOpen()
    {
        if (OpenInterface.Value) OpenInterface.Value.InvokeOnOpen();
    }

    public void Open(Type type, bool toggleCursorState = true, bool closeOthers = true)
    {
        if (!interfaceControllers.TryGetValue(type, out var controller))
        {
            Imperium.IO.LogError($"[Interface] Failed to open interface {type}");
            return;
        }

        if (controller.IsOpen || !controller.CanOpen() || Imperium.Player.isTypingChat) return;
        if (Imperium.Player.inTerminalMenu) Imperium.Terminal.QuitTerminal();

        controller.OnUIOpen();
        imperiumDock.OnUIOpen();

        OpenInterface.Set(controller);

        // Close Unity Explorer menus
        UnityExplorerIntegration.CloseUI();

        // Disable opening UIs when user is currently using terminal due to input selection overwrite
        Imperium.Player.quickMenuManager.CloseQuickMenu();

        if (closeOthers)
        {
            foreach (var interfaceController in interfaceControllers.Values)
            {
                if (interfaceController == controller || !interfaceController) continue;
                interfaceController.OnUIClose();
            }
        }

        if (toggleCursorState) ImpUtils.Interface.ToggleCursorState(true);
    }
}
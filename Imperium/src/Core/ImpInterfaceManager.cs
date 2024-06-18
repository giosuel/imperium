#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.Integration;
using Imperium.Interface;
using Imperium.MonoBehaviours.ImpUI;
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

    internal readonly ImpBinding<BaseUI> OpenInterface = new();

    private ImperiumDock imperiumDock;

    private readonly InputActionMap interfaceMap = new();
    private ImpBinding<ImpTheme> theme;

    // We implement the exiting from UIs with bepinex controls as we have to differentiate
    // between the Escape and Tab character due to some UIs overriding the tab button callback.
    // Unfortunately the native OpenMenu action can be either of the keys.
    private KeyboardShortcut escShortcut = new(KeyCode.Escape);
    private KeyboardShortcut tabShortcut = new(KeyCode.Tab);

    internal static ImpInterfaceManager Create(ImpBinding<ImpTheme> theme)
    {
        var interfaceManager = new GameObject("ImpInterface").AddComponent<ImpInterfaceManager>();
        interfaceManager.theme = theme;

        interfaceManager.imperiumDock = Instantiate(
            ImpAssets.ImperiumDockObject,
            interfaceManager.transform
        ).AddComponent<ImperiumDock>();
        interfaceManager.imperiumDock.InitUI(theme);

        return interfaceManager;
    }

    private void Update()
    {
        if (!OpenInterface.Value) return;

        if (escShortcut.IsDown() || !OpenInterface.Value.IgnoreTab && tabShortcut.IsDown()) Close();
    }

    internal void RegisterInterface<T>(
        GameObject obj,
        string interfaceName = null,
        string keybind = null,
        bool closeOnMovement = false
    ) where T : BaseUI
    {
        if (interfaceControllers.ContainsKey(typeof(T))) return;

        var interfaceObj = Instantiate(obj, transform).AddComponent<T>();
        interfaceObj.InitUI(theme, closeOnMovement);

        interfaceObj.interfaceManager = this;
        interfaceControllers[typeof(T)] = interfaceObj;

        if (imperiumDock && !string.IsNullOrEmpty(interfaceName))
        {
            imperiumDock.RegisterDockButton<T>(interfaceName, this);
        }

        if (!string.IsNullOrEmpty(keybind))
        {
            var bindingName = typeof(T).ToString();
            interfaceMap.AddAction(bindingName, binding: keybind);
            interfaceMap[bindingName].performed += _ => Toggle<T>();
        }
    }

    public void StartListening() => interfaceMap.Enable();

    public void StopListening() => interfaceMap.Disable();

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

        if (toggleCursorState) ImpUtils.Interface.ToggleCursorState(false);
    }

    public void Toggle<T>(bool toggleCursorState = true, bool closeOthers = true)
    {
        if (!interfaceControllers.TryGetValue(typeof(T), out var controller)) return;

        if (controller.IsOpen)
        {
            Close();
        }
        else
        {
            Open<T>(toggleCursorState, closeOthers);
        }
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
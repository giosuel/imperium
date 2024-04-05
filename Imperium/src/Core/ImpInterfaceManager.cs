#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.Integration;
using Imperium.MonoBehaviours.ImpUI;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core;

internal class ImpInterfaceManager : MonoBehaviour
{
    private readonly Dictionary<Type, BaseUI> interfaceControllers = [];
    private readonly Dictionary<int, Type> interfaceParents = [];
    internal readonly ImpBinding<BaseUI> OpenInterface = new();

    private readonly InputActionMap interfaceMap = new();

    // We implement the exiting from UIs with bepinex controls as we have to differentiate
    // between the Escape and Tab character due to some UIs overriding the tab button callback.
    // Unfortunately the native OpenMenu action can be either of the keys.
    private KeyboardShortcut escShortcut = new(KeyCode.Escape);
    private KeyboardShortcut tabShortcut = new(KeyCode.Tab);

    internal static ImpInterfaceManager Create() => new GameObject("ImpInterface").AddComponent<ImpInterfaceManager>();

    private void Update()
    {
        if (!OpenInterface.Value) return;

        if (escShortcut.IsDown() || (!OpenInterface.Value.ignoreTab && tabShortcut.IsDown()))
        {
            Close();
        }
    }

    internal void Register<T>(GameObject obj, string keybind = null, Type parent = null) where T : BaseUI
    {
        if (interfaceControllers.ContainsKey(typeof(T))) return;

        var interfaceObj = Instantiate(obj, transform).AddComponent<T>();
        if (parent != null) interfaceParents[interfaceObj.GetInstanceID()] = parent;
        interfaceObj.interfaceManager = this;
        interfaceControllers[typeof(T)] = interfaceObj;

        if (!string.IsNullOrEmpty(keybind))
        {
            var bindingName = typeof(T).ToString();
            interfaceMap.AddAction(bindingName, binding: keybind);
            interfaceMap[bindingName].performed += _ => Toggle<T>();
        }
    }

    public void StartListening() => interfaceMap.Enable();

    public void StopListening() => interfaceMap.Disable();

    public void Register<T, P>(GameObject obj, string keybind = null) where T : BaseUI
    {
        Register<T>(obj, keybind, typeof(P));
    }

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

        OpenInterface.Value.OnUIClose();

        if (toggleCursorState)
        {
            ImpUtils.Interface.ToggleCursorState(false);
        }

        OpenInterface.Set(null);
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
        var controller = interfaceControllers[type];
        if (controller.IsOpen || !controller.CanOpen() || Imperium.Player.isTypingChat) return;

        controller.OnUIOpen();
        OpenInterface.Set(controller);

        // Close Unity Explorer menus
        UnityExplorerIntegration.CloseUI();

        // Disable opening UIs when user is currently using terminal due to input selection overwrite
        if (Imperium.Player.inTerminalMenu) Imperium.Terminal.QuitTerminal();
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
using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ControlCenter;
using Imperium.Interface.ImperiumUI.Windows.Info;
using Imperium.Interface.ImperiumUI.Windows.MoonControl;
using Imperium.Interface.ImperiumUI.Windows.ObjectSettings;
using Imperium.Interface.ImperiumUI.Windows.Preferences;
using Imperium.Interface.ImperiumUI.Windows.ShipControl;
using Imperium.Interface.ImperiumUI.Windows.Teleportation;
using Imperium.Interface.ImperiumUI.Windows.Visualization;
using Imperium.MonoBehaviours.ImpUI.ImperiumUI.Windows;
using Imperium.MonoBehaviours.ImpUI.RenderingUI;
using Imperium.MonoBehaviours.ImpUI.SaveUI;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Imperium.Interface.ImperiumUI;

public class ImperiumUI : BaseUI
{
    private readonly Dictionary<Type, WindowDefinition> windowControllers = [];
    private readonly Dictionary<Type, ImpBinding<bool>> dockButtonBindings = [];

    private readonly ImpStack<WindowDefinition> controllerStack = [];

    protected override void InitUI()
    {
        RegisterImperiumWindow<VisualizationWindow>(
            ImpAssets.VisualizationWindowObject,
            "Left/Visualization",
            "Visualization"
        );
        RegisterImperiumWindow<TeleportationWindow>(
            ImpAssets.TeleportationWindowObject,
            "Left/Teleportation",
            "Teleportation",
            keybind: Imperium.InputBindings.InterfaceMap.TeleportWindow
        );
        RegisterImperiumWindow<RenderingWindow>(
            ImpAssets.RenderingWindowObject,
            "Left/Rendering",
            "Render Settings"
        );
        RegisterImperiumWindow<SaveEditorWindow>(
            ImpAssets.SaveEditorWindowObject,
            "Left/SaveEditor",
            "Save File Editor"
        );
        RegisterImperiumWindow<PreferencesWindow>(
            ImpAssets.PreferencesWindowObject,
            "Left/Preferences",
            "Imperium Preferences"
        );
        RegisterImperiumWindow<ControlCenterWindow>(
            ImpAssets.ControlCenterWindowObject,
            "Center/ControlCenter",
            "Imperium Control Center"
        );
        RegisterImperiumWindow<ObjectExplorerWindow>(
            ImpAssets.ObjectExplorerWindowObject,
            "Center/ObjectExplorer",
            "Object Explorer"
        );
        RegisterImperiumWindow<MoonControlWindow>(
            ImpAssets.MoonControlWindowObject,
            "Right/MoonControl",
            "Moon Control"
        );
        RegisterImperiumWindow<ShipControlWindow>(
            ImpAssets.ShipControlWindowObject,
            "Right/ShipControl",
            "Ship Control"
        );
        RegisterImperiumWindow<ObjectSettingsWindow>(
            ImpAssets.ObjectSettingsWindowObject,
            "Right/ObjectSettings",
            "Object Settings"
        );
        RegisterImperiumWindow<InfoWindow>(
            ImpAssets.InfoWindowObject,
            "Right/Info",
            "Info"
        );

        LoadLayout();
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("Dock/Left", Variant.BACKGROUND),
            new StyleOverride("Dock/Center", Variant.BACKGROUND),
            new StyleOverride("Dock/Right", Variant.BACKGROUND),
            new StyleOverride("Dock/Left/Border", Variant.DARKER),
            new StyleOverride("Dock/Center/Border", Variant.DARKER),
            new StyleOverride("Dock/Right/Border", Variant.DARKER)
        );
    }

    internal T Get<T>() where T : ImperiumWindow
    {
        return (T)windowControllers.FirstOrDefault(controller => controller.Value.Controller is T).Value.Controller;
    }

    private void RegisterImperiumWindow<T>(
        GameObject obj,
        string dockButtonPath,
        string windowName,
        string windowDescription = null,
        InputAction keybind = null
    ) where T : ImperiumWindow
    {
        if (windowControllers.ContainsKey(typeof(T))) return;

        var floatingWindow = Instantiate(obj.transform.Find("Window").gameObject, container).AddComponent<T>();

        var windowDefinition = new WindowDefinition
        {
            Controller = floatingWindow,
            IsOpen = false,
            WindowType = typeof(T)
        };
        windowControllers[typeof(T)] = windowDefinition;

        floatingWindow.InitWindow(theme, windowDefinition, tooltip);
        floatingWindow.onClose += OnCloseWindow<T>;
        floatingWindow.onOpen += OnOpenWindow<T>;
        floatingWindow.onFocus += OnFocusWindow<T>;

        var button = ImpButton.Bind(
            dockButtonPath,
            container.Find("Dock"),
            () =>
            {
                switch (windowDefinition.IsOpen)
                {
                    case true when GetFocusedWindow() == floatingWindow || windowControllers.Count < 2:
                        windowDefinition.Controller.Close();
                        break;
                    case true:
                        windowDefinition.Controller.FocusWindow();
                        break;
                    default:
                        windowDefinition.Controller.Open();
                        break;
                }
            },
            theme,
            isIconButton: true,
            playClickSound: false,
            tooltipDefinition: new TooltipDefinition
            {
                Tooltip = tooltip,
                Title = windowName,
                Description = windowDescription,
                HasAccess = true
            }
        );
        var buttonBinding = new ImpBinding<bool>(false);
        dockButtonBindings[typeof(T)] = buttonBinding;

        if (!button) return;

        var buttonImage = button.GetComponent<Image>();
        buttonImage.enabled = buttonBinding.Value;
        buttonBinding.onUpdate += isOn => buttonImage.enabled = isOn;

        if (keybind != null)
        {
            keybind.performed += _ =>
            {
                if (!IsOpen) Open();

                if (windowDefinition.IsOpen)
                {
                    windowDefinition.Controller.FocusWindow();
                }
                else
                {
                    windowDefinition.Controller.Open();
                }
            };
        }
    }

    protected override void OnClose() => SaveLayout();

    private void OnFocusWindow<T>() => controllerStack.MoveToBackOrAdd(windowControllers[typeof(T)]);
    private void OnOpenWindow<T>() => dockButtonBindings[typeof(T)].Set(true);
    private void OnCloseWindow<T>() => dockButtonBindings[typeof(T)].Set(false);

    private ImperiumWindow GetFocusedWindow() => controllerStack.Last().Controller;

    private void SaveLayout()
    {
        Imperium.Settings.Preferences.ImperiumWindowLayout.Set(JsonConvert.SerializeObject(controllerStack));
    }

    private void LoadLayout()
    {
        var layoutConfigString = Imperium.Settings.Preferences.ImperiumWindowLayout.Value;
        if (string.IsNullOrEmpty(layoutConfigString)) return;

        if (!ImpUtils.DeserializeJsonSafe<List<WindowDefinition>>(layoutConfigString, out var configList))
        {
            Imperium.IO.LogError("[UI] Failed to load ImperiumUI layout config. Invalid JSON detected.");
            return;
        }

        controllerStack.Clear();
        var controllers = new HashSet<Type>();

        foreach (var windowDefinition in configList)
        {
            var existingDefinition = windowControllers[windowDefinition.WindowType];
            if (!controllers.Add(existingDefinition.WindowType)) continue;

            // Propagate data from config to existing definition and add it to the stack
            existingDefinition.IsOpen = windowDefinition.IsOpen;
            existingDefinition.Position = windowDefinition.Position;
            existingDefinition.ScaleFactor = windowDefinition.ScaleFactor;
            controllerStack.Add(existingDefinition);

            // Update the dock button binding
            dockButtonBindings[existingDefinition.WindowType].Set(windowDefinition.IsOpen);

            // Inform the window of the new state
            existingDefinition.Controller.PlaceWindow(
                windowDefinition.Position,
                windowDefinition.ScaleFactor,
                windowDefinition.IsOpen
            );
        }
    }
}

public record WindowDefinition
{
    internal ImperiumWindow Controller { get; init; }
    public Type WindowType { get; init; }
    public System.Numerics.Vector2 Position { get; set; }
    public float ScaleFactor { get; set; } = 1;
    public bool IsOpen { get; set; }
}
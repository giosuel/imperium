using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ControlCenter;
using Imperium.Interface.ImperiumUI.Windows.Info;
using Imperium.Interface.ImperiumUI.Windows.MoonControl;
using Imperium.Interface.ImperiumUI.Windows.ObjectSettings;
using Imperium.Interface.ImperiumUI.Windows.Preferences;
using Imperium.Interface.ImperiumUI.Windows.ShipControl;
using Imperium.Interface.ImperiumUI.Windows.Teleportation;
using Imperium.Interface.ImperiumUI.Windows.Visualization;
using Imperium.MonoBehaviours.ImpUI;
using Imperium.MonoBehaviours.ImpUI.ImperiumUI.Windows;
using Imperium.MonoBehaviours.ImpUI.RenderingUI;
using Imperium.MonoBehaviours.ImpUI.SaveUI;
using Imperium.Types;
using Imperium.Util;
using Imperium.Util.Binding;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace Imperium.Interface.ImperiumUI;

public class ImperiumUI : BaseUI
{
    private readonly Dictionary<Type, WindowDefinition> windowControllers = [];
    private readonly Dictionary<Type, ImpBinding<bool>> dockButtonBindings = [];

    protected override void InitUI()
    {
        BindDockButton<VisualizationWindow>(ImpAssets.VisualizationWindowObject, "Left/Visualization");
        BindDockButton<TeleportationWindow>(ImpAssets.TeleportationWindowObject, "Left/Teleportation");
        BindDockButton<RenderingWindow>(ImpAssets.RenderingWindowObject, "Left/Rendering");
        BindDockButton<SaveEditorWindow>(ImpAssets.SaveEditorWindowObject, "Left/SaveEditor");
        BindDockButton<PreferencesWindow>(ImpAssets.PreferencesWindowObject, "Left/Preferences");
        BindDockButton<ControlCenterWindow>(ImpAssets.ControlCenterWindowObject, "Center/ControlCenter");
        BindDockButton<ObjectExplorerWindow>(ImpAssets.ObjectExplorerWindowObject, "Center/ObjectExplorer");
        BindDockButton<MoonControlWindow>(ImpAssets.MoonControlWindowObject, "Right/MoonControl");
        BindDockButton<ShipControlWindow>(ImpAssets.ShipControlWindowObject, "Right/ShipControl");
        BindDockButton<ObjectSettingsWindow>(ImpAssets.ObjectSettingsWindowObject, "Right/ObjectSettings");
        BindDockButton<InfoWindow>(ImpAssets.InfoWindowObject, "Right/Info");
    }

    internal T Get<T>() where T : ImperiumWindow
    {
        return (T)windowControllers.FirstOrDefault(controller => controller.Value.Controller is T).Value.Controller;
    }

    private void BindDockButton<T>(GameObject obj, string buttonPath) where T : ImperiumWindow
    {
        if (windowControllers.ContainsKey(typeof(T))) return;

        var floatingWindow = Instantiate(obj.transform.Find("Window").gameObject, container).AddComponent<T>();

        var windowDefinition = new WindowDefinition
        {
            Controller = floatingWindow,
            IsOpen = false
        };
        windowControllers[typeof(T)] = windowDefinition;

        floatingWindow.InitWindow(theme, windowDefinition);
        floatingWindow.onClose += OnCloseWindow<T>;
        floatingWindow.onOpen += OnOpenWindow<T>;

        var buttonBinding = new ImpBinding<bool>(false);
        dockButtonBindings[typeof(T)] = buttonBinding;

        var button = ImpButton.Bind(
            buttonPath,
            container.Find("Dock"),
            buttonBinding,
            theme,
            isIconButton: true
        );
        if (!button) return;

        var buttonImage = button.GetComponent<Image>();
        buttonImage.enabled = buttonBinding.Value;
        buttonBinding.onUpdate += isOn =>
        {
            if (windowDefinition.IsOpen == isOn) return;

            buttonImage.enabled = isOn;

            if (isOn)
            {
                windowDefinition.IsOpen = true;
                windowDefinition.Controller.Open();
            }
            else
            {
                windowDefinition.IsOpen = false;
                windowDefinition.Controller.Close();
            }
        };
    }

    private void OnOpenWindow<T>()
    {
        SaveLayout();
    }

    private void OnCloseWindow<T>()
    {
        SaveLayout();
        dockButtonBindings[typeof(T)].Set(false);
    }

    private void SaveLayout()
    {
        Imperium.IO.LogInfo($"Smogggel: {JsonUtility.ToJson(windowControllers)}");
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
}

internal record WindowDefinition
{
    internal ImperiumWindow Controller { get; init; }
    internal Vector2 Position { get; set; }
    internal float ScaleFactor { get; set; }
    internal bool IsOpen { get; set; }
}
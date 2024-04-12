#region

using System.Collections.Generic;

#endregion

namespace Imperium.MonoBehaviours.ImpUI;

/// <summary>
///     Complex UI that consists of multiple windows that have independent contents and title boxes.
/// </summary>
internal abstract class MultiplexUI : BaseUI
{
    private readonly Dictionary<string, BaseWindow> Windows = [];

    /// <summary>
    ///     Registers a new window within the current UI
    /// </summary>
    /// <param name="windowName"></param>
    /// <param name="isCollapsible">Whether the window has a collapse button or not</param>
    protected T RegisterWindow<T>(string windowName, bool isCollapsible = true) where T : BaseWindow
    {
        var windowObject = container.Find(windowName);
        var window = windowObject.gameObject.AddComponent<T>();
        window.RegisterWindow(this, isCollapsible);

        Windows[windowName] = window;

        return window;
    }

    protected override void OnOpen()
    {
        foreach (var window in Windows.Values) window.TriggerOnOpen();
    }
}
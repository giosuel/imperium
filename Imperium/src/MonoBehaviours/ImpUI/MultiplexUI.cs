#region

using System.Collections.Generic;
using Imperium.Types;
using Imperium.Util.Binding;

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
    /// <param name="themeBinding">The theme to use for the UI.</param>
    protected T RegisterWindow<T>(
        string windowName,
        ImpBinding<ImpTheme> themeBinding
    ) where T : BaseWindow
    {
        theme = themeBinding;

        var windowObject = container.Find(windowName);
        var window = windowObject.gameObject.AddComponent<T>();
        window.RegisterWindow(this, theme);

        Windows[windowName] = window;

        return window;
    }

    protected override void OnOpen()
    {
        foreach (var window in Windows.Values) window.TriggerOnOpen();
    }
}
#region

using System;
using Imperium.MonoBehaviours.ImpUI.Common;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI;

internal abstract class BaseWindow : MonoBehaviour
{
    protected Transform content;
    protected Transform titleBox;
    public event Action onOpen;

    protected BaseUI parentUI;

    /// <summary>
    ///     Registers a window and implements close / collapse functionality for the window.
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="isCollapsible"></param>
    public void RegisterWindow(BaseUI parent, bool isCollapsible)
    {
        parentUI = parent;
        content = transform.Find(isCollapsible ? "Main/Content" : "Content");

        // Use scrollbar content if content is scroll bar
        if (content.gameObject.GetComponent<ScrollRect>()) content = content.Find("Viewport/Content");

        titleBox = transform.Find("TitleBox");
        titleBox.Find("Close").GetComponent<Button>().onClick.AddListener(CloseUI);

        if (isCollapsible) ImpButton.CreateCollapse("Arrow", titleBox, transform.Find("Main"));

        onOpen += OnOpen;

        RegisterWindow();
    }

    public void TriggerOnOpen() => onOpen?.Invoke();

    protected void CloseUI() => parentUI.interfaceManager.Close();
    protected void OpenUI() => parentUI.interfaceManager.Open(parentUI.GetType());

    protected virtual void OnOpen()
    {
    }

    protected abstract void RegisterWindow();
}
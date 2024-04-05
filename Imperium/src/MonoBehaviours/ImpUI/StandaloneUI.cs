#region

using Imperium.MonoBehaviours.ImpUI.Common;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.MonoBehaviours.ImpUI;

/// <summary>
/// Simple UI with a single window.
/// Uses the component called "Window" in the UI container as the window component.
/// </summary>
internal abstract class StandaloneUI : BaseUI
{
    protected Transform content;
    private Transform titleBox;

    protected void InitializeUI(
        bool closeOnMovement = true,
        bool isCollapsible = false,
        bool ignoreTabInput = false
    )
    {
        InitTitleBox(isCollapsible);
        base.InitializeUI(closeOnMovement, ignoreTabInput);
    }

    /// <summary>
    /// Finds content and title box and implements title box close / collapse functionality.
    /// </summary>
    /// <param name="isCollapsible"></param>
    /// <param name="parent"></param>
    private void InitTitleBox(bool isCollapsible, BaseUI parent = null)
    {
        content = transform.Find(isCollapsible ? "Container/Window/Main/Content" : "Container/Window/Content");
        // Use scrollbar content if content is scroll bar
        if (content.gameObject.TryGetComponent<ScrollRect>(out _)) content = content.Find("Viewport/Content");

        titleBox = transform.Find("Container/Window/TitleBox");
        if (titleBox)
        {
            titleBox.Find("Close")?.GetComponent<Button>().onClick.AddListener(Imperium.Interface.Close);
            if (isCollapsible) ImpButton.CreateCollapse("Arrow", titleBox, transform.Find("Container/Window/Main"));
        }
    }
}
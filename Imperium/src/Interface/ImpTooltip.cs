using Imperium.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Imperium.Interface;

public class ImpTooltip : ImpWidget
{
    private Transform panel;
    private RectTransform panelRect;
    private TMP_Text headerText;
    private TMP_Text bodyText;
    private GameObject accessIcon;
    private GameObject accessText;

    private void Awake()
    {
        panel = transform.Find("Panel");
        panelRect = panel.GetComponent<RectTransform>();
        headerText = panel.Find("Header/Title").GetComponent<TMP_Text>();
        accessIcon = panel.Find("Header/Locked").gameObject;
        bodyText = panel.Find("Text").GetComponent<TMP_Text>();
        accessText = panel.Find("Access").gameObject;
    }

    public void UpdatePosition(Vector2 cursorPosition) => SetPosition(cursorPosition);

    public void Activate(string title, string text, bool hasAccess = true)
    {
        if (!Imperium.Settings.Preferences.ShowTooltips.Value) return;

        headerText.text = title;
        bodyText.text = text;

        accessIcon.gameObject.SetActive(!hasAccess);
        accessText.gameObject.SetActive(!hasAccess);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);

        gameObject.SetActive(true);
    }

    public void Deactivate() => gameObject.SetActive(false);

    private void SetPosition(Vector3 cursorPosition)
    {
        panel.transform.position = cursorPosition + new Vector3(15, -20, 0);
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            panel,
            new StyleOverride("", Variant.BACKGROUND),
            new StyleOverride("Border", Variant.DARKER)
        );
    }
}

public record TooltipDefinition
{
    public ImpTooltip Tooltip { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool HasAccess { get; set; }
}
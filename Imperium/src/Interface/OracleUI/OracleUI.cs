#region

using Imperium.API.Types.Networking;
using Imperium.Types;

#endregion

namespace Imperium.Interface.OracleUI;

internal class OracleUI : BaseUI
{
    private readonly OracleCycleEntry[] entries = new OracleCycleEntry[10];

    private void OnOracleUpdate(OracleState state)
    {
        for (var i = 0; i < entries.Length; i++) entries[i].SetState(state, i);
    }

    protected override void InitUI()
    {
        var content = container.Find("Viewport/Content");
        var cycleRowTemplate = content.Find("CycleRowTemplate").gameObject;
        cycleRowTemplate.SetActive(false);

        var cycleTemplate = content.Find("CycleRowTemplate/CycleTemplate").gameObject;
        cycleTemplate.SetActive(false);

        // Initial cycle with only indoor
        entries[0] = content.Find("Header/InitialCycle").gameObject.AddComponent<OracleCycleEntry>();
        entries[0].Initialize(theme);

        var index = 1;
        for (var i = 1; i <= 3; i++)
        {
            var rowObject = Instantiate(cycleRowTemplate, content);
            rowObject.SetActive(true);
            for (var j = 1; j <= 3; j++)
            {
                var entryObject = Instantiate(cycleTemplate, rowObject.transform);
                entryObject.SetActive(true);
                entries[index] = entryObject.AddComponent<OracleCycleEntry>();
                entries[index].Initialize(theme);
                index++;
            }
        }

        Imperium.Oracle.State.onUpdate += OnOracleUpdate;
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container.Find("Viewport/Content"),
            new StyleOverride("Scrollbar", Variant.DARKEST),
            new StyleOverride("Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );
    }

    public override bool CanOpen()
    {
        if (!Imperium.IsSceneLoaded.Value)
        {
            Imperium.IO.Send(
                "Nothing is spawning out here ._.",
                title: "Oracle",
                type: NotificationType.Required
            );
            return false;
        }

        return true;
    }
}
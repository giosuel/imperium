#region

using Imperium.Core;
using Imperium.MonoBehaviours.ImpUI.Common;
using Imperium.Types;
using Imperium.Util.Binding;
using TMPro;
using UnityEngine;
using ContentType = TMPro.TMP_InputField.ContentType;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.SaveUI;

internal class SaveUI : SingleplexUI
{
    private Transform generalSaveContainer;
    private Transform currentSaveContainer;
    private Transform CurrentSaveShipContainer;

    private Transform inputTemplate;
    private Transform toggleTemplate;

    protected override void InitUI()
    {
        generalSaveContainer = content.Find("GeneralSave");
        currentSaveContainer = content.Find("CurrentSave");
        CurrentSaveShipContainer = content.Find("CurrentSaveShip");

        inputTemplate = generalSaveContainer.Find("InputTemplate");
        toggleTemplate = generalSaveContainer.Find("ToggleTemplate");

        inputTemplate.gameObject.SetActive(false);
        toggleTemplate.gameObject.SetActive(false);

        InitGeneralSave();

        if (ES3.FileExists(GameNetworkManager.Instance.currentSaveFileName))
        {
            InitCurrentSave();
        }
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container.Find("Window/Content"),
            new StyleOverride("Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );
    }

    private void InitGeneralSave()
    {
        const string generalSave = ImpConstants.GeneralSaveFile;

        BindTextSetting("PlayerXPNum", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("PlayerLevel", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("FinishedShockMinigame", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("SelectedFile", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("TimesLanded", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindBooleanSetting("HostSettings_Public", generalSave, generalSaveContainer);
        BindTextSetting("HostSettings_Name", generalSave, generalSaveContainer);
        BindTextSetting("LastVerPlayed", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindBooleanSetting("SpiderSafeMode", generalSave, generalSaveContainer);
        BindBooleanSetting("InvertYAxis", generalSave, generalSaveContainer);
        BindTextSetting("ScreenMode", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("FPSCap", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("Bindings", generalSave, generalSaveContainer);
        BindTextSetting("CurrentMic", generalSave, generalSaveContainer);
        BindTextSetting("CurrentMic", generalSave, generalSaveContainer);
        BindBooleanSetting("PushToTalk", generalSave, generalSaveContainer);
        BindBooleanSetting("MicEnabled", generalSave, generalSaveContainer);
        BindTextSetting("LookSens", generalSave, generalSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("MasterVolume", generalSave, generalSaveContainer, ContentType.DecimalNumber);
        BindTextSetting("Gamma", generalSave, generalSaveContainer, ContentType.DecimalNumber);
        BindBooleanSetting("StartInOnlineMode", generalSave, generalSaveContainer);
        BindBooleanSetting("PlayerFinishedSetup", generalSave, generalSaveContainer);
        BindBooleanSetting("LC_StorageTip", generalSave, generalSaveContainer);
        BindBooleanSetting("PlayedDungeonEntrance0", generalSave, generalSaveContainer);
        BindBooleanSetting("PlayedDungeonEntrance1", generalSave, generalSaveContainer);
        BindBooleanSetting("LC_MoveObjectsTip", generalSave, generalSaveContainer);
        BindBooleanSetting("LC_EclipseTip", generalSave, generalSaveContainer);
        BindBooleanSetting("LC_LightningTip", generalSave, generalSaveContainer);
        BindBooleanSetting("LCTip_SecureDoors", generalSave, generalSaveContainer);
        BindBooleanSetting("LCTip_SellScrap", generalSave, generalSaveContainer);
        BindBooleanSetting("LCTip_UseManual", generalSave, generalSaveContainer);
        BindBooleanSetting("LC_IntroTip1", generalSave, generalSaveContainer);
        BindBooleanSetting("LC_0DaysWarning", generalSave, generalSaveContainer);
        BindBooleanSetting("HasUsedTerminal", generalSave, generalSaveContainer);
    }

    private void InitCurrentSave()
    {
        var currentSave = GameNetworkManager.Instance.currentSaveFileName;

        BindTextSetting("GroupCredits", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("CurrentPlanetID", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("ProfitQuota", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("QuotasPassed", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("QuotaFulfilled", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("FileGameVers", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("Stats_StepsTaken", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("Stats_ValueCollected", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("Stats_Deaths", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("Stats_DaysSpent", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("RandomSeed", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindTextSetting("DeadlineTime", currentSave, currentSaveContainer, ContentType.IntegerNumber);
        BindBooleanSetting("ShipUnlockStored_Plushie pajama man", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Goldfish", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Welcome mat", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_JackOLantern", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockMoved_Inverse Teleporter", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Inverse Teleporter", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Loud horn", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Signal translator", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockMoved_Terminal", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Bunkbeds", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockMoved_Bunkbeds", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Romantic table", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Table", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Record player", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Shower", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Toilet", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_File Cabinet", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockMoved_Cupboard", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Cupboard", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockStored_Television", currentSave, CurrentSaveShipContainer);
        BindBooleanSetting("ShipUnlockMoved_Teleporter", currentSave, CurrentSaveShipContainer);
    }

    private void BindTextSetting(
        string settingName,
        string fileName,
        Transform parent,
        ContentType contentType = ContentType.Standard)
    {
        if (!ES3.KeyExists(settingName, fileName)) return;

        var element = Instantiate(inputTemplate, parent);
        element.gameObject.SetActive(true);
        element.Find("Title").GetComponent<TMP_Text>().text = settingName;

        var binding = new ImpBinding<string>(ES3.Load(settingName, fileName).ToString());
        binding.onUpdate += value => ES3.Save(settingName, value, fileName);
        ImpInput.Bind("Input", element, binding, theme);
    }

    private void BindBooleanSetting(string settingName, string fileName, Transform parent)
    {
        if (!ES3.KeyExists(settingName, fileName)) return;

        var element = Instantiate(toggleTemplate, parent);
        element.gameObject.SetActive(true);
        element.Find("Title").GetComponent<TMP_Text>().text = settingName;

        var binding = new ImpBinaryBinding(
            ES3.KeyExists(settingName, fileName) && (bool)ES3.Load(settingName, fileName)
        );
        binding.onUpdate += value => ES3.Save(settingName, value, fileName);
        ImpToggle.Bind("", element, binding, theme);
    }
}
#region

using Imperium.Core;
using Imperium.Types;
using Imperium.Util;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.MonoBehaviours.ImpUI.MapUI;

internal class MinimapOverlay : SingleplexUI
{
    internal Rect CameraRect { get; private set; }
    private TMP_Text positionText;
    private TMP_Text timeText;
    private TMP_Text envText;
    private TMP_Text rotationText;
    private GameObject infoPanel;
    private Transform mapBorder;
    private Canvas canvas;

    private GameObject compass;
    private Transform compassNorth;
    private Transform compassEast;
    private Transform compassSouth;
    private Transform compassWest;

    private const float BorderThickness = 2f;

    protected override void InitUI()
    {
        mapBorder = container.Find("MapBorder");
        timeText = container.Find("MapBorder/Clock/Text").GetComponent<TMP_Text>();
        envText = container.Find("MapBorder/Clock/Day/Text").GetComponent<TMP_Text>();
        positionText = container.Find("MapBorder/InfoPanel/Position").GetComponent<TMP_Text>();
        rotationText = container.Find("MapBorder/InfoPanel/Rotation").GetComponent<TMP_Text>();
        envText = container.Find("MapBorder/InfoPanel/Location").GetComponent<TMP_Text>();
        timeText = container.Find("MapBorder/InfoPanel/Time").GetComponent<TMP_Text>();
        infoPanel = container.Find("MapBorder/InfoPanel").gameObject;

        canvas = GetComponent<Canvas>();

        var baseCanvasScale = canvas.scaleFactor;
        ImpSettings.Map.MinimapScale.onUpdate += value => InitMapScale(baseCanvasScale * value);

        InitMapScale(baseCanvasScale * ImpSettings.Map.MinimapScale.Value);
        InitCompass();
    }


    private void InitMapScale(float scaleFactor)
    {
        canvas.scaleFactor = scaleFactor;

        var mapBorderPosition = mapBorder.gameObject.GetComponent<RectTransform>().position;
        var mapBorderSize = mapBorder.gameObject.GetComponent<RectTransform>().sizeDelta;
        var mapContainerWidth = mapBorderSize.x * canvas.scaleFactor - BorderThickness * 2;
        var mapContainerHeight = mapBorderSize.y * canvas.scaleFactor - BorderThickness * 2;

        CameraRect = new Rect(
            (mapBorderPosition.x + BorderThickness) / Screen.width,
            (mapBorderPosition.y + BorderThickness) / Screen.height,
            mapContainerWidth / Screen.width,
            mapContainerHeight / Screen.height
        );

        if (IsOpen) Imperium.Map.Camera.rect = CameraRect;
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container.Find("MapBorder"),
            new StyleOverride("", Variant.DARKER),
            new StyleOverride("Compass", Variant.FOREGROUND),
            new StyleOverride("Compass/Icon", Variant.FOREGROUND),
            new StyleOverride("InfoPanel", Variant.BACKGROUND),
            new StyleOverride("InfoPanel/Border", Variant.DARKER)
        );
        ImpThemeManager.StyleText(
            themeUpdate,
            container.Find("MapBorder"),
            new StyleOverride("Compass/North", Variant.FOREGROUND),
            new StyleOverride("Compass/East", Variant.FOREGROUND),
            new StyleOverride("Compass/South", Variant.FOREGROUND),
            new StyleOverride("Compass/West", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Position", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/PositionTitle", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Rotation", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/RotationTitle", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Location", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/LocationTitle", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Time", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/TimeTitle", Variant.FOREGROUND)
        );
    }

    private void InitCompass()
    {
        compass = container.Find("MapBorder/Compass").gameObject;
        compass.SetActive(ImpSettings.Map.CompassEnabled.Value);
        ImpSettings.Map.CompassEnabled.onUpdate += compass.SetActive;

        compassNorth = compass.transform.Find("North");
        compassEast = compass.transform.Find("East");
        compassSouth = compass.transform.Find("West");
        compassWest = compass.transform.Find("South");
    }

    private void Update()
    {
        infoPanel.SetActive(ImpSettings.Map.MinimapInfoPanel.Value);

        // Only update the panel when it's activated
        if (ImpSettings.Map.MinimapInfoPanel.Value)
        {
            var playerPosition = Imperium.Player.transform.position;
            positionText.text = $"{ImpUtils.FormatVector(playerPosition, separator: "/", roundDigits: 0)}";

            var playerRotation = Imperium.Player.gameplayCamera.transform.rotation.eulerAngles;
            rotationText.text =
                $"{ImpUtils.FormatVector(playerRotation, separator: "/", roundDigits: 0, unit: "\u00b0")}";

            var time = ImpUtils.FormatDayTime(Imperium.TimeOfDay.currentDayTime);
            var daysSpent = Imperium.StartOfRound.gameStats.daysSpent;
            timeText.text = $"{time} / Day {daysSpent}";

            envText.text = PlayerManager.GetLocationText(Imperium.Player);
        }

        // Only update the compass when it's activated
        if (ImpSettings.Map.CompassEnabled.Value)
        {
            var rotationY = Imperium.Map.Camera.transform.rotation.eulerAngles.y;
            compass.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationY));

            // Counter-rotate to keep the labels upright
            compassNorth.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassEast.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassSouth.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassWest.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
        }

        // Automatically open this UI when nothing else is open
        if ((Imperium.Player.quickMenuManager.isMenuOpen &&
             !Imperium.Interface.Get<MinimapSettings.MinimapSettings>().IsOpen)
            || !ImpSettings.Map.MinimapEnabled.Value
            || Imperium.Freecam.IsFreecamEnabled.Value)
        {
            if (IsOpen) CloseUI();
        }
        else
        {
            if (!IsOpen) OpenUI();
        }
    }
}
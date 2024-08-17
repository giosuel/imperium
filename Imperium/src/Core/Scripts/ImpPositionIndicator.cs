#region

using System;
using Imperium.Util;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Core.Scripts;

public class ImpPositionIndicator : MonoBehaviour
{
    private Transform origin;
    private GameObject indicator;
    private LineRenderer arcLine;
    private bool castToGround;

    private Action<Vector3> registeredCallback;

    internal bool IsActive { get; private set; }

    internal static ImpPositionIndicator Create() => Instantiate(
        ImpAssets.PositionIndicatorObject
    ).AddComponent<ImpPositionIndicator>();

    private void Awake()
    {
        indicator = transform.Find("Indicator").gameObject;
        arcLine = indicator.GetComponent<LineRenderer>();

        Deactivate();
    }

    internal void Activate(Action<Vector3> callback, Transform originTransform = null, bool castGround = true)
    {
        castToGround = castGround;
        registeredCallback = callback;
        origin = originTransform ? originTransform : Imperium.Player.gameplayCamera.transform;

        IsActive = true;
        indicator.SetActive(true);

        Imperium.Interface.Close();

        Imperium.IngamePlayerSettings.playerInput.actions["ActivateItem"].performed += OnLeftClick;
        Imperium.IngamePlayerSettings.playerInput.actions["OpenMenu"].Disable();
        Imperium.IngamePlayerSettings.playerInput.actions["PingScan"].Disable();
        Imperium.InputBindings.StaticMap["RightClick"].performed += OnExitAction;
        Imperium.InputBindings.StaticMap["Escape"].performed += OnExitAction;
        Imperium.InputBindings.BaseMap.TapeMeasure.Disable();
    }

    internal void Deactivate()
    {
        IsActive = false;
        indicator.SetActive(false);
        registeredCallback = null;

        Imperium.IngamePlayerSettings.playerInput.actions["ActivateItem"].performed -= OnLeftClick;
        Imperium.IngamePlayerSettings.playerInput.actions["OpenMenu"].Enable();
        Imperium.IngamePlayerSettings.playerInput.actions["PingScan"].Enable();
        Imperium.InputBindings.StaticMap["RightClick"].performed -= OnExitAction;
        Imperium.InputBindings.StaticMap["Escape"].performed -= OnExitAction;
        Imperium.InputBindings.BaseMap.TapeMeasure.Enable();
    }

    private void OnExitAction(InputAction.CallbackContext context) => Deactivate();

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (IsActive) SubmitIndicator();
    }

    private void SubmitIndicator()
    {
        registeredCallback?.Invoke(indicator.transform.position);
        Deactivate();
    }

    private void Update()
    {
        if (!IsActive || !origin) return;

        var forward = origin.forward;
        var rotateDegrees = Imperium.Settings.Preferences.LeftHandedMode.Value ? -90 : 90;
        var startPosition = origin.position + Quaternion.AngleAxis(rotateDegrees, Vector3.up) * forward;
        var ray = new Ray(startPosition, forward);

        // Raycast to player look position
        Physics.Raycast(ray, out var hitInfo, 1000, ImpConstants.IndicatorMask);

        var endPosition = hitInfo.point;

        if (castToGround)
        {
            // Raycast from look position to the ground below
            Physics.Raycast(
                new Ray(hitInfo.point, Vector3.down),
                out var groundInfo, 100, ImpConstants.IndicatorMask
            );

            endPosition = hitInfo.normal.y > 0 ? hitInfo.point : groundInfo.point;
        }

        indicator.transform.position = endPosition;
        arcLine.transform.RotateAround(endPosition, Vector3.up, 80 * Time.deltaTime);

        arcLine.positionCount = 100;
        arcLine.startWidth = 0.08f;
        arcLine.endWidth = 0.08f;

        if (endPosition == Vector3.zero)
        {
            arcLine.gameObject.SetActive(false);
            return;
        }

        arcLine.gameObject.SetActive(true);

        for (var i = 0; i < 100; i++)
        {
            var position2D = ImpMath.SampleQuadraticBezier(
                startPosition.y,
                endPosition.y,
                startPosition.y + Math.Clamp(Math.Abs(forward.y * 20), 0, 10),
                i / 100f
            );
            arcLine.SetPosition(i, new Vector3(
                Mathf.Lerp(startPosition.x, endPosition.x, i / 100f),
                position2D,
                Mathf.Lerp(startPosition.z, endPosition.z, i / 100f))
            );
        }
    }
}
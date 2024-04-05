#region

using System;
using Imperium.Core;
using Imperium.Util;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.MonoBehaviours.VisualizerObjects;

public class PositionIndicator : MonoBehaviour
{
    private GameObject indicatorObject;
    private LineRenderer indicatorLineRenderer;
    private Transform origin;

    private Action<Vector3> registeredCallback;

    private bool _isActive;

    private bool isActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            indicatorObject.SetActive(value);
        }
    }

    private void Awake()
    {
        indicatorObject = transform.Find("IndicatorObject").gameObject;
        indicatorLineRenderer = indicatorObject.GetComponent<LineRenderer>();

        HideIndicator();
    }

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (isActive) SubmitIndicator();
    }

    public void Activate(Action<Vector3> callback, Transform originTransform = null)
    {
        registeredCallback = callback;
        origin = originTransform ? originTransform : Imperium.Player.gameplayCamera.transform;
        ShowIndicator();
    }

    private void SubmitIndicator()
    {
        registeredCallback?.Invoke(indicatorObject.transform.position);
        HideIndicator();
    }

    private void ShowIndicator()
    {
        isActive = true;

        Imperium.Interface.Close();

        Imperium.IngamePlayerSettings.playerInput.actions["ActivateItem"].performed += OnLeftClick;
        Imperium.IngamePlayerSettings.playerInput.actions["OpenMenu"].Disable();
    }

    public void HideIndicator()
    {
        isActive = false;
        registeredCallback = null;

        Imperium.IngamePlayerSettings.playerInput.actions["ActivateItem"].performed -= OnLeftClick;
        Imperium.IngamePlayerSettings.playerInput.actions["OpenMenu"].Enable();
    }

    public void Update()
    {
        if (!isActive || !origin) return;

        var forward = origin.forward;
        var rotateDegrees = ImpSettings.Preferences.LeftHandedMode.Value ? -90 : 90;
        var startPosition = origin.position + Quaternion.AngleAxis(rotateDegrees, Vector3.up) * forward;
        var ray = new Ray(startPosition, forward);

        // Raycast to player look position
        Physics.Raycast(ray, out var hitInfo, 1000, ImpConstants.IndicatorMask);

        // Raycast from look position to the ground below
        Physics.Raycast(
            new Ray(hitInfo.point, Vector3.down),
            out var groundInfo, 100, ImpConstants.IndicatorMask
        );

        var endPosition = hitInfo.normal.y > 0 ? hitInfo.point : groundInfo.point;

        indicatorObject.transform.position = endPosition;
        if (groundInfo.normal != Vector3.zero)
        {
            indicatorObject.transform.rotation = Quaternion.LookRotation(-groundInfo.normal, Vector3.up);
        }

        indicatorLineRenderer.positionCount = 100;
        indicatorLineRenderer.startWidth = 0.08f;
        indicatorLineRenderer.endWidth = 0.08f;

        for (var i = 0; i < 100; i++)
        {
            var position2D = ImpUtils.Math.SampleQuadraticBezier(
                startPosition.y,
                endPosition.y,
                startPosition.y + Math.Clamp(Math.Abs(forward.y * 20), 0, 10),
                i / 100f
            );
            indicatorLineRenderer.SetPosition(i, new Vector3(
                Mathf.Lerp(startPosition.x, endPosition.x, i / 100f),
                position2D,
                Mathf.Lerp(startPosition.z, endPosition.z, i / 100f))
            );
        }
    }
}
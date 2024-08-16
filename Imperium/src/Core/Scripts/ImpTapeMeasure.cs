#region

using Imperium.Util;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Scripts;

public class ImpTapeMeasure : MonoBehaviour
{
    private GameObject indicator;
    private RectTransform canvasRect;
    private RectTransform panelRect;
    private TMP_Text distanceText;

    private bool isActive;

    private Vector3? startPosition;
    private Vector3? endPosition;
    private Vector3 currentLookPosition;
    private Vector3 currentLookNormal;

    private GameObject startMarker;
    private GameObject endMarker;

    private LineRenderer tapeLine;

    private float rotationZ;

    private LayerMask tapeLayerMask;
    private Camera originCamera;

    private readonly Vector3[] Axes =
    [
        Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
    ];

    internal static ImpTapeMeasure Create() => Instantiate(ImpAssets.TapeIndicatorObject).AddComponent<ImpTapeMeasure>();

    private void Awake()
    {
        indicator = transform.Find("Indicator").gameObject;
        canvasRect = transform.Find("Canvas").GetComponent<RectTransform>();
        panelRect = transform.Find("Canvas/Panel").GetComponent<RectTransform>();
        distanceText = transform.Find("Canvas/Panel/Number").GetComponent<TMP_Text>();

        Imperium.InputBindings.BaseMap.TapeMeasure.performed += OnTapeOpen;

        var game = gameObject.GetComponent<NetworkObject>();

        tapeLine = ImpGeometry.CreateLine(
            transform,
            useWorldSpace: true,
            startColor: new Color(1, 1, 1),
            endColor: new Color(1, 1, 1),
            thickness: 0.03f
        );
        startMarker = ImpGeometry.CreatePrimitive(PrimitiveType.Sphere, transform, color: new Color(1, 1, 1), 0.1f);
        endMarker = ImpGeometry.CreatePrimitive(PrimitiveType.Sphere, transform, color: new Color(1, 1, 1), 0.1f);

        Imperium.Freecam.IsFreecamEnabled.onUpdate += OnFreecamToggle;

        OnFreecamToggle(Imperium.Freecam.IsFreecamEnabled.Value);
        OnExitAction();
    }

    internal void Activate()
    {
        isActive = true;
        indicator.SetActive(true);

        startPosition = null;
        endPosition = null;

        startMarker.SetActive(false);
        endMarker.SetActive(false);
        panelRect.gameObject.SetActive(false);

        Imperium.IngamePlayerSettings.playerInput.actions["ActivateItem"].performed += OnLeftClick;
        Imperium.IngamePlayerSettings.playerInput.actions["OpenMenu"].Disable();
        Imperium.IngamePlayerSettings.playerInput.actions["PingScan"].Disable();
        Imperium.InputBindings.StaticMap["RightClick"].performed += OnExitAction;
        Imperium.InputBindings.StaticMap["Escape"].performed += OnExitAction;

        Imperium.InputBindings.BaseMap.Teleport.Disable();
    }

    internal void Deactivate()
    {
        isActive = false;
        indicator.SetActive(false);

        Imperium.IngamePlayerSettings.playerInput.actions["ActivateItem"].performed -= OnLeftClick;
        Imperium.IngamePlayerSettings.playerInput.actions["OpenMenu"].Enable();
        Imperium.IngamePlayerSettings.playerInput.actions["PingScan"].Enable();
        Imperium.InputBindings.StaticMap["RightClick"].performed -= OnExitAction;
        Imperium.InputBindings.StaticMap["Escape"].performed -= OnExitAction;

        Imperium.InputBindings.BaseMap.Teleport.Enable();
    }

    private void OnExitAction(InputAction.CallbackContext context = default)
    {
        // Preemptive exit, so disable currently active objects
        startMarker.SetActive(false);
        endMarker.SetActive(false);
        panelRect.gameObject.SetActive(false);
        tapeLine.gameObject.SetActive(false);

        Deactivate();
    }

    private void OnLeftClick(InputAction.CallbackContext context = default)
    {
        if (!isActive) return;

        if (!startPosition.HasValue)
        {
            startPosition = currentLookPosition;
            startMarker.transform.position = startPosition.Value;
            startMarker.SetActive(true);
        }
        else if (!endPosition.HasValue)
        {
            endPosition = currentLookPosition;
            endMarker.transform.position = endPosition.Value;
            endMarker.SetActive(true);
            Deactivate();
        }
    }

    private void OnTapeOpen(InputAction.CallbackContext callbackContext)
    {
        if (isActive)
        {
            OnExitAction();
        }
        else if (
            !Imperium.Player.quickMenuManager.isMenuOpen &&
            !Imperium.Player.inTerminalMenu &&
            !Imperium.Player.isTypingChat
        )
        {
            Activate();
        }
    }

    private void OnFreecamToggle(bool isOn)
    {
        tapeLayerMask = isOn ? Imperium.Settings.Freecam.FreecamLayerMask.Value : ImpConstants.TapeIndicatorMask;
        originCamera = isOn ? Imperium.Freecam.FreecamCamera : Imperium.Player.gameplayCamera;
    }

    private void Update()
    {
        // Indicator rotation animation values
        if (rotationZ > 360)
        {
            rotationZ = 0;
        }
        else
        {
            rotationZ += 100 * Time.deltaTime;
        }

        // Raycast to current player look position
        var ray = new Ray(
            originCamera.transform.position + originCamera.transform.forward * 0.4f,
            originCamera.transform.forward
        );
        Physics.Raycast(ray, out var hitInfo, 1000, tapeLayerMask);

        currentLookPosition = hitInfo.point;
        currentLookNormal = hitInfo.normal;

        var scale = Mathf.Clamp(hitInfo.distance / 2, 0.1f, 1);

        if (startPosition.HasValue)
        {
            var directionVector = (endPosition ?? hitInfo.point) - startPosition.Value;

            // Axis snapping when first position is selected and the Alt key is pressed
            if (Imperium.InputBindings.StaticMap["Alt"].IsPressed())
            {
                Vector3 snappingAxis = default;
                var minAngle = 360f;
                foreach (var axis in Axes)
                {
                    var axisAngle = Vector3.Angle(directionVector, axis);
                    if (axisAngle < minAngle)
                    {
                        snappingAxis = axis;
                        minAngle = axisAngle;
                    }
                }

                // The furthest possible point on the axis is defined by the position of the first object in the axis' ray
                var endPointRay = new Ray(startPosition.Value, snappingAxis);
                Physics.Raycast(endPointRay, out var endPointHit, 1000, tapeLayerMask);

                var player = Imperium.Player.gameplayCamera.transform;

                // Angle between the player forward and the snapping axis
                var angleToAxis = Vector3.Angle(player.forward, snappingAxis);

                // Calculate the required length of the player forward with sine
                var oppositeSide = Vector3.Distance(originCamera.transform.position, startPosition.Value);
                var lookDistance = oppositeSide / Mathf.Sin(Mathf.Deg2Rad * angleToAxis);

                // Project the forward vector onto the snapping axis
                var forwardToStart = startPosition.Value - player.position;
                var extendedForward = player.forward * lookDistance;
                var lookDifference = extendedForward - forwardToStart;
                var projection = Vector3.ClampMagnitude(
                    Vector3.Project(lookDifference, snappingAxis),
                    endPointHit.distance
                );

                directionVector = snappingAxis;
                currentLookPosition = startPosition.Value + projection;
                currentLookNormal = -player.forward;

                // If the projection vector was clamped to the end point, align the indicator with the end point
                // otherwise, align the indicator with the opposite of the player's forward to make it face the player
                currentLookNormal = Vector3.Distance(endPointHit.point, currentLookPosition) < 0.5f
                    ? endPointHit.normal
                    : -player.forward;
            }

            var panelWorldPosition = startPosition.Value + directionVector / 2;
            var screenPosition = originCamera.WorldToScreenPoint(panelWorldPosition);

            var activeCameraTexture = originCamera.targetTexture;

            var scaleFactorX = activeCameraTexture.width / canvasRect.sizeDelta.x;
            var scaleFactorY = activeCameraTexture.height / canvasRect.sizeDelta.y;

            var positionX = screenPosition.x / scaleFactorX;
            var positionY = screenPosition.y / scaleFactorY;
            panelRect.anchoredPosition = new Vector2(positionX, positionY);

            var distanceToTape = (originCamera.transform.position - panelWorldPosition).magnitude;
            panelRect.localScale = Vector3.one * Mathf.Clamp(4 / distanceToTape, 0.5f, 2);

            if (isActive)
            {
                tapeLine.gameObject.SetActive(true);
                panelRect.gameObject.SetActive(screenPosition.z > 0);
                distanceText.text = $"{directionVector.magnitude:0.00}u ({directionVector.magnitude * 0.62:0.0}m)";

                ImpGeometry.SetLinePositions(
                    tapeLine,
                    startPosition.Value,
                    endPosition ?? currentLookPosition
                );
            }
        }
        else
        {
            tapeLine.gameObject.SetActive(false);
        }

        // Position the indicator
        indicator.transform.position = currentLookPosition;
        indicator.transform.LookAt(currentLookPosition + currentLookNormal);
        indicator.transform.RotateAround(currentLookPosition, currentLookNormal, rotationZ);
        indicator.transform.localScale = Vector3.one * (scale * 15);
    }
}
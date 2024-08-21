using DunGen;
using Imperium.Util;
using UnityEngine;

namespace Imperium.Core.LevelEditor;

internal class DoorMarker : MonoBehaviour
{
    private Doorway doorway;
    private Material markerMaterial;

    private GameObject marker;

    private readonly Color defaultColor = new(1, 1, 1);
    private readonly Color highlightedColor = new(0.12f, 1f, 0.45f);

    private float defaultScale;
    private float highlightedScale;

    private float currentScale;
    private float targetScale;

    internal Vector3 DoorwayOrigin { get; private set; }
    internal Quaternion DoorwayRotation { get; private set; }

    private GameObject blockingPlane;
    private BoxCollider blockingPlaneCollider;

    internal bool IsConnected;
    internal bool HasDoorway;

    internal DoorwaySocket Socket { get; private set; }

    internal void Init(Doorway markerDoor)
    {
        doorway = markerDoor;
        Socket = markerDoor.Socket;

        DoorwayOrigin = doorway.transform.position;
        DoorwayRotation = doorway.transform.rotation;

        transform.SetParent(markerDoor.transform);
        transform.localPosition = new Vector3(0, markerDoor.socket.size.y / 2f, 0);

        defaultScale = Mathf.Clamp(doorway.socket.size.x * 0.2f, 1, 3);
        highlightedScale = defaultScale * 2.5f;

        marker = transform.Find("marker").gameObject;
        markerMaterial = marker.GetComponent<MeshRenderer>().material;
        markerMaterial.color = defaultColor;
        marker.transform.localScale = Vector3.one * (defaultScale * 30f);

        blockingPlane = ImpGeometry.CreatePrimitive(
            PrimitiveType.Quad,
            transform,
            material: ImpAssets.WireframeRed,
            removeCollider: true,
            removeRenderer: true
        );
        blockingPlaneCollider = blockingPlane.AddComponent<BoxCollider>();
        // Visualization.VisualizeBoxCollider(collider, ImpAssets.WireframeRed);
        blockingPlane.transform.localScale = new Vector3(doorway.socket.size.x, doorway.socket.size.y, 1);
        blockingPlane.transform.localRotation = Quaternion.identity;

        for (var i = 0; i < doorway.transform.childCount; i++)
        {
            if (doorway.transform.GetChild(i).name.Contains("Blocker"))
            {
                doorway.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    internal void DisableCollider()
    {
        blockingPlaneCollider.isTrigger = true;
    }

    internal void Disable()
    {
        marker.SetActive(false);
        blockingPlane.SetActive(false);
    }

    internal void Highlight()
    {
        markerMaterial.color = highlightedColor;
        targetScale = highlightedScale;
    }

    internal void Unhighlight()
    {
        markerMaterial.color = defaultColor;
        targetScale = defaultScale;
    }

    private void Update()
    {
        if (!Mathf.Approximately(currentScale, targetScale))
        {
            currentScale = Mathf.Lerp(currentScale, targetScale, 8 * Time.deltaTime);
            marker.transform.localScale = Vector3.one * (currentScale * 30f);
        }
    }
}
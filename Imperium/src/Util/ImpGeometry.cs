#region

using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Util;

public static class ImpGeometry
{
    /// <summary>
    ///     Instantiates a line in world space.
    /// </summary>
    /// <param name="parent">The object that the line will be parented to</param>
    /// <param name="thickness">The thickness of the line. Default: 0.05</param>
    /// <param name="useWorldSpace">Whether the line positioning should be absolute</param>
    /// <param name="lineName">The name of the line object</param>
    /// <param name="startColor">The starting color of the line. Default: White</param>
    /// <param name="endColor">The end color of the line. Default: White</param>
    /// <param name="positions">A list of positions that define the line</param>
    /// <returns></returns>
    public static LineRenderer CreateLine(
        Transform parent = null,
        float thickness = 0.05f,
        bool useWorldSpace = false,
        string lineName = null,
        Color? startColor = null,
        Color? endColor = null,
        params Vector3[] positions
    )
    {
        var lineObject = new GameObject();

        var lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = thickness;
        lineRenderer.endWidth = thickness;
        lineRenderer.useWorldSpace = useWorldSpace;
        lineRenderer.positionCount = 2;

        if (lineName != null) lineObject.name = lineName;
        if (parent) lineObject.transform.SetParent(parent);
        if (startColor.HasValue) lineRenderer.startColor = startColor.Value;
        if (endColor.HasValue) lineRenderer.endColor = endColor.Value;
        if (positions.Length > 0) SetLinePositions(lineRenderer, positions);

        return lineRenderer;
    }

    /// <summary>
    ///     Sets the color of an existing line.
    /// </summary>
    public static void SetLineColor(LineRenderer lineRenderer, Color? startColor = null, Color? endColor = null)
    {
        if (startColor.HasValue) lineRenderer.startColor = startColor.Value;
        if (endColor.HasValue) lineRenderer.endColor = endColor.Value;
    }

    /// <summary>
    ///     Sets the positions of an existing line.
    /// </summary>
    public static void SetLinePositions(LineRenderer lineRenderer, params Vector3[] positions)
    {
        lineRenderer.positionCount = positions.Length;
        for (var i = 0; i < positions.Length; i++)
        {
            lineRenderer.SetPosition(i, positions[i]);
        }
    }

    /// <summary>
    ///     Creates a primitive shape object in world space with a simple color as material.
    /// </summary>
    /// <param name="type">The type of shape to create</param>
    /// <param name="parent">The object the shape will be parented to</param>
    /// <param name="color">The color of the material of the shape</param>
    /// <param name="size">The size of the shape object</param>
    /// <param name="layer">The layer of the shape object</param>
    /// <param name="name">The name of the shape object</param>
    /// <param name="removeCollider">Whether the colliders of the shape objects should be removed. Default: true</param>
    /// <param name="removeRenderer">Whether the renderer of the shape objects should be removed. Default: false</param>
    public static GameObject CreatePrimitive(
        PrimitiveType type,
        [CanBeNull] Transform parent,
        Color color,
        float size = 1,
        int layer = 0,
        string name = null,
        bool removeCollider = true,
        bool removeRenderer = false
    )
    {
        var sphere = CreatePrimitive(type, parent, size, name, layer, removeCollider, removeRenderer);

        var renderer = sphere.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;

        var material = renderer.material;
        material.shader = Shader.Find("HDRP/Unlit");
        material.color = color;

        return sphere;
    }

    /// <summary>
    ///     Creates a primitive shape object in world space with a given material.
    /// </summary>
    /// <param name="type">The type of shape to create</param>
    /// <param name="parent">The object the shape will be parented to</param>
    /// <param name="material">The material of the shape object.</param>
    /// <param name="size">The size of the shape object</param>
    /// <param name="layer">The layer of the shape object</param>
    /// <param name="name">The name of the shape object</param>
    /// <param name="removeCollider">Whether the colliders of the shape objects should be removed. Default: true</param>
    /// <param name="removeRenderer">Whether the renderer of the shape objects should be removed. Default: false</param>
    public static GameObject CreatePrimitive(
        PrimitiveType type,
        [CanBeNull] Transform parent,
        [CanBeNull] Material material,
        float size = 1,
        int layer = 0,
        string name = null,
        bool removeCollider = true,
        bool removeRenderer = false
    )
    {
        var sphere = CreatePrimitive(type, parent, size, name, layer, removeCollider, removeRenderer);
        if (name != null) sphere.name = name;

        var renderer = sphere.GetComponent<MeshRenderer>();
        renderer.shadowCastingMode = ShadowCastingMode.Off;

        if (material)
        {
            renderer.material = material;
        }
        else
        {
            renderer.material.shader = Shader.Find("HDRP/Unlit");
        }

        return sphere;
    }

    internal static GameObject CreatePrimitive(
        PrimitiveType type,
        [CanBeNull] Transform parent,
        float size = 1,
        string name = null,
        int layer = 0,
        bool removeCollider = true,
        bool removeRenderer = false
    )
    {
        var primitive = GameObject.CreatePrimitive(type);
        if (name != null) primitive.name = name;
        primitive.layer = layer;

        if (removeCollider)
        {
            switch (type)
            {
                case PrimitiveType.Sphere:
                    Object.Destroy(primitive.GetComponent<SphereCollider>());
                    break;
                case PrimitiveType.Cylinder:
                case PrimitiveType.Cube:
                    Object.Destroy(primitive.GetComponent<BoxCollider>());
                    break;
                case PrimitiveType.Quad:
                case PrimitiveType.Plane:
                    Object.Destroy(primitive.GetComponent<MeshCollider>());
                    break;
                case PrimitiveType.Capsule:
                    Object.Destroy(primitive.GetComponent<CapsuleCollider>());
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        primitive.transform.localScale = Vector3.one * size;
        if (parent)
        {
            primitive.transform.position = parent.position;
            primitive.transform.SetParent(parent);
        }

        if (removeRenderer)
        {
            Object.Destroy(primitive.GetComponent<MeshRenderer>());
            Object.Destroy(primitive.GetComponent<MeshFilter>());
        }

        return primitive;
    }

    /// <summary>
    ///     Normalizes the bounds of a rect transform into a rect that has its coordinates between 0 and 1.
    /// </summary>
    public static Rect NormalizeRectTransform(RectTransform input, float canvasScale)
    {
        return Rect.MinMaxRect(
            input.offsetMin.x * canvasScale / Screen.width,
            input.offsetMin.y * canvasScale / Screen.height,
            (Screen.width + input.offsetMax.x * canvasScale) / Screen.width,
            (Screen.height + input.offsetMax.y * canvasScale) / Screen.height
        );
    }
}
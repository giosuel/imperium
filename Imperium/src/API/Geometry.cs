using Imperium.Util;
using UnityEngine;

namespace Imperium.API;

public class Geometry
{
    public static LineRenderer CreateLine(
        Transform parent,
        float thickness = 0.05f,
        bool useWorldSpace = false,
        Color? color = null,
        params Vector3[] positions
    )
    {
        return ImpUtils.Geometry.CreateLine(parent, thickness, useWorldSpace, color, positions);
    }
}
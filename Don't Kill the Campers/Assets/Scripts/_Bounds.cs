using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class _Bounds
{
    public static Vector3Int ConstrainPointToInnerBounds(BoundsInt bounds, Vector3Int point)
    {
        if (point.x > bounds.xMax - 2) point.x = bounds.xMax - 2;
        else if (point.x < bounds.xMin + 1) point.x = bounds.xMin + 1;
        if (point.y > bounds.yMax - 2) point.y = bounds.yMax - 2;
        else if (point.y < bounds.yMin + 1) point.y = bounds.yMin + 1;
        return point;
    }

    public static Vector3Int ConstrainPointToBounds(BoundsInt bounds, Vector3Int point)
    {
        if (bounds.Contains(point)) return point;
        if (point.x > bounds.xMax - 1) point.x = bounds.xMax - 1;
        else if (point.x < bounds.xMin) point.x = bounds.xMin;
        if (point.y > bounds.yMax - 1) point.y = bounds.yMax - 1;
        else if (point.y < bounds.yMin) point.y = bounds.yMin;
        return point;
    }

    //public static Vector3Int ConstrainPointToOutBounds(BoundsInt, Bounds, Vector3Int point)

    public static bool BoundsIntersect(BoundsInt bound1, BoundsInt bound2)
    {
        bool xInt = (bound1.xMin <= bound2.xMin && bound1.xMax >= bound2.xMin) ||
            (bound1.xMin <= bound2.xMax && bound1.xMax >= bound2.xMax);
        bool yInt = (bound1.yMin <= bound2.yMin && bound1.yMax >= bound2.yMin) ||
            (bound1.yMin <= bound2.yMax && bound1.yMax >= bound2.yMax);
        return xInt && yInt;
    }

    public static bool BoundsEncapsulatesBounds(BoundsInt container, BoundsInt containee)
    {
        if (!container.Contains(containee.max)) return false;
        if (!container.Contains(containee.min)) return false;
        return true;
    }
}

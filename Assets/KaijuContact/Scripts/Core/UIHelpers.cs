using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UIHelpers
{
    //library code
    static Vector3[] screenSpaceCorners;
    public static Rect RendererBoundsInScreenSpace(Renderer renderer, Camera camera)
    {
        // This is the space occupied by the object's visuals
        // in WORLD space.
        Bounds bigBounds = renderer.bounds;

        if (screenSpaceCorners == null)
            screenSpaceCorners = new Vector3[8];

        // For each of the 8 corners of our renderer's world space bounding box,
        // convert those corners into screen space.
        screenSpaceCorners[0] = camera.WorldToScreenPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
        screenSpaceCorners[1] = camera.WorldToScreenPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
        screenSpaceCorners[2] = camera.WorldToScreenPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
        screenSpaceCorners[3] = camera.WorldToScreenPoint(new Vector3(bigBounds.center.x + bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
        screenSpaceCorners[4] = camera.WorldToScreenPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
        screenSpaceCorners[5] = camera.WorldToScreenPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y + bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));
        screenSpaceCorners[6] = camera.WorldToScreenPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z + bigBounds.extents.z));
        screenSpaceCorners[7] = camera.WorldToScreenPoint(new Vector3(bigBounds.center.x - bigBounds.extents.x, bigBounds.center.y - bigBounds.extents.y, bigBounds.center.z - bigBounds.extents.z));

        // Now find the min/max X & Y of these screen space corners.
        float min_x = screenSpaceCorners[0].x;
        float min_y = screenSpaceCorners[0].y;
        float max_x = screenSpaceCorners[0].x;
        float max_y = screenSpaceCorners[0].y;

        for (int i = 1; i < 8; i++)
        {
            if (screenSpaceCorners[i].x < min_x)
            {
                min_x = screenSpaceCorners[i].x;
            }
            if (screenSpaceCorners[i].y < min_y)
            {
                min_y = screenSpaceCorners[i].y;
            }
            if (screenSpaceCorners[i].x > max_x)
            {
                max_x = screenSpaceCorners[i].x;
            }
            if (screenSpaceCorners[i].y > max_y)
            {
                max_y = screenSpaceCorners[i].y;
            }
        }

        return Rect.MinMaxRect(min_x, min_y, max_x, max_y);
    }
}

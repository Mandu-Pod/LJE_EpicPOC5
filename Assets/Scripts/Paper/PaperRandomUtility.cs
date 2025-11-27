using System.Collections.Generic;
using UnityEngine;

public static class PaperRandomUtility
{
    /// <summary>
    /// 랜덤 위치 1개 반환
    /// </summary>
    public static Vector2 GetRandomPointOnPolygons(List<List<Vector2>> polygons, int maxAttempts = 500)
    {
        if (polygons == null || polygons.Count == 0)
            throw new System.Exception("Polygon list is empty.");

        GetBoundingBox(polygons, out float minX, out float maxX, out float minY, out float maxY);

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 p = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            if (IsPointInsidePolygons(p, polygons))
                return p;
        }

        return polygons[0][0];
    }

    /// <summary>
    /// 여러 폴리곤 중 하나라도 포함하면 true
    /// </summary>
    public static bool IsPointInsidePolygons(Vector2 point, List<List<Vector2>> polygons)
    {
        foreach (var poly in polygons)
        {
            if (IsPointInPolygon(point, poly))
            {
                return true;

            }
        }
        return false;
    }

    /// <summary>
    /// Ray Casting PIP 알고리즘
    /// </summary>
    public static bool IsPointInPolygon(Vector2 p, List<Vector2> poly)
    {
        bool inside = false;

        for (int i = 0, j = poly.Count - 1; i < poly.Count; j = i++)
        {
            Vector2 a = poly[i];
            Vector2 b = poly[j];

            bool intersect = ((a.y > p.y) != (b.y > p.y)) &&
                             (p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y + Mathf.Epsilon) + a.x);

            if (intersect)
                inside = !inside;
        }

        return inside;
    }

    private static void GetBoundingBox(List<List<Vector2>> polygons,
                                       out float minX, out float maxX,
                                       out float minY, out float maxY)
    {
        Vector2 first = polygons[0][0];

        minX = maxX = first.x;
        minY = maxY = first.y;

        foreach (var poly in polygons)
        {
            foreach (var p in poly)
            {
                if (p.x < minX) minX = p.x;
                if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y;
                if (p.y > maxY) maxY = p.y;
            }
        }
    }
}

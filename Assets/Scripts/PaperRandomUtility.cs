using System.Collections.Generic;
using UnityEngine;

public static class PaperRandomUtility
{
    // 메인 함수: 랜덤 위치 1개 반환
    public static Vector2 GetRandomPointOnPolygons(List<List<Vector2>> polygons, int maxAttempts = 500)
    {
        if (polygons == null || polygons.Count == 0)
            throw new System.Exception("Polygon list is empty.");

        // 전체 영역의 bounding box 구하기
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

        // 실패시 폴리곤의 첫 꼭짓점 반환 (fallback)
        return polygons[0][0];
    }

    // 여러 폴리곤 중 하나라도 포함하면 true
    public static bool IsPointInsidePolygons(Vector2 point, List<List<Vector2>> polygons)
    {
        foreach (var poly in polygons)
        {
            if (IsPointInPolygon(point, poly))
                return true;
        }
        return false;
    }

    // Ray Casting PIP 알고리즘
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

    // 모든 레이어 기준 bounding box 구하기
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

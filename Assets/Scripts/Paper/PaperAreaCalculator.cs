using System.Collections.Generic;
using UnityEngine;

public static class PaperAreaCalculator
{
    /// <summary>
    /// 폴리곤의 면적 계산 (Shoelace formula)
    /// </summary>
    public static float CalculatePolygonArea(List<Vector2> polygon)
    {
        if (polygon == null || polygon.Count < 3)
            return 0f;
        
        float area = 0f;
        int n = polygon.Count;
        
        for (int i = 0; i < n; i++)
        {
            Vector2 current = polygon[i];
            Vector2 next = polygon[(i + 1) % n];
            area += current.x * next.y;
            area -= next.x * current.y;
        }
        
        return Mathf.Abs(area) / 2f;
    }
    
    /// <summary>
    /// 여러 폴리곤 레이어의 총 면적 계산
    /// </summary>
    public static float CalculateTotalArea(List<List<Vector2>> polygonLayers)
    {
        float totalArea = 0f;
        
        foreach (var polygon in polygonLayers)
        {
            totalArea += CalculatePolygonArea(polygon);
        }
        
        return totalArea;
    }
    
    /// <summary>
    /// 초기 면적 대비 현재 면적 비율 계산
    /// </summary>
    public static float CalculateAreaRatio(float currentArea, float initialArea)
    {
        if (initialArea <= 0f)
            return 0f;
        
        return currentArea / initialArea;
    }
}

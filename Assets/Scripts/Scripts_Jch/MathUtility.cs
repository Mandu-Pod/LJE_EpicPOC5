using UnityEngine;

/// <summary>FoldingMesh 기하 연산 유틸리티</summary>
public static class FoldingMathUtility
{
    public const float EPSILON = 0.0001f;

    #region Public Methods - Half-Plane Test
    /// <summary>점이 선의 어느 쪽에 있는지 판별</summary>
    /// <returns>양수: 왼쪽, 음수: 오른쪽, 0: 선 위</returns>
    public static float HalfPlaneTest(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 lineDir = lineEnd - lineStart;
        Vector2 toPoint = point - lineStart;

        // 외적의 Z 성분 (2D에서는 스칼라)
        float cross = lineDir.x * toPoint.y - lineDir.y * toPoint.x;

        if (Mathf.Abs(cross) < EPSILON)
            return 0f;

        return cross;
    }
    #endregion

    #region Public Methods - Line Intersection
    /// <summary>두 선분의 교차점 계산</summary>
    /// <param name="intersection">교차점 (실패 시 Vector2.zero)</param>
    /// <returns>교차 여부</returns>
    public static bool LineIntersection(Vector2 line1Start, Vector2 line1End,
                                       Vector2 line2Start, Vector2 line2End,
                                       out Vector2 intersection)
    {
        intersection = Vector2.zero;

        Vector2 dir1 = line1End - line1Start;
        Vector2 dir2 = line2End - line2Start;

        float cross = dir1.x * dir2.y - dir1.y * dir2.x;

        // 평행
        if (Mathf.Abs(cross) < EPSILON)
            return false;

        Vector2 startDiff = line2Start - line1Start;
        float t1 = (startDiff.x * dir2.y - startDiff.y * dir2.x) / cross;
        float t2 = (startDiff.x * dir1.y - startDiff.y * dir1.x) / cross;

        // 선분 범위 체크
        if (t1 < -EPSILON || t1 > 1f + EPSILON || t2 < -EPSILON || t2 > 1f + EPSILON)
            return false;

        intersection = line1Start + dir1 * t1;
        return true;
    }
    #endregion

    #region Public Methods - Reflection
    /// <summary>선 기준 점 반사</summary>
    public static Vector2 ReflectPoint(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        Vector2 lineDir = (lineEnd - lineStart).normalized;
        Vector2 normal = new Vector2(-lineDir.y, lineDir.x);

        Vector2 toPoint = point - lineStart;
        float distance = Vector2.Dot(toPoint, normal);

        return point - 2f * distance * normal;
    }
    #endregion
}
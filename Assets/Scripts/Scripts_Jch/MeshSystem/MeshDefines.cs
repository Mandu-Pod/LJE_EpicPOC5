using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>동적 메시 데이터 (정점 증감 대응)</summary>
[Serializable]
public class DynamicMeshData
{
    public List<Vector3> Positions = new();
    public List<Vector2> UVs = new();
    public List<int> Triangles = new();
    public List<int> LayerDepths = new();
    public List<bool> FrontFaces = new();

    public DynamicMeshData DeepCopy()
    {
        var copy = new DynamicMeshData();
        copy.Positions = new List<Vector3>(Positions);
        copy.UVs = new List<Vector2>(UVs);
        copy.Triangles = new List<int>(Triangles);
        copy.LayerDepths = new List<int>(LayerDepths);
        copy.FrontFaces = new List<bool>(FrontFaces);
        return copy;
    }

    public void Clear()
    {
        Positions.Clear();
        UVs.Clear();
        Triangles.Clear();
        LayerDepths.Clear();
        FrontFaces.Clear();
    }
}

/// <summary>분할된 셀 추적</summary>
[Serializable]
public struct SplitCellData
{
    public int OriginalCellIndex;
    public List<int> TriangleStartIndices;
    public List<bool> TriangleFoldedFlags;
}

/// <summary>접기 연산 기록</summary>
[Serializable]
public struct FoldOperation
{
    public Vector2 LineStart;
    public Vector2 LineEnd;
    public Vector2 FoldDirection;
    public int[] AffectedCellIndices;
}

/// <summary>접기 스냅샷 (LIFO 복원용)</summary>
[Serializable]
public struct FoldSnapshot
{
    public FoldOperation Operation;
    public DynamicMeshData MeshStateBeforeFold;
    public SplitCellData[] CellStatesBeforeFold;
}
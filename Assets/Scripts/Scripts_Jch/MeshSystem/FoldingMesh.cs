using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>접기 기하 연산 핵심</summary>
public class FoldingMesh : MonoBehaviour
{
    #region Serialized Fields
    [TabGroup("Setup"), SuffixLabel("cells"), MinValue(3)]
    [SerializeField] private int _gridSize = 5;

    [TabGroup("Setup"), SuffixLabel("units")]
    [SerializeField] private float _cellSize = 1f;

    [TabGroup("Setup"), MinValue(1)]
    [SerializeField] private int _maxFoldDepth = 3;

    [TabGroup("Debug"), ReadOnly]
    [SerializeField] private int _currentFoldCount = 0;

    [TabGroup("Debug")]
    [SerializeField] private bool _isDebugLogging = true;
    #endregion

    #region Properties
    public DynamicMeshData CurrentMeshData => _meshData;
    public int GridSize => _gridSize;
    public float CellSize => _cellSize;
    public int CurrentFoldCount => _currentFoldCount;
    #endregion

    #region Private Fields
    private DynamicMeshData _meshData;
    private SplitCellData[] _cellStates;
    private Stack<FoldSnapshot> _foldHistory;
    #endregion

    #region Events
    public event Action<int[]> OnFoldStateChanged;
    #endregion

    #region Unity Lifecycle
    //private void Awake() => Initialize();
    private void OnDestroy() => Cleanup();
    #endregion

    #region Initialization and Cleanup
    /// <summary>의존성 불필요한 내부 초기화</summary>
    public void Initialize()
    {
        _meshData = new DynamicMeshData();
        _cellStates = new SplitCellData[_gridSize * _gridSize];
        _foldHistory = new Stack<FoldSnapshot>();
        _currentFoldCount = 0;

        // 초기 셀 상태 초기화
        for (int i = 0; i < _cellStates.Length; i++)
        {
            _cellStates[i] = new SplitCellData
            {
                OriginalCellIndex = i,
                TriangleStartIndices = new List<int>(),
                TriangleFoldedFlags = new List<bool>()
            };
        }

        GenerateInitialMesh();
        Log($"Initialized: {_gridSize}×{_gridSize} grid, {_meshData.Positions.Count} vertices");
    }

    /// <summary>소멸 프로세스</summary>
    public void Cleanup()
    {
        _meshData?.Clear();
        _foldHistory?.Clear();
        _cellStates = null;
        _currentFoldCount = 0;

        Log("Cleaned up");
    }
    #endregion

    #region Public Methods - Query
    /// <summary>현재 메시 데이터 반환 (렌더러용)</summary>
    public DynamicMeshData GetCurrentMeshData()
    {
        return _meshData;
    }

    /// <summary>셀의 접힘 깊이 반환</summary>
    public int GetFoldDepth(Vector2Int cellCoord)
    {
        int cellIndex = cellCoord.y * _gridSize + cellCoord.x;

        if (cellIndex < 0 || cellIndex >= _cellStates.Length)
        {
            LogError($"Invalid cell coordinate: {cellCoord}");
            return 0;
        }

        int maxDepth = 0;
        for (int i = 0; i < _cellStates[cellIndex].TriangleFoldedFlags.Count; i++)
        {
            if (_cellStates[cellIndex].TriangleFoldedFlags[i])
            {
                int triStartIndex = _cellStates[cellIndex].TriangleStartIndices[i];
                int vertIndex = _meshData.Triangles[triStartIndex];
                maxDepth = Mathf.Max(maxDepth, _meshData.LayerDepths[vertIndex]);
            }
        }

        return maxDepth;
    }
    #endregion

    #region Public Methods - Folding
    /// <summary>접기 실행</summary>
    /// <param name="lineStart">접기선 시작 (그리드 좌표)</param>
    /// <param name="lineEnd">접기선 끝 (그리드 좌표)</param>
    /// <param name="foldDirection">접히는 방향 (정규화 벡터)</param>
    /// <returns>성공 여부</returns>
    public bool TryFold(Vector2 lineStart, Vector2 lineEnd, Vector2 foldDirection)
    {
        if (_currentFoldCount >= _maxFoldDepth)
        {
            LogWarning($"Max fold depth reached: {_maxFoldDepth}");
            return false;
        }

        // 스냅샷 생성
        FoldOperation operation = new FoldOperation
        {
            LineStart = lineStart,
            LineEnd = lineEnd,
            FoldDirection = foldDirection.normalized
        };

        FoldSnapshot snapshot = CreateSnapshot(operation);

        // 영향받는 셀 찾기
        List<int> affectedCells = new List<int>();
        for (int i = 0; i < _cellStates.Length; i++)
        {
            if (ClipCellByLine(i, lineStart, lineEnd))
            {
                affectedCells.Add(i);
            }
        }

        operation.AffectedCellIndices = affectedCells.ToArray();
        snapshot.Operation = operation;

        _foldHistory.Push(snapshot);
        _currentFoldCount++;

        OnFoldStateChanged?.Invoke(operation.AffectedCellIndices);
        Log($"Folded: {affectedCells.Count} cells affected, depth {_currentFoldCount}");

        return true;
    }

    /// <summary>마지막 접기 해제 (LIFO)</summary>
    public bool TryUnfoldLast()
    {
        if (_foldHistory.Count == 0)
        {
            LogWarning("No fold to undo");
            return false;
        }

        FoldSnapshot snapshot = _foldHistory.Pop();
        RestoreSnapshot(snapshot);
        _currentFoldCount--;

        OnFoldStateChanged?.Invoke(snapshot.Operation.AffectedCellIndices);
        Log($"Unfolded: depth now {_currentFoldCount}");

        return true;
    }
    #endregion

    #region Private Methods - Mesh Generation
    /// <summary>초기 그리드 메시 생성</summary>
    private void GenerateInitialMesh()
    {
        _meshData.Clear();

        for (int y = 0; y < _gridSize; y++)
        {
            for (int x = 0; x < _gridSize; x++)
            {
                int cellIndex = y * _gridSize + x;
                AddCellMesh(cellIndex, new Vector2Int(x, y));
            }
        }
    }

    /// <summary>단일 셀 메시 생성 (2 삼각형)</summary>
    private void AddCellMesh(int cellIndex, Vector2Int coord)
    {
        int vertexStartIndex = _meshData.Positions.Count;

        // 4개 정점 (좌하, 우하, 우상, 좌상)
        Vector3 bottomLeft = new Vector3(coord.x * _cellSize, coord.y * _cellSize, 0f);
        Vector3 bottomRight = new Vector3((coord.x + 1) * _cellSize, coord.y * _cellSize, 0f);
        Vector3 topRight = new Vector3((coord.x + 1) * _cellSize, (coord.y + 1) * _cellSize, 0f);
        Vector3 topLeft = new Vector3(coord.x * _cellSize, (coord.y + 1) * _cellSize, 0f);

        _meshData.Positions.Add(bottomLeft);
        _meshData.Positions.Add(bottomRight);
        _meshData.Positions.Add(topRight);
        _meshData.Positions.Add(topLeft);

        // UV (0~1 정규화)
        float uvX = coord.x / (float)_gridSize;
        float uvY = coord.y / (float)_gridSize;
        float uvStep = 1f / _gridSize;

        _meshData.UVs.Add(new Vector2(uvX, uvY));
        _meshData.UVs.Add(new Vector2(uvX + uvStep, uvY));
        _meshData.UVs.Add(new Vector2(uvX + uvStep, uvY + uvStep));
        _meshData.UVs.Add(new Vector2(uvX, uvY + uvStep));

        // 2개 삼각형 (시계 반대 방향)
        _meshData.Triangles.Add(vertexStartIndex + 0);
        _meshData.Triangles.Add(vertexStartIndex + 2);
        _meshData.Triangles.Add(vertexStartIndex + 1);

        _meshData.Triangles.Add(vertexStartIndex + 0);
        _meshData.Triangles.Add(vertexStartIndex + 3);
        _meshData.Triangles.Add(vertexStartIndex + 2);

        // 레이어 깊이 / 앞면
        for (int i = 0; i < 4; i++)
        {
            _meshData.LayerDepths.Add(0);
            _meshData.FrontFaces.Add(true);
        }

        // 셀 상태 추적
        _cellStates[cellIndex].TriangleStartIndices.Add(_meshData.Triangles.Count - 6);
        _cellStates[cellIndex].TriangleStartIndices.Add(_meshData.Triangles.Count - 3);
        _cellStates[cellIndex].TriangleFoldedFlags.Add(false);
        _cellStates[cellIndex].TriangleFoldedFlags.Add(false);
    }
    #endregion

    #region Private Methods - Clipping
    /// <summary>셀을 접기선으로 분할</summary>
    /// <returns>분할 성공 여부</returns>
    private bool ClipCellByLine(int cellIndex, Vector2 lineStart, Vector2 lineEnd)
    {
        if (cellIndex < 0 || cellIndex >= _cellStates.Length)
            return false;

        SplitCellData cellState = _cellStates[cellIndex];
        bool anyClipped = false;

        // 셀의 모든 삼각형 처리 (역순으로 - 삭제 대응)
        for (int i = cellState.TriangleStartIndices.Count - 1; i >= 0; i--)
        {
            if (cellState.TriangleFoldedFlags[i])
                continue;

            int triStartIndex = cellState.TriangleStartIndices[i];

            // 삼각형 3개 정점 + UV + 기타 속성
            int idx0 = _meshData.Triangles[triStartIndex];
            int idx1 = _meshData.Triangles[triStartIndex + 1];
            int idx2 = _meshData.Triangles[triStartIndex + 2];

            List<Vector3> triangleVerts = new List<Vector3> { _meshData.Positions[idx0], _meshData.Positions[idx1], _meshData.Positions[idx2] };
            List<Vector2> triangleUVs = new List<Vector2> { _meshData.UVs[idx0], _meshData.UVs[idx1], _meshData.UVs[idx2] };
            int layerDepth = _meshData.LayerDepths[idx0];
            bool isFront = _meshData.FrontFaces[idx0];

            // Sutherland-Hodgman 클리핑
            List<Vector3> clippedVerts = ClipPolygon(triangleVerts, lineStart, lineEnd);

            if (clippedVerts.Count >= 3 && clippedVerts.Count < triangleVerts.Count)
            {
                anyClipped = true;

                // UV 보간 (간단 구현 - 첫 정점 기준)
                List<Vector2> clippedUVs = new List<Vector2>();
                for (int v = 0; v < clippedVerts.Count; v++)
                    clippedUVs.Add(triangleUVs[0]);

                // 반사된 폴리곤
                List<Vector3> reflectedVerts = new List<Vector3>(clippedVerts);
                ReflectPolygon(reflectedVerts, lineStart, lineEnd);

                // 새 정점 추가 (고정 폴리곤)
                int fixedStartIndex = _meshData.Positions.Count;
                foreach (var v in clippedVerts)
                {
                    _meshData.Positions.Add(v);
                    _meshData.LayerDepths.Add(layerDepth);
                    _meshData.FrontFaces.Add(isFront);
                }
                foreach (var uv in clippedUVs)
                    _meshData.UVs.Add(uv);

                // 새 정점 추가 (반사 폴리곤)
                int foldedStartIndex = _meshData.Positions.Count;
                foreach (var v in reflectedVerts)
                {
                    _meshData.Positions.Add(v);
                    _meshData.LayerDepths.Add(layerDepth + 1);
                    _meshData.FrontFaces.Add(!isFront); // 뒤집힘
                }
                foreach (var uv in clippedUVs)
                    _meshData.UVs.Add(uv);

                // 삼각화
                List<int> fixedTris = TriangulatePolygon(clippedVerts, fixedStartIndex);
                List<int> foldedTris = TriangulatePolygon(reflectedVerts, foldedStartIndex);

                _meshData.Triangles.AddRange(fixedTris);
                _meshData.Triangles.AddRange(foldedTris);

                // 셀 상태 갱신
                cellState.TriangleStartIndices.Add(_meshData.Triangles.Count - fixedTris.Count - foldedTris.Count);
                cellState.TriangleStartIndices.Add(_meshData.Triangles.Count - foldedTris.Count);
                cellState.TriangleFoldedFlags.Add(false);
                cellState.TriangleFoldedFlags.Add(true);

                // 기존 삼각형 비활성화
                cellState.TriangleFoldedFlags[i] = true;
            }
        }

        _cellStates[cellIndex] = cellState;
        return anyClipped;
    }

    /// <summary>폴리곤 Sutherland-Hodgman 클리핑</summary>
    private List<Vector3> ClipPolygon(List<Vector3> vertices, Vector2 lineStart, Vector2 lineEnd)
    {
        List<Vector3> output = new List<Vector3>(vertices);

        for (int i = 0; i < output.Count; i++)
        {
            int nextIndex = (i + 1) % output.Count;

            Vector2 current = new Vector2(output[i].x, output[i].y);
            Vector2 next = new Vector2(output[nextIndex].x, output[nextIndex].y);

            float currentSide = FoldingMathUtility.HalfPlaneTest(current, lineStart, lineEnd);
            float nextSide = FoldingMathUtility.HalfPlaneTest(next, lineStart, lineEnd);

            if (currentSide * nextSide < 0)
            {
                if (FoldingMathUtility.LineIntersection(lineStart, lineEnd, current, next, out Vector2 intersection))
                {
                    Vector3 intersectionPoint = new Vector3(intersection.x, intersection.y, 0f);

                    if (currentSide > 0)
                    {
                        output.Insert(nextIndex, intersectionPoint);
                    }
                    else
                    {
                        output[i] = intersectionPoint;
                    }
                }
            }
            else if (nextSide < 0)
            {
                output.RemoveAt(nextIndex);
            }
        }

        return output;
    }
    #endregion

    #region Private Methods - Reflection
    /// <summary>폴리곤 정점 반사</summary>
    private void ReflectPolygon(List<Vector3> vertices, Vector2 lineStart, Vector2 lineEnd)
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            Vector2 point2D = new Vector2(vertices[i].x, vertices[i].y);
            Vector2 reflected = FoldingMathUtility.ReflectPoint(point2D, lineStart, lineEnd);
            vertices[i] = new Vector3(reflected.x, reflected.y, vertices[i].z);
        }
    }
    #endregion

    #region Private Methods - Triangulation
    /// <summary>폴리곤 팬 삼각화</summary>
    private List<int> TriangulatePolygon(List<Vector3> vertices, int startIndex)
    {
        List<int> triangles = new List<int>();

        if (vertices.Count < 3)
            return triangles;

        // Fan triangulation: 첫 정점을 중심으로
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(startIndex);
            triangles.Add(startIndex + i + 1);
            triangles.Add(startIndex + i);
        }

        return triangles;
    }
    #endregion

    #region Private Methods - UV
    /// <summary>UV 선형 보간</summary>
    private Vector2 InterpolateUV(Vector2 uvA, Vector2 uvB, float t)
    {
        return Vector2.Lerp(uvA, uvB, t);
    }
    #endregion

    #region Private Methods - Snapshot
    /// <summary>현재 상태 스냅샷 생성</summary>
    private FoldSnapshot CreateSnapshot(FoldOperation operation)
    {
        FoldSnapshot snapshot = new FoldSnapshot
        {
            Operation = operation,
            MeshStateBeforeFold = _meshData.DeepCopy(),
            CellStatesBeforeFold = new SplitCellData[_cellStates.Length]
        };

        // 셀 상태 깊은 복사
        for (int i = 0; i < _cellStates.Length; i++)
        {
            snapshot.CellStatesBeforeFold[i] = new SplitCellData
            {
                OriginalCellIndex = _cellStates[i].OriginalCellIndex,
                TriangleStartIndices = new List<int>(_cellStates[i].TriangleStartIndices),
                TriangleFoldedFlags = new List<bool>(_cellStates[i].TriangleFoldedFlags)
            };
        }

        return snapshot;
    }

    /// <summary>스냅샷 복원</summary>
    private void RestoreSnapshot(FoldSnapshot snapshot)
    {
        _meshData = snapshot.MeshStateBeforeFold.DeepCopy();

        for (int i = 0; i < snapshot.CellStatesBeforeFold.Length; i++)
        {
            _cellStates[i] = new SplitCellData
            {
                OriginalCellIndex = snapshot.CellStatesBeforeFold[i].OriginalCellIndex,
                TriangleStartIndices = new List<int>(snapshot.CellStatesBeforeFold[i].TriangleStartIndices),
                TriangleFoldedFlags = new List<bool>(snapshot.CellStatesBeforeFold[i].TriangleFoldedFlags)
            };
        }
    }
    #endregion

    #region Private Methods - Logging
    private void Log(string msg, bool forcely = false)
    {
        if (_isDebugLogging || forcely)
            Debug.Log($"<color=cyan>[FoldingMesh]</color> {msg}", this);
    }

    private void LogWarning(string msg, bool forcely = false)
    {
        if (_isDebugLogging || forcely)
            Debug.LogWarning($"<color=yellow>[FoldingMesh]</color> {msg}", this);
    }

    private void LogError(string msg)
    {
        Debug.LogError($"<color=red>[FoldingMesh]</color> {msg}", this);
    }
    #endregion
}
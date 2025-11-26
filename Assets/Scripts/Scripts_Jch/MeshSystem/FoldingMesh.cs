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

    /// <summary>메시 원점의 월드 오프셋</summary>
    public Vector3 MeshOriginOffset => transform.position;

    /// <summary>그리드 전체 크기 (월드 단위)</summary>
    public float GridWorldSize => _gridSize * _cellSize;
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
    /// <summary>현재 메시 데이터 반환 (렌더러용) - 활성 삼각형만</summary>
    public DynamicMeshData GetCurrentMeshData()
    {
        return _meshData;
    }

    /// <summary>렌더링용 활성 삼각형 인덱스만 반환</summary>
    public void GetActiveTriangles(out List<int> frontTriangles, out List<int> backTriangles)
    {
        frontTriangles = new List<int>();
        backTriangles = new List<int>();

        foreach (var cell in _cellStates)
        {
            for (int i = 0; i < cell.TriangleStartIndices.Count; i++)
            {
                int triStart = cell.TriangleStartIndices[i];

                // 인덱스 유효성 검사
                if (triStart < 0 || triStart + 2 >= _meshData.Triangles.Count)
                    continue;

                int idx0 = _meshData.Triangles[triStart];
                int idx1 = _meshData.Triangles[triStart + 1];
                int idx2 = _meshData.Triangles[triStart + 2];

                // 정점 인덱스 유효성 검사
                if (idx0 >= _meshData.Positions.Count || idx1 >= _meshData.Positions.Count || idx2 >= _meshData.Positions.Count)
                    continue;

                bool isFolded = cell.TriangleFoldedFlags[i];
                bool isFront = _meshData.FrontFaces[idx0];

                // 비활성화된 원본 삼각형은 제외 (i번째가 분할되어 대체된 경우)
                // 새로 생성된 접힌 면(isFolded=true)은 포함
                if (i < cell.TriangleStartIndices.Count - 2 && isFolded && isFront)
                {
                    // 원본 삼각형이 분할된 경우 (뒤에 새 삼각형이 추가됨)
                    continue;
                }

                if (isFront)
                {
                    frontTriangles.Add(idx0);
                    frontTriangles.Add(idx1);
                    frontTriangles.Add(idx2);
                }
                else
                {
                    backTriangles.Add(idx0);
                    backTriangles.Add(idx1);
                    backTriangles.Add(idx2);
                }
            }
        }
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

    #region Public Methods - Coordinate Conversion
    /// <summary>월드 좌표 → 그리드 로컬 좌표</summary>
    public Vector2 WorldToGridPosition(Vector2 worldPosition)
    {
        Vector2 offset = new Vector2(transform.position.x, transform.position.y);
        return worldPosition - offset;
    }

    /// <summary>그리드 로컬 좌표 → 월드 좌표</summary>
    public Vector2 GridToWorldPosition(Vector2 gridPosition)
    {
        Vector2 offset = new Vector2(transform.position.x, transform.position.y);
        return gridPosition + offset;
    }

    /// <summary>월드 좌표 → 셀 인덱스 (범위 밖이면 -1)</summary>
    public int WorldToCellIndex(Vector2 worldPosition)
    {
        Vector2 gridPos = WorldToGridPosition(worldPosition);
        int cellX = Mathf.FloorToInt(gridPos.x / _cellSize);
        int cellY = Mathf.FloorToInt(gridPos.y / _cellSize);

        if (cellX < 0 || cellX >= _gridSize || cellY < 0 || cellY >= _gridSize)
            return -1;

        return cellY * _gridSize + cellX;
    }

    /// <summary>월드 좌표가 그리드 범위 내인지 확인</summary>
    public bool IsWorldPositionInGrid(Vector2 worldPosition)
    {
        Vector2 gridPos = WorldToGridPosition(worldPosition);
        return gridPos.x >= 0 && gridPos.x <= GridWorldSize &&
               gridPos.y >= 0 && gridPos.y <= GridWorldSize;
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

        // 중심 오프셋 계산
        float centerOffset = (_gridSize * _cellSize) * 0.5f;

        // 4개 정점 (좌하, 우하, 우상, 좌상) - 중심 기준
        Vector3 bottomLeft = new Vector3(coord.x * _cellSize - centerOffset, coord.y * _cellSize - centerOffset, 0f);
        Vector3 bottomRight = new Vector3((coord.x + 1) * _cellSize - centerOffset, coord.y * _cellSize - centerOffset, 0f);
        Vector3 topRight = new Vector3((coord.x + 1) * _cellSize - centerOffset, (coord.y + 1) * _cellSize - centerOffset, 0f);
        Vector3 topLeft = new Vector3(coord.x * _cellSize - centerOffset, (coord.y + 1) * _cellSize - centerOffset, 0f);

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

        for (int i = cellState.TriangleStartIndices.Count - 1; i >= 0; i--)
        {
            if (cellState.TriangleFoldedFlags[i])
                continue;

            int triStartIndex = cellState.TriangleStartIndices[i];

            int idx0 = _meshData.Triangles[triStartIndex];
            int idx1 = _meshData.Triangles[triStartIndex + 1];
            int idx2 = _meshData.Triangles[triStartIndex + 2];

            List<Vector2> triangleVerts = new List<Vector2>
        {
            new Vector2(_meshData.Positions[idx0].x, _meshData.Positions[idx0].y),
            new Vector2(_meshData.Positions[idx1].x, _meshData.Positions[idx1].y),
            new Vector2(_meshData.Positions[idx2].x, _meshData.Positions[idx2].y)
        };

            List<Vector2> triangleUVs = new List<Vector2>
        {
            _meshData.UVs[idx0],
            _meshData.UVs[idx1],
            _meshData.UVs[idx2]
        };

            int layerDepth = _meshData.LayerDepths[idx0];
            bool isFront = _meshData.FrontFaces[idx0];

            // 양쪽 폴리곤 생성
            ClipPolygonBothSides(triangleVerts, triangleUVs, lineStart, lineEnd,
                out List<Vector2> fixedVerts, out List<Vector2> fixedUVs,
                out List<Vector2> foldedVerts, out List<Vector2> foldedUVs);

            // 분할 발생 확인 (양쪽 모두 3개 이상 정점)
            if (fixedVerts.Count < 3 || foldedVerts.Count < 3)
                continue;

            anyClipped = true;

            // 접힌 정점 반사
            for (int v = 0; v < foldedVerts.Count; v++)
            {
                foldedVerts[v] = FoldingMathUtility.ReflectPoint(foldedVerts[v], lineStart, lineEnd);
            }

            // 고정 폴리곤 추가
            int fixedStartIndex = _meshData.Positions.Count;
            foreach (var v in fixedVerts)
            {
                _meshData.Positions.Add(new Vector3(v.x, v.y, 0f));
                _meshData.LayerDepths.Add(layerDepth);
                _meshData.FrontFaces.Add(isFront);
            }
            _meshData.UVs.AddRange(fixedUVs);

            // 접힌 폴리곤 추가
            int foldedStartIndex = _meshData.Positions.Count;
            foreach (var v in foldedVerts)
            {
                _meshData.Positions.Add(new Vector3(v.x, v.y, 0f));
                _meshData.LayerDepths.Add(layerDepth + 1);
                _meshData.FrontFaces.Add(!isFront);
            }
            _meshData.UVs.AddRange(foldedUVs);

            // 삼각화
            List<int> fixedTris = TriangulatePolygon(fixedVerts.Count, fixedStartIndex);
            List<int> foldedTris = TriangulatePolygon(foldedVerts.Count, foldedStartIndex);

            // Winding order 반전 (접힌 면)
            for (int t = 0; t < foldedTris.Count; t += 3)
            {
                (foldedTris[t + 1], foldedTris[t + 2]) = (foldedTris[t + 2], foldedTris[t + 1]);
            }

            int fixedTriStart = _meshData.Triangles.Count;
            _meshData.Triangles.AddRange(fixedTris);
            int foldedTriStart = _meshData.Triangles.Count;
            _meshData.Triangles.AddRange(foldedTris);

            // 셀 상태 갱신
            cellState.TriangleStartIndices.Add(fixedTriStart);
            cellState.TriangleStartIndices.Add(foldedTriStart);
            cellState.TriangleFoldedFlags.Add(false);
            cellState.TriangleFoldedFlags.Add(true);

            // 기존 삼각형 비활성화 (실제 삭제 대신 플래그)
            cellState.TriangleFoldedFlags[i] = true;

            Log($"Cell {cellIndex}: split into {fixedVerts.Count}v fixed + {foldedVerts.Count}v folded");
        }

        _cellStates[cellIndex] = cellState;
        return anyClipped;
    }

    /// <summary>Sutherland-Hodgman 클리핑 - 양쪽 폴리곤 생성</summary>
    private void ClipPolygonBothSides(
        List<Vector2> inputVerts, List<Vector2> inputUVs,
        Vector2 lineStart, Vector2 lineEnd,
        out List<Vector2> positiveVerts, out List<Vector2> positiveUVs,
        out List<Vector2> negativeVerts, out List<Vector2> negativeUVs)
    {
        positiveVerts = new List<Vector2>();
        positiveUVs = new List<Vector2>();
        negativeVerts = new List<Vector2>();
        negativeUVs = new List<Vector2>();

        if (inputVerts.Count < 3)
            return;

        for (int i = 0; i < inputVerts.Count; i++)
        {
            int nextI = (i + 1) % inputVerts.Count;

            Vector2 current = inputVerts[i];
            Vector2 next = inputVerts[nextI];
            Vector2 currentUV = inputUVs[i];
            Vector2 nextUV = inputUVs[nextI];

            float currentSide = FoldingMathUtility.HalfPlaneTest(current, lineStart, lineEnd);
            float nextSide = FoldingMathUtility.HalfPlaneTest(next, lineStart, lineEnd);

            bool currentPositive = currentSide >= 0;
            bool nextPositive = nextSide >= 0;

            // 현재 정점 추가
            if (currentPositive)
            {
                positiveVerts.Add(current);
                positiveUVs.Add(currentUV);
            }
            else
            {
                negativeVerts.Add(current);
                negativeUVs.Add(currentUV);
            }

            // 엣지가 라인을 교차하면 교차점 추가
            if (currentPositive != nextPositive)
            {
                if (FoldingMathUtility.LineIntersection(lineStart, lineEnd, current, next, out Vector2 intersection))
                {
                    // UV 보간
                    float t = Vector2.Distance(current, intersection) / Vector2.Distance(current, next);
                    Vector2 intersectionUV = Vector2.Lerp(currentUV, nextUV, t);

                    positiveVerts.Add(intersection);
                    positiveUVs.Add(intersectionUV);
                    negativeVerts.Add(intersection);
                    negativeUVs.Add(intersectionUV);
                }
            }
        }
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
    private List<int> TriangulatePolygon(int vertexCount, int startIndex)
    {
        List<int> triangles = new List<int>();

        if (vertexCount < 3)
            return triangles;

        for (int i = 1; i < vertexCount - 1; i++)
        {
            triangles.Add(startIndex);
            triangles.Add(startIndex + i);
            triangles.Add(startIndex + i + 1);
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
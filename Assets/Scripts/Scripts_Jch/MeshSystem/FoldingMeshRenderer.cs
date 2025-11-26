using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mesh;

/// <summary>FoldingMesh 시각화</summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FoldingMeshRenderer : MonoBehaviour
{
    #region Serialized Fields
    [TabGroup("Setup"), Required]
    [SerializeField] private Material _frontMaterial;

    [TabGroup("Setup"), Required]
    [SerializeField] private Material _backMaterial;

    [TabGroup("Debug")]
    [SerializeField] private bool _isDebugLogging = true;

    [TabGroup("Debug")]
    [SerializeField] private bool _showVertexGizmos = false;

    [TabGroup("Debug"), ShowIf("_showVertexGizmos"), SuffixLabel("units")]
    [SerializeField] private float _gizmoSize = 0.1f;
    #endregion

    #region Properties
    public Mesh CurrentMesh => _mesh;
    #endregion

    #region Private Fields
    private FoldingMesh _foldingMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
    private DynamicMeshData _meshData => _foldingMesh?.GetCurrentMeshData();
    #endregion

    #region Unity Lifecycle
    //private void Awake() => Initialize();
    private void OnDestroy() => Cleanup();
    #endregion

    #region Initialization and Cleanup
    /// <summary>의존성 불필요한 내부 초기화</summary>
    public void Initialize()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshRenderer = GetComponent<MeshRenderer>();

        if (_frontMaterial == null || _backMaterial == null)
        {
            LogError("Materials not assigned");
            return;
        }

        _mesh = new Mesh();
        _mesh.name = "FoldingMesh";
        _meshFilter.mesh = _mesh;

        // 앞뒷면 렌더링 위해 2개 서브메시 사용
        _meshRenderer.materials = new Material[] { _frontMaterial, _backMaterial };

        Log("Initialized");
    }

    /// <summary>외부 의존성 필요한 초기화</summary>
    public void LateInitialize(FoldingMesh foldingMesh)
    {
        _foldingMesh = foldingMesh;

        if (_foldingMesh != null)
        {
            _foldingMesh.OnFoldStateChanged += OnFoldStateChanged;
            UpdateMesh();
            Log($"Late initialized with FoldingMesh (Grid: {_foldingMesh.GridSize}×{_foldingMesh.GridSize})");
        }
        else
        {
            LogError("FoldingMesh is null");
        }
    }

    /// <summary>소멸 프로세스</summary>
    public void Cleanup()
    {
        if (_foldingMesh != null)
        {
            _foldingMesh.OnFoldStateChanged -= OnFoldStateChanged;
        }

        if (_mesh != null)
        {
            Destroy(_mesh);
            _mesh = null;
        }

        Log("Cleaned up");
    }

    private void OnFoldStateChanged(int[] affectedCells)
    {
        UpdateMesh();
    }
    #endregion

    #region Public Methods - Mesh Update
    /// <summary>메시 갱신</summary>
    public void UpdateMesh()
    {
        if (_foldingMesh == null)
        {
            LogWarning("FoldingMesh not set");
            return;
        }

        DynamicMeshData meshData = _foldingMesh.GetCurrentMeshData();

        if (meshData == null || meshData.Positions.Count == 0)
        {
            LogWarning("No mesh data to render");
            return;
        }

        GenerateMesh(meshData);
    }
    #endregion

    #region Private Methods - Mesh Generation
    /// <summary>DynamicMeshData → Unity Mesh 변환</summary>
    private void GenerateMesh(DynamicMeshData meshData)
    {
        _mesh.Clear();

        if (meshData.Positions.Count == 0)
            return;

        // 정점 데이터
        _mesh.SetVertices(meshData.Positions);
        _mesh.SetUVs(0, meshData.UVs);

        // 색상
        Color[] colors = new Color[meshData.Positions.Count];
        int gridSize = _foldingMesh.GridSize;

        for (int i = 0; i < colors.Length; i++)
        {
            Vector3 pos = meshData.Positions[i];
            int cellX = Mathf.Clamp(Mathf.FloorToInt(pos.x / _foldingMesh.CellSize), 0, gridSize - 1);
            int cellY = Mathf.Clamp(Mathf.FloorToInt(pos.y / _foldingMesh.CellSize), 0, gridSize - 1);
            int cellIndex = cellY * gridSize + cellX;
            colors[i] = GetCellColor(cellIndex, gridSize);
        }
        _mesh.SetColors(colors);

        // 활성 삼각형만 가져오기
        _foldingMesh.GetActiveTriangles(out List<int> frontTriangles, out List<int> backTriangles);

        _mesh.subMeshCount = 2;
        _mesh.SetTriangles(frontTriangles, 0);
        _mesh.SetTriangles(backTriangles, 1);

        _mesh.RecalculateNormals();
        _mesh.RecalculateBounds();

        Log($"Mesh updated: {meshData.Positions.Count} verts, {frontTriangles.Count / 3} front tris, {backTriangles.Count / 3} back tris");
    }

    /// <summary>셀 인덱스 기반 색상 계산</summary>
    private Color GetCellColor(int cellIndex, int gridSize)
    {
        float hue = (cellIndex / (float)(gridSize * gridSize)) % 1f;
        return Color.HSVToRGB(hue, 0.6f, 0.9f);
    }
    #endregion

    #region Debug - Gizmos
    private void OnDrawGizmos()
    {
        if (!_showVertexGizmos || _foldingMesh == null || _meshData == null)
            return;

        DrawVertexGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!_showVertexGizmos || _foldingMesh == null)
            return;

        // 그리드 경계 표시
        DrawGridBounds();
    }

    private void DrawVertexGizmos()
    {
        DynamicMeshData meshData = _foldingMesh.GetCurrentMeshData();
        if (meshData == null || meshData.Positions.Count == 0)
            return;

        Vector3 offset = _foldingMesh.MeshOriginOffset;

        for (int i = 0; i < meshData.Positions.Count; i++)
        {
            Vector3 worldPos = meshData.Positions[i] + offset;
            bool isFront = i < meshData.FrontFaces.Count && meshData.FrontFaces[i];

            Gizmos.color = isFront ? Color.red : Color.cyan;
            Gizmos.DrawSphere(worldPos, _gizmoSize);
        }
    }

    private void DrawGridBounds()
    {
        Vector3 offset = _foldingMesh.MeshOriginOffset;
        float size = _foldingMesh.GridWorldSize;

        Gizmos.color = Color.yellow;

        // 그리드 외곽선
        Vector3 bl = offset;
        Vector3 br = offset + new Vector3(size, 0, 0);
        Vector3 tr = offset + new Vector3(size, size, 0);
        Vector3 tl = offset + new Vector3(0, size, 0);

        Gizmos.DrawLine(bl, br);
        Gizmos.DrawLine(br, tr);
        Gizmos.DrawLine(tr, tl);
        Gizmos.DrawLine(tl, bl);
    }
    #endregion
    #region Private Methods - Logging
    private void Log(string msg, bool forcely = false)
    {
        if (_isDebugLogging || forcely)
            Debug.Log($"<color=magenta>[FoldingMeshRenderer]</color> {msg}", this);
    }

    private void LogWarning(string msg, bool forcely = false)
    {
        if (_isDebugLogging || forcely)
            Debug.LogWarning($"<color=yellow>[FoldingMeshRenderer]</color> {msg}", this);
    }

    private void LogError(string msg)
    {
        Debug.LogError($"<color=red>[FoldingMeshRenderer]</color> {msg}", this);
    }
    #endregion
}
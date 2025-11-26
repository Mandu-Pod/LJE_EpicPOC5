using UnityEngine;
using Sirenix.OdinInspector;

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
    #endregion

    #region Properties
    public Mesh CurrentMesh => _mesh;
    #endregion

    #region Private Fields
    private FoldingMesh _foldingMesh;
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
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

        // 정점 데이터
        _mesh.SetVertices(meshData.Positions);
        _mesh.SetUVs(0, meshData.UVs);

        // 색상 초기화 (모든 정점에 기본값)
        Color[] colors = new Color[meshData.Positions.Count];
        int gridSize = _foldingMesh.GridSize;

        // 기본 색상 설정
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.white;
        }

        // 삼각형 기반 색상 할당
        for (int i = 0; i < meshData.Triangles.Count; i += 3)
        {
            int idx0 = meshData.Triangles[i];
            int idx1 = meshData.Triangles[i + 1];
            int idx2 = meshData.Triangles[i + 2];

            // 인덱스 유효성 검증
            if (idx0 >= meshData.Positions.Count || idx1 >= meshData.Positions.Count || idx2 >= meshData.Positions.Count)
            {
                LogError($"Invalid triangle index: {idx0}, {idx1}, {idx2} (max: {meshData.Positions.Count - 1})");
                continue;
            }

            Vector3 pos = meshData.Positions[idx0];

            // 그리드 좌표 역산
            int cellX = Mathf.Clamp(Mathf.FloorToInt(pos.x / _foldingMesh.CellSize), 0, gridSize - 1);
            int cellY = Mathf.Clamp(Mathf.FloorToInt(pos.y / _foldingMesh.CellSize), 0, gridSize - 1);
            int cellIndex = cellY * gridSize + cellX;

            Color cellColor = GetCellColor(cellIndex, gridSize);

            colors[idx0] = cellColor;
            colors[idx1] = cellColor;
            colors[idx2] = cellColor;
        }

        _mesh.SetColors(colors);

        // 서브메시 분리 (앞면/뒷면)
        _mesh.subMeshCount = 2;

        var frontTriangles = new System.Collections.Generic.List<int>();
        var backTriangles = new System.Collections.Generic.List<int>();

        for (int i = 0; i < meshData.Triangles.Count; i += 3)
        {
            int vertIndex = meshData.Triangles[i];

            // 인덱스 유효성 재검증
            if (vertIndex >= meshData.FrontFaces.Count)
            {
                LogError($"Invalid FrontFaces index: {vertIndex} (max: {meshData.FrontFaces.Count - 1})");
                continue;
            }

            bool isFront = meshData.FrontFaces[vertIndex];

            if (isFront)
            {
                frontTriangles.Add(meshData.Triangles[i]);
                frontTriangles.Add(meshData.Triangles[i + 1]);
                frontTriangles.Add(meshData.Triangles[i + 2]);
            }
            else
            {
                backTriangles.Add(meshData.Triangles[i]);
                backTriangles.Add(meshData.Triangles[i + 1]);
                backTriangles.Add(meshData.Triangles[i + 2]);
            }
        }

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
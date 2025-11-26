using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>FoldingMesh 시스템 통합 관리</summary>
[RequireComponent(typeof(FoldingMesh), typeof(FoldingMeshRenderer))]
public class FoldingMeshSystem : MonoBehaviour
{
    #region Serialized Fields
    [TabGroup("Debug")]
    [SerializeField] private bool _isDebugLogging = true;
    #endregion

    #region Properties
    public FoldingMesh Mesh => _foldingMesh;
    public FoldingMeshRenderer Renderer => _renderer;
    #endregion

    #region Private Fields
    private FoldingMesh _foldingMesh;
    private FoldingMeshRenderer _renderer;
    #endregion

    #region Unity Lifecycle
    private void Awake() => Initialize();
    private void OnDestroy() => Cleanup();
    #endregion

    #region Initialization and Cleanup
    /// <summary>시스템 초기화 및 하위 컴포넌트 순차 초기화</summary>
    public void Initialize()
    {
        if (!ValidateComponents())
        {
            LogError("Component validation failed");
            return;
        }

        // 1단계: FoldingMesh 초기화
        _foldingMesh.Initialize();

        // 2단계: Renderer 초기화
        _renderer.Initialize();

        // 3단계: Renderer에 FoldingMesh 연결
        _renderer.LateInitialize(_foldingMesh);

        Log("System initialized successfully");
    }

    /// <summary>소멸 프로세스</summary>
    public void Cleanup()
    {
        if (_renderer != null)
        {
            _renderer.Cleanup();
        }

        if (_foldingMesh != null)
        {
            _foldingMesh.Cleanup();
        }

        Log("System cleaned up");
    }
    #endregion

    #region Public Methods - Query
    /// <summary>시스템 준비 상태 확인</summary>
    public bool IsReady()
    {
        return _foldingMesh != null && _renderer != null;
    }
    #endregion

    #region Private Methods - Component Setup
    /// <summary>필수 컴포넌트 탐색 및 검증</summary>
    private bool ValidateComponents()
    {
        _foldingMesh = GetComponent<FoldingMesh>();
        if (_foldingMesh == null)
        {
            LogError("FoldingMesh component not found");
            return false;
        }

        _renderer = GetComponent<FoldingMeshRenderer>();
        if (_renderer == null)
        {
            LogError("FoldingMeshRenderer component not found");
            return false;
        }

        return true;
    }
    #endregion

    #region Private Methods - Logging
    private void Log(string msg, bool forcely = false)
    {
        if (_isDebugLogging || forcely)
            Debug.Log($"<color=blue>[FoldingMeshSystem]</color> {msg}", this);
    }

    private void LogWarning(string msg, bool forcely = false)
    {
        if (_isDebugLogging || forcely)
            Debug.LogWarning($"<color=yellow>[FoldingMeshSystem]</color> {msg}", this);
    }

    private void LogError(string msg)
    {
        Debug.LogError($"<color=red>[FoldingMeshSystem]</color> {msg}", this);
    }
    #endregion
}
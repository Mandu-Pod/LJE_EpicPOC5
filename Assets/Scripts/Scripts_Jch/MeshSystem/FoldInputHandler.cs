using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>마우스 드래그 → 접기 입력 처리</summary>
public class FoldInputHandler : MonoBehaviour
{
    #region Serialized Fields
    [TabGroup("Setup"), Required]
    [SerializeField] private Camera _mainCamera;

    [TabGroup("Setup"), SuffixLabel("units")]
    [SerializeField] private float _minDragDistance = 0.5f;

    [TabGroup("Debug"), ReadOnly]
    [SerializeField] private bool _isDragging = false;

    [TabGroup("Debug")]
    [SerializeField] private bool _isDebugLogging = true;
    #endregion

    #region Properties
    public bool IsDragging => _isDragging;
    #endregion

    #region Private Fields
    private FoldingMesh _foldingMesh;
    private Vector2 _dragStartWorld;
    private Vector2 _dragCurrentWorld;
    #endregion

    #region Unity Lifecycle
    private void Awake() => Initialize();
    private void Update() => HandleInput();
    private void OnDestroy() => Cleanup();
    #endregion

    #region Initialization and Cleanup
    /// <summary>의존성 불필요한 내부 초기화</summary>
    public void Initialize()
    {
        if (_mainCamera == null)
            _mainCamera = Camera.main;

        _isDragging = false;

        Log("Initialized");
    }

    /// <summary>외부 의존성 필요한 초기화</summary>
    public void LateInitialize(FoldingMesh foldingMesh)
    {
        _foldingMesh = foldingMesh;

        if (_foldingMesh != null)
        {
            Log($"Late initialized with FoldingMesh");
        }
        else
        {
            LogError("FoldingMesh is null");
        }
    }

    /// <summary>소멸 프로세스</summary>
    public void Cleanup()
    {
        _foldingMesh = null;
        _isDragging = false;

        Log("Cleaned up");
    }
    #endregion

    #region Private Methods - Input Handling
    /// <summary>입력 처리</summary>
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnDragStart(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && _isDragging)
        {
            OnDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            OnDragEnd();
        }
    }

    /// <summary>드래그 시작</summary>
    private void OnDragStart(Vector2 screenPosition)
    {
        _dragStartWorld = ScreenToWorld(screenPosition);
        _dragCurrentWorld = _dragStartWorld;
        _isDragging = true;

        Log($"Drag started at {_dragStartWorld}");
    }

    /// <summary>드래그 중</summary>
    private void OnDrag(Vector2 screenPosition)
    {
        _dragCurrentWorld = ScreenToWorld(screenPosition);
    }

    /// <summary>드래그 종료</summary>
    private void OnDragEnd()
    {
        Vector2 dragDelta = _dragCurrentWorld - _dragStartWorld;
        float dragDistance = dragDelta.magnitude;

        if (dragDistance < _minDragDistance)
        {
            Log($"Drag too short: {dragDistance:F2} < {_minDragDistance:F2}");
            _isDragging = false;
            return;
        }

        Vector2 foldDirection = CalculateFoldDirection(_dragStartWorld, _dragCurrentWorld);

        bool success = _foldingMesh.TryFold(_dragStartWorld, _dragCurrentWorld, foldDirection);

        if (success)
        {
            Log($"Fold executed: {_dragStartWorld} → {_dragCurrentWorld}, direction {foldDirection}", true);
        }
        else
        {
            LogWarning("Fold failed");
        }

        _isDragging = false;
    }
    #endregion

    #region Private Methods - Coordinate Conversion
    /// <summary>스크린 → 월드 좌표 변환</summary>
    private Vector2 ScreenToWorld(Vector2 screenPosition)
    {
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_mainCamera.transform.position.z));
        return new Vector2(worldPos.x, worldPos.y);
    }
    #endregion

    #region Private Methods - Fold Direction
    /// <summary>드래그 방향 → 접는 방향 계산</summary>
    private Vector2 CalculateFoldDirection(Vector2 lineStart, Vector2 lineEnd)
    {
        // 접기선에 수직인 방향을 접는 방향으로
        Vector2 lineDir = (lineEnd - lineStart).normalized;
        Vector2 perpendicular = new Vector2(-lineDir.y, lineDir.x);

        return perpendicular;
    }
    #endregion

    #region Private Methods - Logging
    private void Log(string msg, bool forcely = false)
    {
        if (_isDebugLogging || forcely)
            Debug.Log($"<color=yellow>[FoldInputHandler]</color> {msg}", this);
    }

    private void LogWarning(string msg, bool forcely = false)
    {
        if (_isDebugLogging || forcely)
            Debug.LogWarning($"<color=yellow>[FoldInputHandler]</color> {msg}", this);
    }

    private void LogError(string msg)
    {
        Debug.LogError($"<color=red>[FoldInputHandler]</color> {msg}", this);
    }
    #endregion
}
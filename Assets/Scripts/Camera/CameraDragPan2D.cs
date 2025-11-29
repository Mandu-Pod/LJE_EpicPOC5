using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class CameraDragPan2D : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float dragSensitivity = 1f; // 1.0 = 1:1 이동

    [Header("Zoom")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomSensitivity = 1f; // 스크롤 1틱 당 감소할 size
    [SerializeField] private float minOrthoSize = 1f;
    [SerializeField] private float maxOrthoSize = 20f;
    [SerializeField] private bool zoomTowardsMouse = true;

    [Header("Bounds (optional)")]
    [SerializeField] private bool clampToBounds = false;
    [SerializeField] private Vector2 minBounds = new Vector2(-100f, -100f);
    [SerializeField] private Vector2 maxBounds = new Vector2(100f, 100f);

    private Vector3 dragOriginWorld;
    private bool isDragging;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null) targetCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (targetCamera == null) return;

        if (MiddleMouseDown())
        {
            isDragging = true;
            dragOriginWorld = GetMouseWorldPosition();
        }
        else if (MiddleMouseUp())
        {
            isDragging = false;
        }

        if (isDragging && MiddleMouseHeld())
        {
            Vector3 currentWorld = GetMouseWorldPosition();
            Vector3 delta = (dragOriginWorld - currentWorld) * dragSensitivity;

            Vector3 newPos = targetCamera.transform.position + delta;

            if (clampToBounds)
            {
                newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
                newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);
            }

            // Z는 유지 (2D 카메라 고정)
            newPos.z = targetCamera.transform.position.z;
            targetCamera.transform.position = newPos;
        }

        // Wheel Zoom (Orthographic)
        if (enableZoom && targetCamera.orthographic)
        {
            float scrollY = GetScrollDeltaY();
            if (Mathf.Abs(scrollY) > 0.0001f)
            {
                if (zoomTowardsMouse)
                {
                    Vector3 before = GetMouseWorldPosition();
                    float newSize = Mathf.Clamp(targetCamera.orthographicSize - scrollY * zoomSensitivity, minOrthoSize, maxOrthoSize);
                    targetCamera.orthographicSize = newSize;
                    Vector3 after = GetMouseWorldPosition();
                    Vector3 camDelta = before - after;

                    Vector3 newPos = targetCamera.transform.position + camDelta;
                    if (clampToBounds)
                    {
                        newPos.x = Mathf.Clamp(newPos.x, minBounds.x, maxBounds.x);
                        newPos.y = Mathf.Clamp(newPos.y, minBounds.y, maxBounds.y);
                    }
                    newPos.z = targetCamera.transform.position.z;
                    targetCamera.transform.position = newPos;
                }
                else
                {
                    float newSize = Mathf.Clamp(targetCamera.orthographicSize - scrollY * zoomSensitivity, minOrthoSize, maxOrthoSize);
                    targetCamera.orthographicSize = newSize;
                }
            }
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 sp = GetMouseScreenPosition();
        // 2D(Ortho)에서는 z가 무시되지만, 퍼스펙티브 호환을 위해 월드 0면까지의 거리로 설정
        sp.z = Mathf.Abs(targetCamera.transform.position.z);
        return targetCamera.ScreenToWorldPoint(sp);
    }

    private Vector3 GetMouseScreenPosition()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null ? (Vector3)Mouse.current.position.ReadValue() : Vector3.zero;
#else
        return Input.mousePosition;
#endif
    }

    private bool MiddleMouseDown()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.middleButton.wasPressedThisFrame;
#else
        return Input.GetMouseButtonDown(2);
#endif
    }

    private bool MiddleMouseUp()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.middleButton.wasReleasedThisFrame;
#else
        return Input.GetMouseButtonUp(2);
#endif
    }

    private bool MiddleMouseHeld()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null && Mouse.current.middleButton.isPressed;
#else
        return Input.GetMouseButton(2);
#endif
    }

    private float GetScrollDeltaY()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        return Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
#else
        return Input.mouseScrollDelta.y;
#endif
    }

    private void OnDisable()
    {
        isDragging = false;
    }
}

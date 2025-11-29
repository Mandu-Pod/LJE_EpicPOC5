using UnityEngine;

/// <summary>
/// 카메라 쉐이크 효과
/// </summary>
public class CameraShake : MonoBehaviour
{
    private static CameraShake instance;
    public static CameraShake Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("CameraShake");
                instance = obj.AddComponent<CameraShake>();
            }
            return instance;
        }
    }

    private Camera targetCamera;
    private Vector3 originalPosition;
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            if (targetCamera == null)
                targetCamera = Camera.main;

            if (targetCamera != null)
            {
                // 랜덤한 위치로 카메라 흔들기
                targetCamera.transform.localPosition = originalPosition + Random.insideUnitSphere * shakeMagnitude;

                shakeTimer -= Time.deltaTime;

                if (shakeTimer <= 0f)
                {
                    // 쉐이크 종료 - 원래 위치로 복원
                    targetCamera.transform.localPosition = originalPosition;
                    shakeTimer = 0f;
                }
            }
        }
    }

    /// <summary>
    /// 카메라 쉐이크 시작
    /// </summary>
    /// <param name="duration">지속 시간</param>
    /// <param name="magnitude">흔들림 강도</param>
    public void Shake(float duration, float magnitude)
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null)
        {
            originalPosition = targetCamera.transform.localPosition;
            shakeDuration = duration;
            shakeMagnitude = magnitude;
            shakeTimer = duration;
        }
    }

    /// <summary>
    /// 간단한 쉐이크 (기본 설정)
    /// </summary>
    public void ShakeDefault()
    {
        Shake(0.2f, 0.15f);
    }
}

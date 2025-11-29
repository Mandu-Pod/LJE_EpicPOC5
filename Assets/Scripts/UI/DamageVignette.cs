using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 플레이어 피격 시 화면 가장자리 빨간색 효과
/// </summary>
public class DamageVignette : MonoBehaviour
{
    private static DamageVignette instance;
    public static DamageVignette Instance
    {
        get
        {
            if (instance == null)
            {
                // Canvas 생성
                GameObject canvasObj = new GameObject("DamageVignetteCanvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // 최상위에 표시
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // 빨간색 이미지 생성
                GameObject imageObj = new GameObject("DamageVignetteImage");
                imageObj.transform.SetParent(canvasObj.transform, false);
                
                Image image = imageObj.AddComponent<Image>();
                image.color = new Color(1f, 0f, 0f, 0f); // 빨간색, 투명
                
                RectTransform rect = imageObj.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                
                // DamageVignette 컴포넌트 추가
                instance = imageObj.AddComponent<DamageVignette>();
                instance.vignetteImage = image;
                
                DontDestroyOnLoad(canvasObj);
            }
            return instance;
        }
    }

    [Header("효과 설정")]
    [SerializeField] private float flashDuration = 0.3f;
    [SerializeField] private float maxAlpha = 0.5f;
    
    private Image vignetteImage;
    private Coroutine currentFlashCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 빨간색 깜빡임 효과 실행
    /// </summary>
    public void Flash()
    {
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
        }
        currentFlashCoroutine = StartCoroutine(FlashCoroutine());
    }

    /// <summary>
    /// 깜빡임 코루틴
    /// </summary>
    private IEnumerator FlashCoroutine()
    {
        if (vignetteImage == null) yield break;

        float elapsed = 0f;
        Color color = vignetteImage.color;

        // 페이드 인 (빠르게)
        while (elapsed < flashDuration * 0.3f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, maxAlpha, elapsed / (flashDuration * 0.3f));
            color.a = alpha;
            vignetteImage.color = color;
            yield return null;
        }

        // 페이드 아웃 (천천히)
        elapsed = 0f;
        while (elapsed < flashDuration * 0.7f)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(maxAlpha, 0f, elapsed / (flashDuration * 0.7f));
            color.a = alpha;
            vignetteImage.color = color;
            yield return null;
        }

        // 완전히 투명하게
        color.a = 0f;
        vignetteImage.color = color;
        
        currentFlashCoroutine = null;
    }

    /// <summary>
    /// 효과 설정 변경
    /// </summary>
    public void SetFlashSettings(float duration, float alpha)
    {
        flashDuration = duration;
        maxAlpha = Mathf.Clamp01(alpha);
    }
}

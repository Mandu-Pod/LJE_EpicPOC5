using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 유닛의 HP 바 UI 표시 (선택사항)
/// </summary>
public class UnitHealthBar : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private Image healthBarBackground;
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("설정")]
    [SerializeField] private Vector3 offset = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector2 size = new Vector2(0.5f, 0.08f);

    private Unit unit;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        unit = GetComponentInParent<Unit>();

        if (canvas == null)
        {
            CreateHealthBar();
        }
    }

    private void CreateHealthBar()
    {
        // Canvas 생성
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = offset;

        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 200; // 모든 것 위에 표시

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = size;

        // 배경 생성
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localScale = Vector3.one;

        healthBarBackground = bgObj.AddComponent<Image>();
        healthBarBackground.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 채우기 생성
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localScale = Vector3.one;

        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = new Color(0f, 1f, 0f, 0.9f); // 초록색
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;

        // 텍스트 생성
        GameObject textObj = new GameObject("HealthText");
        textObj.transform.SetParent(canvasObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localScale = Vector3.one;

        healthText = textObj.AddComponent<TextMeshProUGUI>();
        healthText.alignment = TextAlignmentOptions.Center;
        healthText.fontSize = 0.15f;
        healthText.color = Color.white;
        healthText.fontStyle = FontStyles.Bold;
        healthText.enableAutoSizing = true;
        healthText.fontSizeMin = 0.1f;
        healthText.fontSizeMax = 0.2f;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
    }

    private void Update()
    {
        if (unit == null || healthBarFill == null) return;

        // HP 비율 계산
        float healthPercent = (float)unit.CurrentHP / unit.MaxHP;
        healthBarFill.fillAmount = healthPercent;

        // HP 텍스트 업데이트
        if (healthText != null)
        {
            healthText.text = $"{unit.CurrentHP}/{unit.MaxHP}";
        }

        // HP에 따라 색상 변경
        if (healthPercent > 0.6f)
            healthBarFill.color = new Color(0f, 1f, 0f, 0.9f); // 초록색
        else if (healthPercent > 0.3f)
            healthBarFill.color = new Color(1f, 0.92f, 0.016f, 0.9f); // 노란색
        else
            healthBarFill.color = new Color(1f, 0f, 0f, 0.9f); // 빨간색

        // 카메라를 향하도록 회전
        if (canvas != null && mainCamera != null)
        {
            canvas.transform.LookAt(canvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                     mainCamera.transform.rotation * Vector3.up);
        }

        // HP가 가득 차면 숨김
        if (healthBarFill.fillAmount >= 1f)
        {
            canvas.gameObject.SetActive(false);
        }
        else
        {
            canvas.gameObject.SetActive(true);
        }
    }
}

using UnityEngine;

public class Mark : MonoBehaviour
{
    public MarkType Type { get; private set; }
    public int Durability { get; private set; }

    public bool IsTool => Type.IsTool();
    public bool IsResource => Type.IsResource();
    public MarkCategory Category => Type.GetCategory();

    // 원래 위치 저장 (접기 취소 시 복원용)
    private Vector3 originalPosition;
    private bool isFlipped = false;

    // 아웃라인 관련
    private GameObject outlineObject;
    private SpriteRenderer outlineRenderer;
    private SpriteRenderer mainRenderer;

    [Header("아웃라인 설정")]
    private float outlineScale = 1.3f;
    private Color validCombineColor = Color.green;
    private Color invalidCombineColor = Color.red;

    public bool IsFlipped => isFlipped;
    public Vector3 OriginalPosition => originalPosition;

    public void Initialize(MarkType type, int durability = 0)
    {
        Type = type;
        Durability = IsTool ? durability : 0;
        originalPosition = transform.position;

        SetupOutline();
    }

    private void SetupOutline()
    {
        // 메인 렌더러 찾기
        mainRenderer = GetComponent<SpriteRenderer>();
        if (mainRenderer == null)
            mainRenderer = GetComponentInChildren<SpriteRenderer>();

        if (mainRenderer == null) return;

        // 마크가 종이 위에 보이도록 sortingOrder 설정
        mainRenderer.sortingOrder = 100; // 종이보다 위

        // 아웃라인 오브젝트 생성
        outlineObject = new GameObject("Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localScale = Vector3.one * outlineScale;

        // 아웃라인 렌더러 설정
        outlineRenderer = outlineObject.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = mainRenderer.sprite;
        outlineRenderer.sortingOrder = 99;  // 마크보다 뒤, 종이보다 앞
        outlineRenderer.color = Color.clear;  // 기본은 투명

        outlineObject.SetActive(false);
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 접힐 때 반사 위치로 이동
    /// </summary>
    public void MoveToFlippedPosition(Vector2 reflectedPosition)
    {
        if (!isFlipped)
        {
            originalPosition = transform.position;
        }
        isFlipped = true;
        transform.position = new Vector3(reflectedPosition.x, reflectedPosition.y, transform.position.z);
    }

    /// <summary>
    /// 접기 취소 시 원래 위치로 복원
    /// </summary>
    public void RestoreOriginalPosition()
    {
        if (isFlipped)
        {
            transform.position = originalPosition;
            isFlipped = false;
        }
        HideOutline();
    }

    /// <summary>
    /// 접기 확정 시 현재 위치를 새 원래 위치로 설정
    /// </summary>
    public void ConfirmPosition()
    {
        originalPosition = transform.position;
        isFlipped = false;
        HideOutline();
    }

    /// <summary>
    /// 유효한 조합 아웃라인 표시 (초록)
    /// </summary>
    public void ShowValidOutline()
    {
        ShowOutline(validCombineColor);
    }

    /// <summary>
    /// 무효한 조합 아웃라인 표시 (빨강)
    /// </summary>
    public void ShowInvalidOutline()
    {
        ShowOutline(invalidCombineColor);
    }

    private void ShowOutline(Color color)
    {
        if (outlineObject == null || outlineRenderer == null) return;

        outlineObject.SetActive(true);
        outlineRenderer.color = color;
    }

    /// <summary>
    /// 아웃라인 숨기기
    /// </summary>
    public void HideOutline()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }

    /// <summary>
    /// 도구 사용 시 내구도 감소. 0이 되면 true 반환 (파괴 필요)
    /// </summary>
    public bool UseTool()
    {
        if (!IsTool) return false;

        Durability--;
        return Durability <= 0;
    }
}
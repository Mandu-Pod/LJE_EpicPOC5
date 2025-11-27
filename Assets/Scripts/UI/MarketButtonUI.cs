using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MarketButtonUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("아이콘 설정 (선택사항)")]
    [SerializeField] private Image iconImage;

    [Header("Cost Text 색상 설정")]
    [SerializeField] private Color affordableColor = new Color(0.2f, 0.6f, 0.2f, 1f);     // 어두운 초록색
    [SerializeField] private Color unaffordableColor = new Color(0.6f, 0.2f, 0.2f, 1f);   // 어두운 붉은색

    [Header("현재 아이템 데이터")]
    private MarketItemSO currentItem;

    public MarketItemSO CurrentItem => currentItem;

    /// <summary>
    /// MarketItemSO 데이터를 받아서 UI 업데이트
    /// </summary>
    public void SetItemData(MarketItemSO item)
    {
        if (item == null)
        {
            Debug.LogWarning("[MarketButtonUI] 아이템 데이터가 null입니다.");
            return;
        }

        currentItem = item;
        UpdateUI();
    }

    /// <summary>
    /// UI 요소들 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (currentItem == null) return;

        // 아이템 이름
        if (itemNameText != null)
        {
            itemNameText.text = currentItem.itemName;
        }

        // 비용
        if (costText != null)
        {
            costText.text = $"coin * {currentItem.cost}";
        }

        // 설명
        if (descriptionText != null)
        {
            descriptionText.text = currentItem.description;
        }

        // 아이콘 (선택사항)
        if (iconImage != null)
        {
            // 추후 MarketItemSO에 Sprite 필드 추가 시 사용
            // iconImage.sprite = currentItem.icon;
        }
    }

    /// <summary>
    /// 구매 가능 여부에 따라 버튼 상태 업데이트
    /// </summary>
    public void UpdateButtonState(int currentCoins)
    {
        if (currentItem == null) return;

        Button button = GetComponent<Button>();
        if (button == null) return;

        // 코인이 부족하면 버튼 비활성화
        bool canAfford = currentCoins >= currentItem.cost;
        button.interactable = canAfford;

        // Cost Text 색상 변경 - 어두운 초록/붉은색
        if (costText != null)
        {
            costText.color = canAfford ? affordableColor : unaffordableColor;
        }
    }
}
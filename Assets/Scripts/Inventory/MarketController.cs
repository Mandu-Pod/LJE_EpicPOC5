using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MarketController : SingletonObject<MarketController>
{
    [Header("UI 설정")]
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Transform buttonContainer;

    [Header("마켓 아이템 목록")]
    [SerializeField] private List<MarketItemSO> marketItems;

    private List<GameObject> createdButtons = new List<GameObject>();
    private List<MarketButtonUI> buttonUIList = new List<MarketButtonUI>();

    private void Start()
    {
        GenerateMarketButtons();
    }

    private void OnEnable()
    {
        InventorySystem.OnInventoryChanged += UpdateAllButtonStates;
    }

    private void OnDisable()
    {
        InventorySystem.OnInventoryChanged -= UpdateAllButtonStates;
    }

    private void GenerateMarketButtons()
    {
        if (buttonPrefab == null || buttonContainer == null)
        {
            Debug.LogError("[마켓] 버튼 프리팹 또는 컨테이너가 설정되지 않았습니다.");
            return;
        }

        ClearButtons();

        foreach (var item in marketItems)
        {
            if (item == null) continue;

            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            createdButtons.Add(buttonObj);

            MarketButtonUI buttonUI = buttonObj.GetComponent<MarketButtonUI>();
            if (buttonUI == null)
            {
                Debug.LogWarning("[마켓] 버튼 프리팹에 MarketButtonUI 컴포넌트가 없습니다.");
                buttonUI = buttonObj.AddComponent<MarketButtonUI>();
            }

            buttonUI.SetItemData(item);
            buttonUIList.Add(buttonUI);

            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                MarketItemSO currentItem = item;
                button.onClick.AddListener(() => OnPurchaseButtonClicked(currentItem));
            }
        }

        Debug.Log($"[마켓] {marketItems.Count}개의 아이템 버튼 생성 완료");

        UpdateAllButtonStates();
    }

    private void UpdateAllButtonStates()
    {
        if (InventorySystem.Instance == null) return;

        int currentCoins = InventorySystem.Instance.GetItemCount(MarkType.Coin);

        foreach (var buttonUI in buttonUIList)
        {
            if (buttonUI != null)
            {
                buttonUI.UpdateButtonState(currentCoins);
            }
        }
    }

    public void OnPurchaseButtonClicked(MarketItemSO item)
    {
        if (item == null)
        {
            Debug.LogWarning("[마켓] 아이템 정보가 없습니다.");
            return;
        }

        int currentCoins = InventorySystem.Instance.GetItemCount(MarkType.Coin);

        if (currentCoins < item.cost)
        {
            Debug.Log($"[마켓] 코인 부족! (보유: {currentCoins}, 필요: {item.cost})");
            return;
        }

        bool success = InventorySystem.Instance.RemoveItem(MarkType.Coin, item.cost);

        if (!success)
        {
            Debug.LogError("[마켓] 코인 차감 실패");
            return;
        }

        ExecuteItemAction(item);

        Debug.Log($"[마켓] '{item.itemName}' 구매 완료! (비용: {item.cost} 코인)");
    }

    private void ExecuteItemAction(MarketItemSO item)
    {
        switch (item.itemType)
        {
            case MarketItemType.NewPaperForest:
                if (PaperController.Instance != null)
                {
                    PaperController.Instance.CreateNewPaper(PaperType.Forest);
                    Debug.Log("[마켓] 새로운 숲 종이 생성됨");
                }
                break;

            case MarketItemType.NewPaperPlains:
                if (PaperController.Instance != null)
                {
                    PaperController.Instance.CreateNewPaper(PaperType.Plains);
                    Debug.Log("[마켓] 새로운 평지 종이 생성됨");
                }
                break;

            default:
                Debug.LogWarning($"[마켓] 알 수 없는 아이템 타입: {item.itemType}");
                break;
        }
    }

    private void ClearButtons()
    {
        foreach (var button in createdButtons)
        {
            if (button != null)
                Destroy(button);
        }
        createdButtons.Clear();
        buttonUIList.Clear();
    }

    public void RefreshMarket()
    {
        GenerateMarketButtons();
    }
}
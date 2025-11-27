using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySellUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform sellButtonContainer;
    [SerializeField] private GameObject sellButtonPrefab;

    [Header("Sell Price Settings")]
    [SerializeField] private int treeSellPrice = 1;
    [SerializeField] private int stoneSellPrice = 1;
    [SerializeField] private int woodSellPrice = 2;
    [SerializeField] private int foodSellPrice = 1;


    private List<GameObject> sellButtons = new List<GameObject>();
    private Dictionary<MarkType, int> sellPrices = new Dictionary<MarkType, int>();

    private void Start()
    {
        InitializeSellPrices();
        GenerateSellButtons();
    }

    private void OnEnable()
    {
        InventorySystem.OnInventoryChanged += GenerateSellButtons;
    }

    private void OnDisable()
    {
        InventorySystem.OnInventoryChanged -= GenerateSellButtons;
    }

    private void InitializeSellPrices()
    {
        sellPrices[MarkType.Tree] = treeSellPrice;
        sellPrices[MarkType.Stone] = stoneSellPrice;
        sellPrices[MarkType.Wood] = woodSellPrice;
        sellPrices[MarkType.Food] = foodSellPrice;
    }

    private void GenerateSellButtons()
    {
        ClearButtons();

        if (InventorySystem.Instance == null || sellButtonPrefab == null || sellButtonContainer == null)
            return;

        Dictionary<MarkType, int> items = InventorySystem.Instance.GetAllItems();

        foreach (var item in items)
        {
            if (!CanSellItem(item.Key))
                continue;

            if (item.Value <= 0)
                continue;

            CreateSellButton(item.Key, item.Value);
        }
    }

    private bool CanSellItem(MarkType type)
    {
        return type != MarkType.Person && type != MarkType.Coin;
    }

    private void CreateSellButton(MarkType itemType, int itemCount)
    {
        if (!sellPrices.ContainsKey(itemType))
        {
            Debug.LogWarning($"[Sell UI] {itemType} has no sell price set");
            return;
        }

        GameObject buttonObj = Instantiate(sellButtonPrefab, sellButtonContainer);
        sellButtons.Add(buttonObj);

        TextMeshProUGUI[] textComponents = buttonObj.GetComponentsInChildren<TextMeshProUGUI>();

        if (textComponents.Length >= 2)
        {
            textComponents[0].text = $"{GetItemName(itemType)} x{itemCount}";
            textComponents[1].text = $"Sell: {sellPrices[itemType]}";
        }

        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnSellButtonClicked(itemType));
        }
    }

    private void OnSellButtonClicked(MarkType itemType)
    {
        if (InventorySystem.Instance == null)
            return;

        int currentCount = InventorySystem.Instance.GetItemCount(itemType);
        if (currentCount <= 0)
        {
            Debug.Log($"[Sell] No {itemType} to sell");
            return;
        }

        if (!sellPrices.ContainsKey(itemType))
        {
            Debug.LogWarning($"[Sell] {itemType} cannot be sold");
            return;
        }

        bool removed = InventorySystem.Instance.RemoveItem(itemType, 1);
        if (removed)
        {
            int sellPrice = sellPrices[itemType];
            InventorySystem.Instance.AddItem(MarkType.Coin, sellPrice);
            Debug.Log($"[Sell] Sold {itemType} for {sellPrice} Coin");
        }
    }

    private void ClearButtons()
    {
        foreach (var button in sellButtons)
        {
            if (button != null)
                Destroy(button);
        }
        sellButtons.Clear();
    }

    private string GetItemName(MarkType type)
    {
        return type switch
        {
            MarkType.Tree => "트리",
            MarkType.Stone => "돌",
            MarkType.Wood => "나무",
            MarkType.Food => "식량",
            _ => "Unknown"
        };
    }
}
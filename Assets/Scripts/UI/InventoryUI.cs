using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Transform itemContainer;
    [SerializeField] private GameObject itemSlotPrefab;

    [Header("텍스트 (간단 버전)")]
    [SerializeField] private TextMeshProUGUI inventoryText;

    [Header("아이콘 설정")]
    [SerializeField] private string treeIcon = "T";
    [SerializeField] private string stoneIcon = "S";
    [SerializeField] private string woodIcon = "W";
    [SerializeField] private string foodIcon = "F";
    [SerializeField] private string personIcon = "P";
    [SerializeField] private string coinIcon = "C";
    [SerializeField] private string unknownIcon = "?";

    private void OnEnable()
    {
        InventorySystem.OnInventoryChanged += UpdateUI;
    }

    private void OnDisable()
    {
        InventorySystem.OnInventoryChanged -= UpdateUI;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (InventorySystem.Instance == null) return;

        Dictionary<MarkType, int> items = InventorySystem.Instance.GetAllItems();

        if (inventoryText != null)
        {
            string text = "<b>인벤토리</b>\n";

            if (items.Count == 0)
            {
                text += "(비어있음)";
            }
            else
            {
                foreach (var item in items)
                {
                    text += $"{GetItemIcon(item.Key)} {GetItemName(item.Key)}: {item.Value}\n";
                }
            }

            inventoryText.text = text;
        }
    }

    private string GetItemIcon(MarkType type)
    {
        return type switch
        {
            MarkType.Tree => treeIcon,
            MarkType.Stone => stoneIcon,
            MarkType.Wood => woodIcon,
            MarkType.Food => foodIcon,
            MarkType.Person => personIcon,
            MarkType.Coin => coinIcon,
            _ => unknownIcon
        };
    }

    private string GetItemName(MarkType type)
    {
        return type switch
        {
            MarkType.Tree => "나무",
            MarkType.Stone => "돌",
            MarkType.Wood => "목재",
            MarkType.Food => "식량",
            MarkType.Person => "인구",
            MarkType.Coin => "코인",
            _ => "알 수 없음"
        };
    }
}
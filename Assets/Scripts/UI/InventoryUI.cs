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
    [SerializeField] private string treeIcon = "🌲";
    [SerializeField] private string woodIcon = "🪵";
    [SerializeField] private string axeIcon = "🪓";
    [SerializeField] private string pickaxeIcon = "⛏️";
    [SerializeField] private string unknownIcon = "❓";
    
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
        
        // 간단 텍스트 버전
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
            MarkType.Wood => woodIcon,
            MarkType.Axe => axeIcon,
            MarkType.Pickaxe => pickaxeIcon,
            _ => unknownIcon
        };
    }
    
    private string GetItemName(MarkType type)
    {
        return type switch
        {
            MarkType.Tree => "나무",
            MarkType.Wood => "목재",
            MarkType.Axe => "도끼",
            MarkType.Pickaxe => "곡괭이",
            MarkType.Token => "토큰",
            _ => "알 수 없음"
        };
    }
}

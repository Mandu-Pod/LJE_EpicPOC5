using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : SingletonObject<InventorySystem>
{
    public static event Action OnInventoryChanged;
    
    private Dictionary<MarkType, int> items = new Dictionary<MarkType, int>();
    
    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(MarkType type, int amount = 1)
    {
        if (items.ContainsKey(type))
            items[type] += amount;
        else
            items[type] = amount;
        
        Debug.Log($"[인벤토리] {type} +{amount} (현재: {items[type]})");
        OnInventoryChanged?.Invoke();
    }
    
    /// <summary>
    /// 아이템 제거. 성공 시 true 반환
    /// </summary>
    public bool RemoveItem(MarkType type, int amount = 1)
    {
        if (!items.ContainsKey(type) || items[type] < amount)
            return false;
        
        items[type] -= amount;
        
        if (items[type] <= 0)
            items.Remove(type);
        
        Debug.Log($"[인벤토리] {type} -{amount}");
        OnInventoryChanged?.Invoke();
        return true;
    }
    
    /// <summary>
    /// 아이템 개수 확인
    /// </summary>
    public int GetItemCount(MarkType type)
    {
        return items.ContainsKey(type) ? items[type] : 0;
    }
    
    /// <summary>
    /// 전체 아이템 목록 반환
    /// </summary>
    public Dictionary<MarkType, int> GetAllItems()
    {
        return new Dictionary<MarkType, int>(items);
    }
}
